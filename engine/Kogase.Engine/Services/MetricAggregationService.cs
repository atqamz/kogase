using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Kogase.Engine.Data;
using Kogase.Engine.Models;
using System.Text.Json;
using Polly;
using Npgsql;

namespace Kogase.Engine.Services
{
    public class MetricAggregationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MetricAggregationService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(1);
        private readonly Polly.Retry.AsyncRetryPolicy _retryPolicy;

        public MetricAggregationService(
            IServiceProvider serviceProvider,
            ILogger<MetricAggregationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            
            // Define a retry policy for transient database errors
            _retryPolicy = Policy
                .Handle<NpgsqlException>()
                .Or<InvalidOperationException>()
                .WaitAndRetryAsync(
                    retryCount: 5,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            "Retry {RetryCount} for {Context} after {Delay}s due to: {Message}",
                            retryCount,
                            context?.OperationKey ?? "database operation",
                            timeSpan.TotalSeconds,
                            exception.Message);
                    });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MetricAggregationService started");

            // Initial delay to allow the application to fully start up
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Use the retry policy when aggregating metrics
                    await _retryPolicy.ExecuteAsync(async () => await AggregateMetricsAsync());
                    
                    // Wait for the next interval
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (Exception ex) when (!(ex is TaskCanceledException))
                {
                    _logger.LogError(ex, "Error occurred while aggregating metrics");
                    
                    // Wait a shorter time and try again
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }

        private async Task AggregateMetricsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<KogaseDbContext>();
            
            _logger.LogInformation("Starting metrics aggregation");
            
            try
            {
                // Test database connectivity before proceeding
                bool isConnected = await TestDatabaseConnectionAsync(dbContext);
                if (!isConnected)
                {
                    throw new InvalidOperationException("Unable to connect to the database after multiple attempts");
                }
                
                // Get all active projects
                var projects = await dbContext.Projects.ToListAsync();
                
                foreach (var project in projects)
                {
                    await AggregateProjectMetricsAsync(dbContext, project.Id);
                }
                
                _logger.LogInformation("Metrics aggregation completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during metrics aggregation");
                throw;
            }
        }
        
        private async Task<bool> TestDatabaseConnectionAsync(KogaseDbContext dbContext)
        {
            try
            {
                // Run a simple query to check connection
                await dbContext.Database.CanConnectAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Database connection test failed");
                return false;
            }
        }

        private async Task AggregateProjectMetricsAsync(KogaseDbContext dbContext, int projectId)
        {
            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);
            var thisMonth = new DateTime(today.Year, today.Month, 1);
            
            try
            {
                // Aggregate DAU (Daily Active Users)
                await AggregateDailyActiveUsersAsync(dbContext, projectId, today);
                
                // Aggregate MAU (Monthly Active Users)
                await AggregateMonthlyActiveUsersAsync(dbContext, projectId, thisMonth);
                
                // Aggregate session metrics
                await AggregateSessionMetricsAsync(dbContext, projectId, today);
                
                // Add additional metric aggregations as needed
                
                _logger.LogInformation("Completed metric aggregation for project {ProjectId}", projectId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error aggregating metrics for project {ProjectId}", projectId);
            }
        }

        private async Task AggregateDailyActiveUsersAsync(KogaseDbContext dbContext, int projectId, DateTime date)
        {
            try
            {
                // Find distinct device IDs for the day
                var deviceIds = await dbContext.Events
                    .Where(e => e.ProjectId == projectId && e.Timestamp.Date == date && e.DeviceId != null)
                    .Select(e => e.DeviceId)
                    .Distinct()
                    .ToListAsync();
                
                var dauCount = deviceIds.Count;
                
                // Check if metric already exists
                var existingMetric = await dbContext.Metrics
                    .FirstOrDefaultAsync(m => 
                        m.ProjectId == projectId && 
                        m.MetricType == "dau" && 
                        m.Period == "daily" &&
                        m.PeriodStart.Date == date);
                
                if (existingMetric != null)
                {
                    // Update existing metric
                    existingMetric.Value = dauCount;
                    existingMetric.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // Create new metric
                    var metric = new Metric
                    {
                        ProjectId = projectId,
                        MetricType = "dau",
                        Period = "daily",
                        PeriodStart = date,
                        Value = dauCount,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    dbContext.Metrics.Add(metric);
                }
                
                await dbContext.SaveChangesAsync();
                
                _logger.LogInformation("Aggregated DAU for project {ProjectId}: {Value}", 
                    projectId, dauCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating DAU for project {ProjectId}", projectId);
                throw;
            }
        }

        private async Task AggregateMonthlyActiveUsersAsync(KogaseDbContext dbContext, int projectId, DateTime monthStart)
        {
            try
            {
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                
                // Find distinct device IDs for the month
                var deviceIds = await dbContext.Events
                    .Where(e => 
                        e.ProjectId == projectId && 
                        e.Timestamp >= monthStart && 
                        e.Timestamp <= monthEnd &&
                        e.DeviceId != null)
                    .Select(e => e.DeviceId)
                    .Distinct()
                    .ToListAsync();
                
                var mauCount = deviceIds.Count;
                
                // Check if metric already exists
                var existingMetric = await dbContext.Metrics
                    .FirstOrDefaultAsync(m => 
                        m.ProjectId == projectId && 
                        m.MetricType == "mau" && 
                        m.Period == "monthly" &&
                        m.PeriodStart.Date == monthStart);
                
                if (existingMetric != null)
                {
                    // Update existing metric
                    existingMetric.Value = mauCount;
                    existingMetric.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // Create new metric
                    var metric = new Metric
                    {
                        ProjectId = projectId,
                        MetricType = "mau",
                        Period = "monthly",
                        PeriodStart = monthStart,
                        Value = mauCount,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    dbContext.Metrics.Add(metric);
                }
                
                await dbContext.SaveChangesAsync();
                
                _logger.LogInformation("Aggregated MAU for project {ProjectId}: {Value}", 
                    projectId, mauCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating MAU for project {ProjectId}", projectId);
                throw;
            }
        }

        private async Task AggregateSessionMetricsAsync(KogaseDbContext dbContext, int projectId, DateTime date)
        {
            try
            {
                // Get completed sessions for the day
                var sessions = await dbContext.Sessions
                    .Include(s => s.Device)
                    .Where(s => 
                        s.Device != null && 
                        s.Device.ProjectId == projectId && 
                        s.EndTime != null && 
                        s.Duration != null &&
                        s.StartTime.Date == date)
                    .ToListAsync();
                
                if (sessions.Any())
                {
                    // Calculate average session duration
                    double averageSessionDuration = sessions
                        .Where(s => s.Duration.HasValue)
                        .Select(s => s.Duration ?? 0)
                        .DefaultIfEmpty(0)
                        .Average();
                    
                    // Save session count metric
                    var sessionCountMetric = await dbContext.Metrics
                        .FirstOrDefaultAsync(m => 
                            m.ProjectId == projectId && 
                            m.MetricType == "session_count" && 
                            m.Period == "daily" &&
                            m.PeriodStart.Date == date);
                    
                    if (sessionCountMetric != null)
                    {
                        sessionCountMetric.Value = sessions.Count;
                        sessionCountMetric.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        sessionCountMetric = new Metric
                        {
                            ProjectId = projectId,
                            MetricType = "session_count",
                            Period = "daily",
                            PeriodStart = date,
                            Value = sessions.Count,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        
                        dbContext.Metrics.Add(sessionCountMetric);
                    }
                    
                    // Save average session duration metric
                    var sessionDurationMetric = await dbContext.Metrics
                        .FirstOrDefaultAsync(m => 
                            m.ProjectId == projectId && 
                            m.MetricType == "avg_session_duration" && 
                            m.Period == "daily" &&
                            m.PeriodStart.Date == date);
                    
                    if (sessionDurationMetric != null)
                    {
                        sessionDurationMetric.Value = averageSessionDuration;
                        sessionDurationMetric.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        sessionDurationMetric = new Metric
                        {
                            ProjectId = projectId,
                            MetricType = "avg_session_duration",
                            Period = "daily",
                            PeriodStart = date,
                            Value = averageSessionDuration,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        
                        dbContext.Metrics.Add(sessionDurationMetric);
                    }
                    
                    await dbContext.SaveChangesAsync();
                    
                    _logger.LogInformation("Aggregated session metrics for project {ProjectId}: {Count} sessions, {Duration}s avg duration", 
                        projectId, sessions.Count, averageSessionDuration);
                }
                
                // Extract platform from events if available
                var events = await dbContext.Events
                    .Include(e => e.Device)
                    .Where(e => e.ProjectId == projectId && e.Timestamp.Date == date)
                    .ToListAsync();
                
                if (events.Count > 0)
                {
                    // Group events by platform
                    var platformCounts = events
                        .Where(e => e.Device != null)
                        .Select(e => e.Device!.Platform)
                        .Where(platform => !string.IsNullOrEmpty(platform))
                        .GroupBy(platform => platform)
                        .Select(g => new { Platform = g.Key ?? string.Empty, Count = g.Count() })
                        .ToDictionary(x => x.Platform, x => x.Count);
                    
                    // Add platform breakdown to DAU metric
                    if (platformCounts.Any())
                    {
                        var dailyMetric = await dbContext.Metrics
                            .FirstOrDefaultAsync(m => 
                                m.ProjectId == projectId && 
                                m.MetricType == "dau" && 
                                m.Period == "daily" &&
                                m.PeriodStart.Date == date);
                        
                        if (dailyMetric != null)
                        {
                            dailyMetric.Dimensions = JsonSerializer.Serialize(new { platforms = platformCounts });
                            await dbContext.SaveChangesAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating session metrics for project {ProjectId}", projectId);
                throw;
            }
        }
    }
} 
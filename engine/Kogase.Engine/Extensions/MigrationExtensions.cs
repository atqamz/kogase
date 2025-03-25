using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace Kogase.Engine.Extensions
{
    /// <summary>
    /// Extensions for database migrations and initialization
    /// </summary>
    public static class MigrationExtensions
    {
        /// <summary>
        /// Check if the migrations history table exists in the database
        /// </summary>
        public static bool MigrationsHistoryTableExists(this DatabaseFacade database)
        {
            try
            {
                // Try to get the migrations history table
                var historyRepository = database.GetService<IHistoryRepository>();
                var exists = historyRepository.Exists();
                return exists;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Safely apply migrations with proper error handling for tables-already-exist scenarios
        /// </summary>
        public static void SafeMigrate(this DatabaseFacade database, ILogger logger)
        {
            try
            {
                // Check if we can connect to the database
                if (!database.CanConnect())
                {
                    logger.LogInformation("Cannot connect to database, creating it");
                    database.EnsureCreated();
                    return;
                }

                // Check if the migrations history table exists
                if (!database.MigrationsHistoryTableExists())
                {
                    // If database exists but migrations history table doesn't, we need to handle this specially
                    try
                    {
                        logger.LogInformation("Database exists but migrations history table doesn't. Creating migrations history table.");
                        
                        // Create the migrations history table
                        var migrator = database.GetService<IMigrator>();
                        migrator.Migrate();
                    }
                    catch (Exception ex) when (IsTableExistsException(ex))
                    {
                        logger.LogWarning(ex, "Tables already exist but migrations history table doesn't. " +
                            "This could happen if the database was created without using migrations.");
                        
                        // We'll try to create just the migrations history table and record the initial migration
                        RecordMigrationManually(database, logger);
                    }
                }
                else
                {
                    // Normal migration scenario
                    logger.LogInformation("Applying pending migrations");
                    database.Migrate();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during migration");
                throw;
            }
        }

        private static bool IsTableExistsException(Exception ex)
        {
            // Check if the exception is related to tables already existing
            return ex.ToString().Contains("already exists");
        }

        private static void RecordMigrationManually(DatabaseFacade database, ILogger logger)
        {
            try
            {
                // Get all migrations that should be recorded
                var migrationsAssembly = database.GetService<IMigrationsAssembly>();
                var migrations = migrationsAssembly.Migrations.Keys;
                
                logger.LogInformation("Attempting to record {Count} migrations as completed: {Migrations}", 
                    migrations.Count(), string.Join(", ", migrations));

                // Try to manually create the migrations history table and record migrations
                var historyRepository = database.GetService<IHistoryRepository>();
                var creationScript = historyRepository.GetCreateScript();
                
                using (var connection = database.GetDbConnection())
                {
                    if (connection.State != System.Data.ConnectionState.Open)
                    {
                        connection.Open();
                    }
                    
                    // Create the migrations history table
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = creationScript;
                        command.ExecuteNonQuery();
                    }
                    
                    // Add each migration to the history
                    foreach (var migration in migrations)
                    {
                        string productVersion = typeof(MigrationExtensions).Assembly.GetName().Version.ToString();
                        
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = $"INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ('{migration}', '{productVersion}');";
                            try
                            {
                                command.ExecuteNonQuery();
                                logger.LogInformation("Recorded migration {Migration}", migration);
                            }
                            catch (Exception ex)
                            {
                                logger.LogWarning(ex, "Failed to record migration {Migration}", migration);
                            }
                        }
                    }
                }
                
                logger.LogInformation("Successfully recorded migrations as completed");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to record migrations manually");
            }
        }
    }
} 
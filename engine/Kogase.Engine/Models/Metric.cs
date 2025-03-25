using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kogase.Engine.Models
{
    public class Metric
    {
        [Key]
        public long Id { get; set; }
        
        [Required]
        public int ProjectId { get; set; }
        
        [ForeignKey("ProjectId")]
        public Project? Project { get; set; }
        
        [Required]
        public string MetricType { get; set; } = string.Empty; // "dau", "mau", "session_length", etc.
        
        [Required]
        public string Period { get; set; } = string.Empty; // "hourly", "daily", "weekly", "monthly", "yearly", "total"
        
        [Required]
        public DateTime PeriodStart { get; set; }
        
        [Required]
        public double Value { get; set; }
        
        public string? Dimensions { get; set; }

        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
    }
} 
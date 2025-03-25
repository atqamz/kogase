using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Kogase.Engine.Models
{
    public class Event
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ProjectId { get; set; }
        
        [ForeignKey("ProjectId")]
        public Project? Project { get; set; }
        
        public int? DeviceId { get; set; }
        
        [ForeignKey("DeviceId")]
        public Device? Device { get; set; }
        
        [Required]
        [StringLength(50)]
        public string EventType { get; set; } = null!;
        
        [Required]
        [StringLength(100)]
        public string EventName { get; set; } = null!;
        
        public string? Parameters { get; set; }
        
        [Required]
        public DateTime Timestamp { get; set; }
    }
} 
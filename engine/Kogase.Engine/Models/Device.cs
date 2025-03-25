using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kogase.Engine.Models
{
    public class Device
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ProjectId { get; set; }
        
        [ForeignKey("ProjectId")]
        public Project? Project { get; set; }
        
        [Required]
        [StringLength(100)]
        public string DeviceId { get; set; } = null!;
        
        [Required]
        [StringLength(50)]
        public string Platform { get; set; } = null!;
        
        [Required]
        [StringLength(50)]
        public string OsVersion { get; set; } = null!;
        
        [Required]
        [StringLength(50)]
        public string AppVersion { get; set; } = null!;
        
        [Required]
        public DateTime FirstSeen { get; set; }
        
        [Required]
        public DateTime LastSeen { get; set; }
        
        public string? Country { get; set; }

        public ICollection<Session> Sessions { get; set; } = new List<Session>();
        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
} 
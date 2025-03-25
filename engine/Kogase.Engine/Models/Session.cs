using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kogase.Engine.Models
{
    public class Session
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DeviceId { get; set; }

        [Required]
        [StringLength(50)]
        public string SessionId { get; set; } = null!;

        [Required]
        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }
        
        public int? Duration { get; set; }

        [ForeignKey("DeviceId")]
        public Device? Device { get; set; }
    }
} 
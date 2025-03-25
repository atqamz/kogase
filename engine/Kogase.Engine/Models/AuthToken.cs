using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kogase.Engine.Models
{
    public class AuthToken
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [ForeignKey("UserId")]
        public User? User { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Token { get; set; } = null!;
        
        public DateTime CreatedAt { get; set; }
        
        [Required]
        public DateTime ExpiresAt { get; set; }
        
        public DateTime? LastUsedAt { get; set; }
    }
} 
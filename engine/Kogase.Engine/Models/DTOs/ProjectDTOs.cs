using System;

namespace Kogase.Engine.Models.DTOs
{
    public class ProjectCreateRequest
    {
        public string Name { get; set; } = null!;
    }

    public class ProjectResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string ApiKey { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string OwnerName { get; set; } = null!;
    }

    public class ApiKeyResponse
    {
        public string ApiKey { get; set; } = null!;
    }

    public class ProjectMemberRequest
    {
        public int UserId { get; set; }
        public string Role { get; set; } = null!;
    }

    public class ProjectMemberResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
} 
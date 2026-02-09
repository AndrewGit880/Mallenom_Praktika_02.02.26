using System;
using System.ComponentModel.DataAnnotations;

namespace SimpleBlog.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string AuthorId { get; set; } = string.Empty;
        public virtual ApplicationUser Author { get; set; } = null!;

        [Required]
        public int PostId { get; set; }
        public virtual Post Post { get; set; } = null!;
    }
}
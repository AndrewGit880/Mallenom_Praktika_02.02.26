using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SimpleBlog.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string AuthorId { get; set; } = string.Empty;
        public virtual ApplicationUser Author { get; set; } = null!;

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
using System.ComponentModel.DataAnnotations;

namespace MySimpleBlog.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public int AuthorId { get; set; }
        public User Author { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
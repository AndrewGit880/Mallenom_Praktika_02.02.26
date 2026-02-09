using System.ComponentModel.DataAnnotations;

namespace MySimpleBlog.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        public int PostId { get; set; }
        public Post Post { get; set; }

        public int AuthorId { get; set; }
        public User Author { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
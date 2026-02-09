using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SimpleBlog.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public UserRole Role { get; set; } = UserRole.User; // Используем enum

        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
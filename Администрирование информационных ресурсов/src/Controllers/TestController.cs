using Microsoft.AspNetCore.Mvc;
using MySimpleBlog.Models;

namespace MySimpleBlog.Controllers
{
    public class TestController : Controller
    {
        private readonly AppDbContext _context;

        public TestController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult CheckDatabase()
        {
            try
            {
                var userCount = _context.Users.Count();
                var postCount = _context.Posts.Count();
                var commentCount = _context.Comments.Count();

                return Content($"База данных работает! Пользователей: {userCount}, Постов: {postCount}, Комментариев: {commentCount}");
            }
            catch (Exception ex)
            {
                return Content($"Ошибка базы данных: {ex.Message}");
            }
        }

        [HttpGet]
        public IActionResult CheckSession()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            return Content($"Сессия: UserId={userId}, Username={username}, Role={role}");
        }
    }
}
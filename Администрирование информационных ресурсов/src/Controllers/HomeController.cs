using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySimpleBlog.Models;

namespace MySimpleBlog.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(AppDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("GET / вызван");

            try
            {
                var posts = await _context.Posts
                    .Include(p => p.Author)
                    .Include(p => p.Comments)
                        .ThenInclude(c => c.Author)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                ViewBag.UserId = HttpContext.Session.GetString("UserId");
                ViewBag.Username = HttpContext.Session.GetString("Username");
                ViewBag.Role = HttpContext.Session.GetString("Role");

                _logger.LogInformation($"Найдено {posts.Count} записей");
                _logger.LogInformation($"Пользователь в сессии: {ViewBag.Username}");

                return View(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка в Index: {ex.Message}");
                return View(new List<Post>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(string title, string content)
        {
            _logger.LogInformation("POST /Home/CreatePost вызван");

            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(userId) || role != "Moderator")
            {
                _logger.LogWarning("Нет прав для создания поста");
                return RedirectToAction("Index");
            }

            try
            {
                var post = new Post
                {
                    Title = title,
                    Content = content,
                    AuthorId = int.Parse(userId),
                    CreatedAt = DateTime.Now
                };

                _context.Posts.Add(post);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Создан пост: {title}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при создании поста: {ex.Message}");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateComment(int postId, string content)
        {
            _logger.LogInformation("POST /Home/CreateComment вызван");

            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Пользователь не авторизован для комментирования");
                return RedirectToAction("Index");
            }

            try
            {
                var comment = new Comment
                {
                    PostId = postId,
                    Content = content,
                    AuthorId = int.Parse(userId),
                    CreatedAt = DateTime.Now
                };

                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Добавлен комментарий к посту {postId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при создании комментария: {ex.Message}");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            _logger.LogInformation("POST /Home/DeleteComment вызван");

            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Пользователь не авторизован");
                return RedirectToAction("Index");
            }

            try
            {
                var comment = await _context.Comments
                    .FirstOrDefaultAsync(c => c.Id == commentId);

                if (comment == null)
                {
                    _logger.LogWarning($"Комментарий {commentId} не найден");
                    return RedirectToAction("Index");
                }

                if (role != "Moderator" && comment.AuthorId.ToString() != userId)
                {
                    _logger.LogWarning($"Нет прав для удаления комментария {commentId}");
                    return RedirectToAction("Index");
                }

                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Удален комментарий {commentId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при удалении комментария: {ex.Message}");
            }

            return RedirectToAction("Index");
        }
    }
}
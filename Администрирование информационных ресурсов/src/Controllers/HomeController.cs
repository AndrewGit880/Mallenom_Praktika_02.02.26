using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleBlog.Data;
using SimpleBlog.Models;
using System.Diagnostics;

namespace SimpleBlog.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var posts = await _context.Posts
                    .Include(p => p.Comments)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                // Если нет постов, создаем тестовые
                if (!posts.Any())
                {
                    posts = new List<Post>
                    {
                        new Post
                        {
                            Id = 1,
                            Title = "Добро пожаловать в наш блог!",
                            Content = "Это первая запись в нашем блоге. Здесь вы можете делиться мыслями и общаться с другими пользователями.",
                            CreatedAt = DateTime.Now,
                            AuthorId = "moderator-id",
                            Comments = new List<Comment>
                            {
                                new Comment
                                {
                                    Id = 1,
                                    Content = "Отличная первая запись! Ждем новых статей.",
                                    CreatedAt = DateTime.Now.AddHours(-2),
                                    AuthorId = "user-id",
                                    PostId = 1
                                }
                            }
                        },
                        new Post
                        {
                            Id = 2,
                            Title = "Как пользоваться этим блогом",
                            Content = "Модераторы могут создавать записи, пользователи могут комментировать, гости могут только читать.",
                            CreatedAt = DateTime.Now.AddDays(-1),
                            AuthorId = "moderator-id",
                            Comments = new List<Comment>()
                        }
                    };
                }

                return View(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке записей");
                return View(new List<Post>());
            }
        }

        [Authorize(Roles = "Moderator")]
        public IActionResult CreatePost()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Moderator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(Post post)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userId))
                    {
                        ModelState.AddModelError("", "Пользователь не найден");
                        return View(post);
                    }

                    post.AuthorId = userId;
                    post.CreatedAt = DateTime.UtcNow;

                    _context.Posts.Add(post);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Запись успешно создана!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при создании записи");
                    ModelState.AddModelError("", "Ошибка при создании записи");
                }
            }

            return View(post);
        }

        [HttpPost]
        [Authorize(Roles = "User,Moderator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int postId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Комментарий не может быть пустым";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "Пользователь не найден";
                    return RedirectToAction(nameof(Index));
                }

                var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
                if (!postExists)
                {
                    TempData["ErrorMessage"] = "Запись не найдена";
                    return RedirectToAction(nameof(Index));
                }

                var comment = new Comment
                {
                    PostId = postId,
                    Content = content.Trim(),
                    AuthorId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Комментарий добавлен!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении комментария");
                TempData["ErrorMessage"] = "Ошибка при добавлении комментария";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Moderator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int id)
        {
            try
            {
                var comment = await _context.Comments.FindAsync(id);
                if (comment == null)
                {
                    TempData["ErrorMessage"] = "Комментарий не найден";
                    return RedirectToAction(nameof(Index));
                }

                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Комментарий удален!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении комментария");
                TempData["ErrorMessage"] = "Ошибка при удалении комментария";
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
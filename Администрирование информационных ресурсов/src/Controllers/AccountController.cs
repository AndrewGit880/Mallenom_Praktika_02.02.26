using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySimpleBlog.Models;
using System.Security.Cryptography;
using System.Text;

namespace MySimpleBlog.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(AppDbContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Register()
        {
            _logger.LogInformation("GET /Account/Register вызван");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user)
        {
            _logger.LogInformation("POST /Account/Register вызван");

            try
            {
                if (ModelState.IsValid)
                {
                    _logger.LogInformation("ModelState валиден");

                    var existingUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email == user.Email || u.Username == user.Username);

                    if (existingUser != null)
                    {
                        _logger.LogWarning("Пользователь уже существует");
                        ViewBag.ErrorMessage = "Пользователь с таким email или именем уже существует";
                        return View(user);
                    }

                    user.Password = HashPassword(user.Password);
                    _logger.LogInformation($"Пароль хеширован для пользователя: {user.Username}");

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Пользователь {user.Username} сохранен в БД");

                    HttpContext.Session.SetString("UserId", user.Id.ToString());
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("Role", user.Role);

                    _logger.LogInformation($"Сессия установлена для пользователя: {user.Username}");

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    _logger.LogWarning("ModelState невалиден");
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        _logger.LogWarning($"Ошибка валидации: {error.ErrorMessage}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при регистрации: {ex.Message}");
                ViewBag.ErrorMessage = $"Произошла ошибка: {ex.Message}";
            }

            return View(user);
        }

        public IActionResult Login()
        {
            _logger.LogInformation("GET /Account/Login вызван");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            _logger.LogInformation($"POST /Account/Login вызван для email: {email}");

            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    ViewBag.ErrorMessage = "Пожалуйста, заполните все поля";
                    return View();
                }

                var hashedPassword = HashPassword(password);
                _logger.LogInformation($"Хешированный пароль: {hashedPassword}");

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email && u.Password == hashedPassword);

                if (user != null)
                {
                    _logger.LogInformation($"Пользователь найден: {user.Username}");

                    HttpContext.Session.SetString("UserId", user.Id.ToString());
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("Role", user.Role);

                    _logger.LogInformation($"Сессия установлена для: {user.Username}");

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    _logger.LogWarning("Пользователь не найден или неверный пароль");
                    ViewBag.ErrorMessage = "Неверный email или пароль";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при входе: {ex.Message}");
                ViewBag.ErrorMessage = $"Произошла ошибка: {ex.Message}";
            }

            return View();
        }

        public IActionResult Logout()
        {
            _logger.LogInformation("GET /Account/Logout вызван");
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }
}
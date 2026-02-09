using Microsoft.EntityFrameworkCore;
using MySimpleBlog.Models;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Используем строку подключения по умолчанию");
    connectionString = "server=localhost;port=3306;database=MySimpleBlog;user=root;password=;";
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

var app = builder.Build();

app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

InitializeDatabase(app);

Console.WriteLine("Приложение запущено. Перейдите по адресу: https://localhost:5001");
app.Run();

void InitializeDatabase(IApplicationBuilder appBuilder)
{
    using (var serviceScope = appBuilder.ApplicationServices.CreateScope())
    {
        var context = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            context.Database.EnsureCreated();
            Console.WriteLine("? База данных создана/проверена");

            if (!context.Users.Any())
            {
                AddInitialData(context);
                Console.WriteLine("? Тестовые данные добавлены");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Ошибка базы данных: {ex.Message}");
        }
    }
}

void AddInitialData(AppDbContext context)
{
    var users = new List<User>
    {
        new User
        {
            Username = "Модератор",
            Email = "moderator@test.com",
            Password = HashPassword("123456"),
            Role = "Moderator"
        },
        new User
        {
            Username = "Алексей",
            Email = "user1@test.com",
            Password = HashPassword("123456"),
            Role = "User"
        }
    };

    context.Users.AddRange(users);
    context.SaveChanges();

    var posts = new List<Post>
    {
        new Post
        {
            Title = "Добро пожаловать в мой блог!",
            Content = "Это демонстрационный блог с возможностью регистрации пользователей, создания постов и комментариев.",
            AuthorId = users[0].Id,
            CreatedAt = DateTime.Now
        }
    };

    context.Posts.AddRange(posts);
    context.SaveChanges();
}

string HashPassword(string password)
{
    using (var sha256 = SHA256.Create())
    {
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
    }
}
using CMSBuilder.Data;
using CMSBuilder.Helpers;
using CMSBuilder.Models;
using Microsoft.EntityFrameworkCore;

namespace CMSBuilder.Services;

public class AuthService
{
    public (bool Success, string Error, User? User) Login(string login, string password)
    {
        if (!ValidationHelper.IsValidLogin(login, out var loginError))
            return (false, loginError, null);
        if (!ValidationHelper.IsValidPassword(password, out var passError))
            return (false, passError, null);

        using var db = new AppDbContext();
        var user = db.Users.FirstOrDefault(u => u.Login == login.Trim());
        if (user == null || !PasswordHasher.Verify(password, user.PasswordHash))
            return (false, "Неверный логин или пароль.", null);

        return (true, string.Empty, user);
    }

    public (bool Success, string Error, User? User) Register(string login, string email, string password, string confirm)
    {
        if (!ValidationHelper.IsValidLogin(login, out var loginError))
            return (false, loginError, null);
        if (!ValidationHelper.IsValidEmail(email, out var emailError))
            return (false, emailError, null);
        if (!ValidationHelper.IsValidPassword(password, out var passError))
            return (false, passError, null);
        if (password != confirm)
            return (false, "Пароли не совпадают.", null);

        using var db = new AppDbContext();
        var trimmedLogin = login.Trim();
        var trimmedEmail = email.Trim();

        if (db.Users.Any(u => u.Login == trimmedLogin))
            return (false, "Пользователь с таким логином уже существует.", null);
        if (db.Users.Any(u => u.Email == trimmedEmail))
            return (false, "Email уже используется.", null);

        var user = new User
        {
            Login = trimmedLogin,
            Email = trimmedEmail,
            PasswordHash = PasswordHasher.Hash(password),
            RegisteredAt = DateTime.UtcNow
        };
        db.Users.Add(user);
        db.SaveChanges();
        return (true, string.Empty, user);
    }
}

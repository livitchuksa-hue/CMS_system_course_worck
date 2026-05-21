using System.Text.RegularExpressions;

namespace CMSBuilder.Helpers;

public static class ValidationHelper
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static bool IsValidLogin(string login, out string error)
    {
        if (string.IsNullOrWhiteSpace(login))
        {
            error = "Введите логин.";
            return false;
        }
        if (login.Length < 3)
        {
            error = "Логин должен быть не короче 3 символов.";
            return false;
        }
        error = string.Empty;
        return true;
    }

    public static bool IsValidPassword(string password, out string error)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            error = "Введите пароль.";
            return false;
        }
        if (password.Length < 6)
        {
            error = "Пароль должен быть не короче 6 символов.";
            return false;
        }
        error = string.Empty;
        return true;
    }

    public static bool IsValidEmail(string email, out string error)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            error = "Введите email.";
            return false;
        }
        if (!EmailRegex.IsMatch(email))
        {
            error = "Некорректный email.";
            return false;
        }
        error = string.Empty;
        return true;
    }
}

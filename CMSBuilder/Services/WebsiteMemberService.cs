using CMSBuilder.Data;
using CMSBuilder.Models;
using CMSBuilder.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace CMSBuilder.Services;

public class WebsiteMemberService
{
    public List<WebsiteMemberDto> GetMembers(int websiteId)
    {
        using var db = new AppDbContext();
        var website = db.Websites.Include(w => w.Owner).FirstOrDefault(w => w.Id == websiteId);
        if (website == null) return [];

        var members = new List<WebsiteMemberDto>
        {
            new()
            {
                UserId = website.OwnerId,
                Login = website.Owner.Login,
                Email = website.Owner.Email,
                RoleName = "Владелец",
                RoleCode = "owner",
                IsOwner = true
            }
        };

        var roles = db.WebsiteUserRoles
            .Include(ur => ur.User)
            .Include(ur => ur.Role)
            .Where(ur => ur.WebsiteId == websiteId && ur.UserId != website.OwnerId)
            .ToList();

        foreach (var ur in roles)
        {
            members.Add(new WebsiteMemberDto
            {
                UserId = ur.UserId,
                Login = ur.User.Login,
                Email = ur.User.Email,
                RoleName = ur.Role.Name,
                RoleCode = ur.Role.Code,
                IsOwner = false
            });
        }

        return members;
    }

    public (bool Success, string Error) InviteByLogin(int websiteId, int inviterId, string login, string roleCode)
    {
        login = login.Trim();
        if (string.IsNullOrWhiteSpace(login))
            return (false, "Введите логин пользователя.");

        using var db = new AppDbContext();
        var website = db.Websites.FirstOrDefault(w => w.Id == websiteId);
        if (website == null) return (false, "Сайт не найден.");
        if (website.OwnerId != inviterId && !IsAdminOrOwner(db, websiteId, inviterId))
            return (false, "Недостаточно прав для приглашения.");

        var user = db.Users.FirstOrDefault(u => u.Login == login);
        if (user == null) return (false, "Пользователь с таким логином не найден.");
        if (user.Id == website.OwnerId) return (false, "Владелец уже в команде.");

        if (db.WebsiteUserRoles.Any(ur => ur.WebsiteId == websiteId && ur.UserId == user.Id))
            return (false, "Пользователь уже участник сайта.");

        var role = db.Roles.FirstOrDefault(r => r.Code == roleCode)
            ?? db.Roles.First(r => r.Code == "viewer");

        db.WebsiteUserRoles.Add(new WebsiteUserRole
        {
            WebsiteId = websiteId,
            UserId = user.Id,
            RoleId = role.Id
        });
        db.SaveChanges();
        return (true, string.Empty);
    }

    public (bool Success, string Error) RemoveMember(int websiteId, int actorId, int memberUserId)
    {
        using var db = new AppDbContext();
        var website = db.Websites.FirstOrDefault(w => w.Id == websiteId);
        if (website == null) return (false, "Сайт не найден.");
        if (website.OwnerId != actorId)
            return (false, "Только владелец может удалить участника.");
        if (memberUserId == website.OwnerId)
            return (false, "Нельзя удалить владельца.");

        var link = db.WebsiteUserRoles.FirstOrDefault(ur => ur.WebsiteId == websiteId && ur.UserId == memberUserId);
        if (link == null) return (false, "Участник не найден.");

        db.WebsiteUserRoles.Remove(link);
        db.SaveChanges();
        return (true, string.Empty);
    }

    public (bool Success, string Error) LeaveWebsite(int websiteId, int userId)
    {
        using var db = new AppDbContext();
        var website = db.Websites.FirstOrDefault(w => w.Id == websiteId);
        if (website == null) return (false, "Сайт не найден.");
        if (website.OwnerId == userId)
            return (false, "Владелец не может покинуть свой сайт.");

        var link = db.WebsiteUserRoles.FirstOrDefault(ur => ur.WebsiteId == websiteId && ur.UserId == userId);
        if (link == null) return (false, "Вы не являетесь участником этого сайта.");

        db.WebsiteUserRoles.Remove(link);
        db.SaveChanges();
        return (true, string.Empty);
    }

    public bool IsOwner(int websiteId, int userId)
    {
        using var db = new AppDbContext();
        return db.Websites.Any(w => w.Id == websiteId && w.OwnerId == userId);
    }

    private static bool IsAdminOrOwner(AppDbContext db, int websiteId, int userId)
    {
        var website = db.Websites.First(w => w.Id == websiteId);
        if (website.OwnerId == userId) return true;
        return db.WebsiteUserRoles
            .Include(ur => ur.Role)
            .Any(ur => ur.WebsiteId == websiteId && ur.UserId == userId &&
                       (ur.Role.Code == "admin" || ur.Role.Code == "owner"));
    }
}

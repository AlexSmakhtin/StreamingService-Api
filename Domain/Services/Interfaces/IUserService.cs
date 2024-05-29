using Domain.Entities;
using Domain.Entities.Enums;

namespace Domain.Services.Interfaces;

public interface IUserService
{
    Task<User> Register(
        string name,
        string email,
        string password,
        Statuses status,
        Roles role,
        DateTime birthday,
        CancellationToken ct,
        string avatarFilePath = "");

    Task<User> Authenticate(string email, string password, CancellationToken ct);
    Task ChangeAvatar(User user, string path, CancellationToken ct);

    Task ChangePassword(User user, string oldPassword, string newPassword, CancellationToken ct);
}
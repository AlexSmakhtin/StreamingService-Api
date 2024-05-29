using Domain.Entities;
using Domain.Entities.Enums;
using Domain.Exceptions;
using Domain.Repositories;
using Domain.Services.Interfaces;

namespace Domain.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasherService _passwordHasherService;

    public UserService(IUserRepository repository, IPasswordHasherService passwordHasherService)
    {
        _userRepository = repository;
        _passwordHasherService = passwordHasherService;
    }

    public async Task<User> Register(
        string name,
        string email,
        string password,
        Statuses status,
        Roles role,
        DateTime birthday,
        CancellationToken ct,
        string avatarFilePath = "")
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(password);
        var existedUser =
            await _userRepository.FindByEmail(email, ct);
        if (existedUser != null)
            throw new EmailAlreadyExistsException("Email already used");
        var hashedPassword = _passwordHasherService.HashPassword(password);
        var newUser = new User(name, email, hashedPassword, status, role, birthday, avatarFilePath);
        await _userRepository.Add(newUser, ct);
        return newUser;
    }

    public async Task<User> Authenticate(string email, string password, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(password);
        var existedUser =
            await _userRepository.FindByEmail(email, ct);
        if (existedUser == null)
            throw new UserNotFoundException("Account not found");
        var result = _passwordHasherService.VerifyPassword(existedUser.HashedPassword, password);
        if (result == false)
            throw new IncorrectPasswordException("Password incorrect");
        return existedUser;
    }

    public async Task ChangeAvatar(User user, string path, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        user.AvatarFilePath = path;
        await _userRepository.Update(user, ct);
    }

    public async Task ChangePassword(User user, string oldPassword, string newPassword, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(oldPassword);
        ArgumentNullException.ThrowIfNull(newPassword);
        var isOldIsNew = _passwordHasherService.VerifyPassword(user.HashedPassword, newPassword);
        if (isOldIsNew == true)
            throw new InvalidOperationException("New password is equal to old");
        var isVerified = _passwordHasherService.VerifyPassword(user.HashedPassword, oldPassword);
        if (isVerified == false)
            throw new InvalidOperationException("Old password doesn't match");
        var newHashedPassword = _passwordHasherService.HashPassword(newPassword);
        user.HashedPassword = newHashedPassword;
        await _userRepository.Update(user, ct);
    }
}
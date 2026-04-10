using DataTechnologies.Repositories;
using DataTechnologies.Models;

namespace DataTechnologies.Services;

public class UserService(UserRepository userRepository, IPasswordHasher passwordHasher)
{
    public int AddUser(User user)
    {
        if (string.IsNullOrWhiteSpace(user.Email))
            throw new ArgumentException("Email cannot be empty.");
        if (string.IsNullOrWhiteSpace(user.PasswordHash))
            throw new ArgumentException("Password hash cannot be empty.");
        if (string.IsNullOrWhiteSpace(user.Name))
            throw new ArgumentException("Name cannot be empty.");
        if (user.PasswordHash.Length < 8)
            throw new ArgumentException("Password must be at least 8 characters long.");
        
        User? existingUser = userRepository.GetByEmail(user.Email);
        if (existingUser != null)
            throw new InvalidOperationException("A user with this email already exists.");

        user.PasswordHash = passwordHasher.HashPassword(user.PasswordHash);
        return userRepository.Add(user);
    }

    public bool DeleteUser(int userId)
    {
        if (userId <= 0)
            throw new ArgumentException("Invalid user ID.");

        return userRepository.Delete(userId);
    }

    public bool UpdateUser(User user)
    {
        if (user.UserId <= 0)
            throw new ArgumentException("Invalid user ID.");
        if (string.IsNullOrWhiteSpace(user.Email))
            throw new ArgumentException("Email cannot be empty.");
        if (string.IsNullOrWhiteSpace(user.Name))
            throw new ArgumentException("Name cannot be empty.");

        var existingUser = userRepository.GetById(user.UserId) 
            ?? throw new InvalidOperationException("User not found.");

        if (userRepository.GetByEmail(user.Email) is User emailUser && emailUser.UserId != user.UserId)
            throw new InvalidOperationException("Another user with this email already exists.");
        if (string.IsNullOrWhiteSpace(user.PasswordHash))
            user.PasswordHash = existingUser.PasswordHash;
        else
            user.PasswordHash = passwordHasher.HashPassword(user.PasswordHash);
        return userRepository.Update(user);
    }

    public bool Authenticate(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.");
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty.");

        var user = userRepository.GetByEmail(email);
        return user != null && passwordHasher.VerifyPassword(password, user.PasswordHash);
    }

    public User? GetUserById(int userId)
    {
        if (userId <= 0)
            throw new ArgumentException("Invalid user ID.");

        return userRepository.GetById(userId);
    }

    public User? GetUserByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.");

        return userRepository.GetByEmail(email);
    }

}
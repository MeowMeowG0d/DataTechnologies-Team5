using DataTechnologies.Repositories;
using DataTechnologies.Models;
using DataTechnologies.Utils;
using Npgsql;

namespace DataTechnologies.Services;

public class UserService(IUserRepository userRepository, IPasswordHasher passwordHasher)
{
    private const string InvalidEmailError = "Email format is invalid.";
    private const string InvalidNameError = "Name format is invalid.";
    private const string PasswordSyntaxError = "Password must be 8-64 characters long and include at least one uppercase letter, one lowercase letter, one digit and one special character, without spaces.";

    private static void ValidateEmail(string email)
    {
        if (!InputWhitelistValidator.IsValidEmail(email))
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty.");

            throw new ArgumentException(InvalidEmailError);
        }
    }

    private static void ValidateName(string name)
    {
        if (!InputWhitelistValidator.IsValidName(name))
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty.");

            throw new ArgumentException(InvalidNameError);
        }
    }

    private static void ValidatePasswordSyntax(string password, string emptyPasswordMessage)
    {
        if (!InputWhitelistValidator.IsValidPassword(password))
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException(emptyPasswordMessage);

            throw new ArgumentException(PasswordSyntaxError);
        }
    }

    public int AddUser(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        ValidateEmail(user.Email);
        ValidateName(user.Name);
        ValidatePasswordSyntax(user.PasswordHash, "Password hash cannot be empty.");
        
        User? existingUser = userRepository.GetByEmail(user.Email);
        if (existingUser != null)
            throw new InvalidOperationException("A user with this email already exists.");

        user.PasswordHash = passwordHasher.HashPassword(user.PasswordHash);
        try
        {
            return userRepository.Add(user);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            // Closes the race window between the GetByEmail check above and this insert;
            // the DB-level unique index on EmailIndex is the actual source of truth.
            throw new InvalidOperationException("A user with this email already exists.", ex);
        }
    }

    public bool DeleteUser(int userId)
    {
        if (userId <= 0)
            throw new ArgumentException("Invalid user ID.");

        return userRepository.Delete(userId);
    }

    public bool UpdateUser(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        if (user.UserId <= 0)
            throw new ArgumentException("Invalid user ID.");
        ValidateEmail(user.Email);
        ValidateName(user.Name);

        var existingUser = userRepository.GetById(user.UserId) 
            ?? throw new InvalidOperationException("User not found.");

        if (userRepository.GetByEmail(user.Email) is User emailUser && emailUser.UserId != user.UserId)
            throw new InvalidOperationException("Another user with this email already exists.");
        if (string.IsNullOrWhiteSpace(user.PasswordHash))
            user.PasswordHash = existingUser.PasswordHash;
        else
        {
            ValidatePasswordSyntax(user.PasswordHash, "Password cannot be empty.");
            user.PasswordHash = passwordHasher.HashPassword(user.PasswordHash);
        }
        return userRepository.Update(user);
    }

    public bool ResetPassword(string email, string newPassword)
    {
        ValidateEmail(email);
        ValidatePasswordSyntax(newPassword, "Password cannot be empty.");

        var existingUser = userRepository.GetByEmail(email)
            ?? throw new InvalidOperationException("No user found with this email address.");

        existingUser.PasswordHash = passwordHasher.HashPassword(newPassword);
        return userRepository.Update(existingUser);
    }

    public bool Authenticate(string email, string password)
    {
        ValidateEmail(email);
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
        ValidateEmail(email);

        return userRepository.GetByEmail(email);
    }
    public IReadOnlyCollection<User> GetAllUsers()
    {
        return userRepository.GetAll();
    }

    public IReadOnlyCollection<Orders> GetPreviousOrders(int userId)
    {
        if (userId <= 0)
            throw new ArgumentException("Invalid user ID.");

        return userRepository.GetPreviousOrders(userId);
    }

}
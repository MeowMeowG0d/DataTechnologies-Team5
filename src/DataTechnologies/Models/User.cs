namespace DataTechnologies.Models;

public class User : IComparable<User>, IEquatable<User>
{
    public int UserId { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string Name { get; set; }
    public bool EmailVerified { get; set; }

    public override string ToString() => $"ID: {UserId}, Email: {Email}, Name: {Name}, Email Verified: {EmailVerified}";

    public int CompareTo(User? other)
    {
        if (other is null)
            return 1;

        var result = UserId.CompareTo(other.UserId);
        if (result != 0)
            return result;

        return string.Compare(Email, other.Email, StringComparison.OrdinalIgnoreCase);
    }

    public bool Equals(User? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return UserId == other.UserId && string.Equals(Email, other.Email, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
        => obj is User user && Equals(user);

    public override int GetHashCode()
        => HashCode.Combine(UserId, Email?.ToLowerInvariant());
}
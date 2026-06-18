namespace DataTechnologies.Models;

public class User : IComparable<User>, IEquatable<User>
{

    public static bool operator <(User? left, User? right)
    {
        if (left is null)
            return right is not null;

        return left.CompareTo(right) < 0;
    }

    public static bool operator >(User? left, User? right)
    {
        return right < left;
    }

    public static bool operator <=(User? left, User? right)
    {
        return !(right < left);
    }

    public static bool operator >=(User? left, User? right)
    {
        return !(left < right);
    }
    public int UserId { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string Name { get; set; }
    public bool EmailVerified { get; set; }

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

        return UserId == other.UserId &&
               string.Equals(Email, other.Email, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
        => obj is User user && Equals(user);

    public override int GetHashCode()
        => HashCode.Combine(UserId, Email?.ToUpperInvariant());

    public static bool operator ==(User? left, User? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(User? left, User? right)
        => !(left == right);
}
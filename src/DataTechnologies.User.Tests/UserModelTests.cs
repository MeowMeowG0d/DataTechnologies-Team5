using Xunit;
using DataTechnologies.Models;

namespace UserTests
{
    public class UserModelTests
    {
        private static User CreateUser(int id = 1, string email = "test@example.com", string name = "Test User") => new()
        {
            UserId = id,
            Email = email,
            PasswordHash = "hash",
            Name = name
        };

        [Fact]
        public void Create_SetsAllRequiredFields()
        {
            var user = new User
            {
                UserId = 5,
                Email = "user@example.com",
                PasswordHash = "hashed-password",
                Name = "Jane Doe",
                EmailVerified = true
            };

            Assert.Equal(5, user.UserId);
            Assert.Equal("user@example.com", user.Email);
            Assert.Equal("hashed-password", user.PasswordHash);
            Assert.Equal("Jane Doe", user.Name);
            Assert.True(user.EmailVerified);
        }

        [Fact]
        public void Create_EmailVerified_DefaultsToFalse()
        {
            var user = new User
            {
                Email = "user@example.com",
                PasswordHash = "hashed-password",
                Name = "Jane Doe"
            };

            Assert.False(user.EmailVerified);
        }

        [Fact]
        public void ToString_ContainsIdEmailAndName()
        {
            var user = CreateUser(id: 3, email: "a@b.com", name: "Alice");

            var text = user.ToString();

            Assert.Contains("3", text);
            Assert.Contains("a@b.com", text);
            Assert.Contains("Alice", text);
        }

        [Fact]
        public void CompareTo_Null_ReturnsPositive()
        {
            Assert.True(CreateUser().CompareTo(null) > 0);
        }

        [Fact]
        public void CompareTo_LowerUserId_ReturnsNegative()
        {
            Assert.True(CreateUser(id: 1).CompareTo(CreateUser(id: 2)) < 0);
        }

        [Fact]
        public void CompareTo_SameIdDifferentEmail_ComparesByEmail()
        {
            var a = CreateUser(id: 1, email: "a@example.com");
            var b = CreateUser(id: 1, email: "b@example.com");

            Assert.True(a.CompareTo(b) < 0);
            Assert.True(b.CompareTo(a) > 0);
        }

        [Fact]
        public void CompareTo_SameIdAndEmailDifferentCase_ReturnsZero()
        {
            var a = CreateUser(email: "Test@Example.com");
            var b = CreateUser(email: "test@example.com");

            Assert.Equal(0, a.CompareTo(b));
        }

        [Fact]
        public void Equals_Null_ReturnsFalse()
        {
            Assert.False(CreateUser().Equals(null));
        }

        [Fact]
        public void Equals_SameReference_ReturnsTrue()
        {
            var user = CreateUser();

            Assert.True(user.Equals(user));
        }

        [Fact]
        public void Equals_SameIdAndEmailIgnoringCase_ReturnsTrue()
        {
            var a = CreateUser(email: "Test@Example.com");
            var b = CreateUser(email: "test@example.com");

            Assert.True(a.Equals(b));
        }

        [Fact]
        public void Equals_DifferentUserId_ReturnsFalse()
        {
            Assert.False(CreateUser(id: 1).Equals(CreateUser(id: 2)));
        }

        [Fact]
        public void Equals_DifferentEmail_ReturnsFalse()
        {
            var a = CreateUser(email: "a@example.com");
            var b = CreateUser(email: "b@example.com");

            Assert.False(a.Equals(b));
        }

        [Fact]
        public void GetHashCode_EqualUsersWithDifferentEmailCasing_AreEqual()
        {
            var a = CreateUser(email: "Test@Example.com");
            var b = CreateUser(email: "test@example.com");

            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }
    }
}

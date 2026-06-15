using Xunit;
using DataTechnologies.Services;

namespace UserTests
{
    public class PasswordHasherTests
    {
        private readonly PasswordHasher _hasher = new();

        [Fact]
        public void HashPassword_ReturnsIterationsSaltAndHashSeparatedByDots()
        {
            var hash = _hasher.HashPassword("MySecret123");

            var parts = hash.Split('.');
            Assert.Equal(3, parts.Length);
            Assert.True(int.TryParse(parts[0], out var iterations));
            Assert.True(iterations > 0);
            Assert.NotEmpty(parts[1]);
            Assert.NotEmpty(parts[2]);
        }

        [Fact]
        public void HashPassword_SamePasswordTwice_ProducesDifferentHashes()
        {
            var first = _hasher.HashPassword("MySecret123");
            var second = _hasher.HashPassword("MySecret123");

            Assert.NotEqual(first, second);
        }

        [Fact]
        public void VerifyPassword_CorrectPassword_ReturnsTrue()
        {
            var hash = _hasher.HashPassword("MySecret123");

            Assert.True(_hasher.VerifyPassword("MySecret123", hash));
        }

        [Fact]
        public void VerifyPassword_WrongPassword_ReturnsFalse()
        {
            var hash = _hasher.HashPassword("MySecret123");

            Assert.False(_hasher.VerifyPassword("WrongPassword", hash));
        }

        [Theory]
        [InlineData("")]
        [InlineData("no-dots-at-all")]
        [InlineData("only.two")]
        public void VerifyPassword_MalformedHash_ReturnsFalse(string malformedHash)
        {
            Assert.False(_hasher.VerifyPassword("MySecret123", malformedHash));
        }

        [Fact]
        public void VerifyPassword_NonNumericIterations_ReturnsFalse()
        {
            Assert.False(_hasher.VerifyPassword("MySecret123", "abc.c2FsdA==.aGFzaA=="));
        }

        [Fact]
        public void VerifyPassword_InvalidBase64Salt_ReturnsFalse()
        {
            Assert.False(_hasher.VerifyPassword("MySecret123", "600000.not-base64!.aGFzaA=="));
        }

        [Fact]
        public void VerifyPassword_TamperedHash_ReturnsFalse()
        {
            var hash = _hasher.HashPassword("MySecret123");
            var parts = hash.Split('.');
            var tampered = $"{parts[0]}.{parts[1]}.{parts[2].Substring(1)}A";

            Assert.False(_hasher.VerifyPassword("MySecret123", tampered));
        }
    }
}

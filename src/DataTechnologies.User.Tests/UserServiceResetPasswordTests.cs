using System;
using Xunit;
using Moq;
using DataTechnologies.Services;
using DataTechnologies.Repositories;
using DataTechnologies.Models;

namespace UserTests
{
    public class UserServiceResetPasswordTests
    {
        private static User SampleUser() => new()
        {
            UserId = 1,
            Email = "user@example.com",
            Name = "Test User",
            PasswordHash = "old-hash",
            EmailVerified = true
        };

        [Fact]
        public void ResetPassword_WithValidEmailAndPassword_HashesAndUpdates()
        {
            var user = SampleUser();
            var mockRepo = new Mock<IUserRepository>();
            mockRepo.Setup(r => r.GetByEmail(user.Email)).Returns(user);
            mockRepo.Setup(r => r.Update(It.IsAny<User>())).Returns(true);

            var hasher = new PasswordHasher();
            var service = new UserService(mockRepo.Object, hasher);

            var result = service.ResetPassword(user.Email, "newSecret123");

            Assert.True(result);
            Assert.True(hasher.VerifyPassword("newSecret123", user.PasswordHash));
            mockRepo.Verify(r => r.Update(It.Is<User>(u => u.UserId == user.UserId)), Times.Once);
        }

        [Fact]
        public void ResetPassword_AfterReset_CanAuthenticateWithNewPassword()
        {
            var user = SampleUser();
            var mockRepo = new Mock<IUserRepository>();
            mockRepo.Setup(r => r.GetByEmail(user.Email)).Returns(user);
            mockRepo.Setup(r => r.Update(It.IsAny<User>())).Returns(true);

            var service = new UserService(mockRepo.Object, new PasswordHasher());

            service.ResetPassword(user.Email, "newSecret123");

            Assert.True(service.Authenticate(user.Email, "newSecret123"));
            Assert.False(service.Authenticate(user.Email, "wrongPassword"));
        }

        [Fact]
        public void ResetPassword_WithUnknownEmail_Throws()
        {
            var mockRepo = new Mock<IUserRepository>();
            mockRepo.Setup(r => r.GetByEmail(It.IsAny<string>())).Returns((User?)null);

            var service = new UserService(mockRepo.Object, new PasswordHasher());

            Assert.Throws<InvalidOperationException>(
                () => service.ResetPassword("unknown@example.com", "newSecret123"));
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("user@")]
        public void ResetPassword_WithInvalidEmailFormat_Throws(string email)
        {
            var mockRepo = new Mock<IUserRepository>();
            var service = new UserService(mockRepo.Object, new PasswordHasher());

            var ex = Assert.Throws<ArgumentException>(() => service.ResetPassword(email, "newSecret123"));
            Assert.Contains("Email format is invalid", ex.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData("short")]
        [InlineData("password123")]
        [InlineData("PASSWORD123")]
        [InlineData("Password")]
        [InlineData("Password 123")]
        public void ResetPassword_WithInvalidPassword_Throws(string password)
        {
            var mockRepo = new Mock<IUserRepository>();
            var service = new UserService(mockRepo.Object, new PasswordHasher());

            Assert.Throws<ArgumentException>(
                () => service.ResetPassword("user@example.com", password));
        }
    }
}

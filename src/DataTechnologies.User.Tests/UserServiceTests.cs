using System;
using System.Collections.Generic;
using Xunit;
using Moq;
using DataTechnologies.Services;
using DataTechnologies.Repositories;
using DataTechnologies.Models;

namespace UserTests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _repo = new();
        private readonly Mock<IPasswordHasher> _hasher = new();
        private readonly UserService _service;

        public UserServiceTests()
        {
            _service = new UserService(_repo.Object, _hasher.Object);
        }

        private static User CreateUser(int id = 0, string email = "test@example.com",
            string password = "Password123", string name = "Test User") => new()
        {
            UserId = id,
            Email = email,
            PasswordHash = password,
            Name = name
        };

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void AddUser_EmptyEmail_ThrowsArgumentException(string email)
        {
            Assert.Throws<ArgumentException>(() => _service.AddUser(CreateUser(email: email)));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void AddUser_EmptyPassword_ThrowsArgumentException(string password)
        {
            Assert.Throws<ArgumentException>(() => _service.AddUser(CreateUser(password: password)));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void AddUser_EmptyName_ThrowsArgumentException(string name)
        {
            Assert.Throws<ArgumentException>(() => _service.AddUser(CreateUser(name: name)));
        }

        [Fact]
        public void AddUser_PasswordShorterThanEightCharacters_ThrowsArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(() => _service.AddUser(CreateUser(password: "short")));
            Assert.Contains("at least 8 characters", ex.Message);
        }

        [Fact]
        public void AddUser_EmailAlreadyExists_ThrowsInvalidOperationException()
        {
            _repo.Setup(r => r.GetByEmail("test@example.com")).Returns(CreateUser(id: 1));

            Assert.Throws<InvalidOperationException>(() => _service.AddUser(CreateUser()));
        }

        [Fact]
        public void AddUser_ValidUser_HashesPasswordAndReturnsNewId()
        {
            _repo.Setup(r => r.GetByEmail(It.IsAny<string>())).Returns((User?)null);
            _repo.Setup(r => r.Add(It.IsAny<User>())).Returns(42);
            _hasher.Setup(h => h.HashPassword("Password123")).Returns("hashed-value");

            var user = CreateUser();
            var newId = _service.AddUser(user);

            Assert.Equal(42, newId);
            Assert.Equal("hashed-value", user.PasswordHash);
            _repo.Verify(r => r.Add(It.Is<User>(u => u.PasswordHash == "hashed-value")), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void DeleteUser_InvalidId_ThrowsArgumentException(int userId)
        {
            Assert.Throws<ArgumentException>(() => _service.DeleteUser(userId));
        }

        [Fact]
        public void DeleteUser_ValidId_DelegatesToRepository()
        {
            _repo.Setup(r => r.Delete(5)).Returns(true);

            Assert.True(_service.DeleteUser(5));
            _repo.Verify(r => r.Delete(5), Times.Once);
        }

        [Fact]
        public void UpdateUser_InvalidId_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => _service.UpdateUser(CreateUser(id: 0)));
        }

        [Fact]
        public void UpdateUser_EmptyEmail_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => _service.UpdateUser(CreateUser(id: 1, email: " ")));
        }

        [Fact]
        public void UpdateUser_EmptyName_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => _service.UpdateUser(CreateUser(id: 1, name: " ")));
        }

        [Fact]
        public void UpdateUser_UserNotFound_ThrowsInvalidOperationException()
        {
            _repo.Setup(r => r.GetById(1)).Returns((User?)null);

            Assert.Throws<InvalidOperationException>(() => _service.UpdateUser(CreateUser(id: 1)));
        }

        [Fact]
        public void UpdateUser_EmailTakenByAnotherUser_ThrowsInvalidOperationException()
        {
            _repo.Setup(r => r.GetById(1)).Returns(CreateUser(id: 1));
            _repo.Setup(r => r.GetByEmail("test@example.com")).Returns(CreateUser(id: 2));

            Assert.Throws<InvalidOperationException>(() => _service.UpdateUser(CreateUser(id: 1)));
        }

        [Fact]
        public void UpdateUser_EmptyPassword_KeepsExistingPasswordHash()
        {
            var existing = CreateUser(id: 1, password: "existing-hash");
            _repo.Setup(r => r.GetById(1)).Returns(existing);
            _repo.Setup(r => r.GetByEmail("test@example.com")).Returns(existing);
            _repo.Setup(r => r.Update(It.IsAny<User>())).Returns(true);

            var update = CreateUser(id: 1, password: "");
            Assert.True(_service.UpdateUser(update));

            Assert.Equal("existing-hash", update.PasswordHash);
            _hasher.Verify(h => h.HashPassword(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void UpdateUser_NewPassword_HashesPasswordBeforeUpdating()
        {
            var existing = CreateUser(id: 1, password: "existing-hash");
            _repo.Setup(r => r.GetById(1)).Returns(existing);
            _repo.Setup(r => r.GetByEmail("test@example.com")).Returns(existing);
            _repo.Setup(r => r.Update(It.IsAny<User>())).Returns(true);
            _hasher.Setup(h => h.HashPassword("NewPassword123")).Returns("new-hash");

            var update = CreateUser(id: 1, password: "NewPassword123");
            Assert.True(_service.UpdateUser(update));

            Assert.Equal("new-hash", update.PasswordHash);
        }

        [Theory]
        [InlineData("", "Password123")]
        [InlineData("test@example.com", "")]
        public void Authenticate_EmptyInput_ThrowsArgumentException(string email, string password)
        {
            Assert.Throws<ArgumentException>(() => _service.Authenticate(email, password));
        }

        [Fact]
        public void Authenticate_UnknownEmail_ReturnsFalse()
        {
            _repo.Setup(r => r.GetByEmail("unknown@example.com")).Returns((User?)null);

            Assert.False(_service.Authenticate("unknown@example.com", "Password123"));
        }

        [Fact]
        public void Authenticate_WrongPassword_ReturnsFalse()
        {
            _repo.Setup(r => r.GetByEmail("test@example.com")).Returns(CreateUser(id: 1, password: "stored-hash"));
            _hasher.Setup(h => h.VerifyPassword("WrongPassword", "stored-hash")).Returns(false);

            Assert.False(_service.Authenticate("test@example.com", "WrongPassword"));
        }

        [Fact]
        public void Authenticate_CorrectPassword_ReturnsTrue()
        {
            _repo.Setup(r => r.GetByEmail("test@example.com")).Returns(CreateUser(id: 1, password: "stored-hash"));
            _hasher.Setup(h => h.VerifyPassword("Password123", "stored-hash")).Returns(true);

            Assert.True(_service.Authenticate("test@example.com", "Password123"));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void GetUserById_InvalidId_ThrowsArgumentException(int userId)
        {
            Assert.Throws<ArgumentException>(() => _service.GetUserById(userId));
        }

        [Fact]
        public void GetUserById_ValidId_ReturnsUserFromRepository()
        {
            var user = CreateUser(id: 7);
            _repo.Setup(r => r.GetById(7)).Returns(user);

            Assert.Same(user, _service.GetUserById(7));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void GetUserByEmail_EmptyEmail_ThrowsArgumentException(string email)
        {
            Assert.Throws<ArgumentException>(() => _service.GetUserByEmail(email));
        }

        [Fact]
        public void GetUserByEmail_ValidEmail_ReturnsUserFromRepository()
        {
            var user = CreateUser(id: 7);
            _repo.Setup(r => r.GetByEmail("test@example.com")).Returns(user);

            Assert.Same(user, _service.GetUserByEmail("test@example.com"));
        }

        [Fact]
        public void GetAllUsers_ReturnsAllUsersFromRepository()
        {
            var users = new List<User> { CreateUser(id: 1), CreateUser(id: 2, email: "other@example.com") };
            _repo.Setup(r => r.GetAll()).Returns(users);

            Assert.Same(users, _service.GetAllUsers());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void GetPreviousOrders_InvalidId_ThrowsArgumentException(int userId)
        {
            Assert.Throws<ArgumentException>(() => _service.GetPreviousOrders(userId));
        }

        [Fact]
        public void GetPreviousOrders_ValidId_ReturnsOrdersFromRepository()
        {
            var orders = new List<Orders>
            {
                new() { OrderID = 1, OrderNumber = "ORD-001", UserID = 7, PaymentMethod = "iDEAL", OrderStatus = "Shipped", TotalAmount = 19.99f }
            };
            _repo.Setup(r => r.GetPreviousOrders(7)).Returns(orders);

            Assert.Same(orders, _service.GetPreviousOrders(7));
        }
    }
}

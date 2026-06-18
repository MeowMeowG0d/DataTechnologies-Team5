using DataTechnologies.Models;

namespace DataTechnologies.Repositories;

public interface IUserRepository
{
    int Add(User user);
    bool Update(User user);
    bool Delete(int userId);
    User? GetById(int userId);
    User? GetByEmail(string email);
    IReadOnlyCollection<User> GetAll();
    IReadOnlyCollection<Orders> GetPreviousOrders(int userId);
}

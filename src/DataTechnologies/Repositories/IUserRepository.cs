using DataTechnologies.Models;

namespace DataTechnologies.Repositories;

public interface IUserRepository
{
    int Add(User user);
    bool Update(User user);
    bool Delete(int userId);
    User? GetById(int userId);
    User? GetByEmail(string email);
    List<User> GetAll();
    List<Orders> GetPreviousOrders(int userId);
}

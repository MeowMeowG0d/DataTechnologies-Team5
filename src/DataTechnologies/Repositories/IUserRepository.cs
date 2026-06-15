using DataTechnologies.Models;
using System.Collections.Generic;

namespace DataTechnologies.Repositories
{
    public interface IUserRepository
    {
        int Add(User user);
        User? GetById(int userId);
        User? GetByEmail(string email);
        List<User> GetAll();
        List<Orders> GetPreviousOrders(int userId);
        bool Update(User user);
        bool Delete(int userId);
    }
}

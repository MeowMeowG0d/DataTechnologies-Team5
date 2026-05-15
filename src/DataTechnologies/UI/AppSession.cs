using DataTechnologies.Models;

namespace DataTechnologies.UI
{
    public class AppSession
    {
        public bool IsLoggedIn => CurrentUser != null;
        public User? CurrentUser { get; private set; }
        
        public int UserId => CurrentUser?.UserId ?? 0;

        public void Login(User user) => CurrentUser = user;
        public void Logout() => CurrentUser = null;
    }
}

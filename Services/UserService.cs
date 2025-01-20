using Microsoft.EntityFrameworkCore;
using RestoApp.Data;

namespace RestoApp.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetMaxUserIdAsync()
        {
            // Ensure the users table has data, then fetch the maximum user ID
            var maxId = await _context.Users.MaxAsync(u => u.Id);
            return (maxId + 1);
        }
    }
}

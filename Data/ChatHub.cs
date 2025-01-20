using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RestoApp.Models.Entities;

namespace RestoApp.Data
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        public ChatHub(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;

            var user = _httpContextAccessor.HttpContext.User;
            if (user.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(user);
                if (!string.IsNullOrEmpty(userId))
                {
                    var userConnection = new UserConnection
                    {
                        UserId = Convert.ToInt32(userId),
                        ConnectionId = connectionId,
                        DataEnteredBy = Convert.ToInt32(userId)
                    };

                    var existingConnection = await _context.UserConnections
                        .FirstOrDefaultAsync(x => x.UserId == Convert.ToInt32(userId));

                    if (existingConnection != null)
                    {
                        existingConnection.ConnectionId = connectionId;
                        _context.UserConnections.Update(existingConnection);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        var maxId = 1;
                        if (await _context.UserConnections.AnyAsync())
                        {
                            maxId = await _context.UserConnections.MaxAsync(u => u.Id);
                            maxId = maxId + 1;
                        }
                        userConnection.Id = Convert.ToInt32(maxId);
                        await _context.UserConnections.AddAsync(userConnection);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}

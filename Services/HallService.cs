using AutoMapper;
using Microsoft.AspNetCore.Identity;
using RestoApp.Data;
using RestoApp.Models.Entities;
using RestoApp.Models.ViewModels;
using RestoApp.Models;
using RestoApp.Repository;
using Microsoft.EntityFrameworkCore;

namespace RestoApp.Services
{
    public class HallService
    {
        private readonly IRepository<Hall> _hallRepository;
        private readonly ApplicationDbContext context;
        private readonly ILogger<HallService> _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HallService(IRepository<Hall> hallRepository, ApplicationDbContext context, ILogger<HallService> logger, IMapper mapper, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _hallRepository = hallRepository;
            this.context = context;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }
        private async Task<int> GetMaxId()
        {
            if (await context.Halls.AnyAsync())
            {
                return await context.Halls.MaxAsync(e => e.HallId) + 1;
            }
            return 1;
        }
        public async Task<APIResponseEntity> GetHallAsync()
        {
            APIResponseEntity _response = new APIResponseEntity();
            try
            {
                var categories = await _hallRepository.GetAllAsync();
                _response.statusCode = 1;
                _response.status = "Success";
                _response.data = categories;
            }
            catch (Exception ex)
            {
                _response.statusCode = 0;
                _response.status = "Failed";
                _logger.LogError(ex, "An error occurred in {ServiceName} at {Time}", nameof(HallService), DateTime.Now);
            }
            return _response;
        }
        public async Task<APIResponseEntity> GetHallByIdAsync(int id)
        {
            APIResponseEntity _response = new APIResponseEntity();
            try
            {
                var hall = await _hallRepository.GetByIdAsync(id);
                _response.statusCode = 1;
                _response.status = "Success";
                _response.data = hall;
            }
            catch (Exception ex)
            {
                _response.statusCode = 0;
                _response.status = "Failed";
                _logger.LogError(ex, "An error occurred in {ServiceName} at {Time}", nameof(HallService), DateTime.Now);
            }
            return _response;
        }
        private async Task<Hall> GetHallByNameAsync(string name)
        {
            if (await context.Halls.AnyAsync())
            {
                return await context.Halls.FirstOrDefaultAsync(n => n.HallName == name);
            }
            return new Hall();
        }
        public async Task<APIResponseEntity> AddHallAsync(HallViewModel hall)
        {
            APIResponseEntity _response = new APIResponseEntity();
            try
            {
                var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
                if (hall.HallId == 0)
                {
                    hall.HallId = await GetMaxId();
                    hall.DataEnteredBy = user.Id;
                    var HallExist = await context.Halls.FirstOrDefaultAsync(h => h.HallName == hall.HallName);
                    if (HallExist == null)
                    {
                        var Hall = _mapper.Map<Hall>(hall);
                        await _hallRepository.AddAsync(Hall);
                        _response.statusCode = 1;
                        _response.status = "Success";
                        _response.message = "Hall created successfully";
                    }
                    else
                    {
                        _response.statusCode = 0;
                        _response.status = "Failed";
                        _response.message = "Hall already exists";
                    }
                }
                else
                {
                    var existHall = await context.Halls.FirstOrDefaultAsync(h=>h.HallId==hall.HallId);
                    if (existHall == null)
                    {
                        _response.statusCode = 0;
                        _response.status = "Failed";
                        _response.message = "Hall not found";
                    }
                    else
                    {
                        var HallExist = await context.Halls.FirstOrDefaultAsync(c => c.HallName == hall.HallName && c.HallId != hall.HallId);
                        if (HallExist == null)
                        {
                            existHall.HallName = hall.HallName;
                            existHall.HallDescription = hall.HallDescription;
                            existHall.DataModifiedBy = user.Id;
                            existHall.DataModifiedOn = DateTime.Now;
                            await _hallRepository.UpdateAsync(existHall);
                            _response.statusCode = 1;
                            _response.status = "Success";
                            _response.message = "Hall updated successfully";
                        }
                        else
                        {
                            _response.statusCode = 0;
                            _response.status = "Failed";
                            _response.message = "Hall already exists";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in {ServiceName} at {Time}", nameof(HallService), DateTime.Now);
            }
            return _response;
        }
    }
}

using AutoMapper;
using Microsoft.AspNetCore.Identity;
using RestoApp.Data;
using RestoApp.Models.Entities;
using RestoApp.Models.ViewModels;
using RestoApp.Models;
using RestoApp.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RestoApp.Services
{
    public class TableService
    {
        private readonly IRepository<Table> _tableRepository;
        private readonly ApplicationDbContext context;
        private readonly ILogger<TableService> _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TableService(IRepository<Table> tableRepository, ApplicationDbContext context, ILogger<TableService> logger, IMapper mapper, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _tableRepository = tableRepository;
            this.context = context;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }
        public TableViewModel GetHallsAsync()
        {
            try
            {
                var model = new TableViewModel
                {
                    Halls = context.Halls
                        .Select(c => new SelectListItem
                        {
                            Value = c.HallId.ToString(),
                            Text = c.HallName
                        }).ToList()
                };
                model.Halls.Insert(0, new SelectListItem
                {
                    Value = "0",
                    Text = "Select Hall"
                });
                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in {ServiceName} at {Time}", nameof(CategoryService), DateTime.Now);
                return new TableViewModel();
            }
        }
        private async Task<int> GetMaxId()
        {
            if (await context.Tables.AnyAsync())
            {
                return await context.Tables.MaxAsync(e => e.Id) + 1;
            }
            return 1;
        }
        public async Task<APIResponseEntity> GetTableAsync()
        {
            APIResponseEntity _response = new APIResponseEntity();
            try
            {
                var tables = await context.Tables.Include(m=>m.Hall).ToListAsync();
                _response.statusCode = 1;
                _response.status = "Success";
                _response.data = tables;
            }
            catch (Exception ex)
            {
                _response.statusCode = 0;
                _response.status = "Failed";
                _logger.LogError(ex, "An error occurred in {ServiceName} at {Time}", nameof(TableService), DateTime.Now);
            }
            return _response;
        }
        public async Task<APIResponseEntity> GetTableByIdAsync(int id)
        {
            APIResponseEntity _response = new APIResponseEntity();
            try
            {
                var table = await _tableRepository.GetByIdAsync(id);
                _response.statusCode = 1;
                _response.status = "Success";
                _response.data = table;
            }
            catch (Exception ex)
            {
                _response.statusCode = 0;
                _response.status = "Failed";
                _logger.LogError(ex, "An error occurred in {ServiceName} at {Time}", nameof(TableService), DateTime.Now);
            }
            return _response;
        }
        private async Task<Table> GetTableByNameAsync(string name)
        {
            if (await context.Tables.AnyAsync())
            {
                return await context.Tables.FirstOrDefaultAsync(n => n.Name == name);
            }
            return new Table();
        }
        public async Task<APIResponseEntity> AddTableAsync(TableViewModel table)
        {
            APIResponseEntity _response = new APIResponseEntity();
            try
            {
                var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
                if (table.Id == 0)
                {
                    table.Id = await GetMaxId();
                    table.DataEnteredBy = user.Id;
                    var TableExist = await context.Tables.FirstOrDefaultAsync(t => t.Name == table.Name && t.HallId==table.HallId);
                    if (TableExist == null)
                    {
                        var Table = _mapper.Map<Table>(table);
                        await _tableRepository.AddAsync(Table);
                        _response.statusCode = 1;
                        _response.status = "Success";
                        _response.message = "Table created successfully";
                    }
                    else
                    {
                        _response.statusCode = 0;
                        _response.status = "Failed";
                        _response.message = "Table already exists";
                    }
                }
                else
                {
                    var existTable = await context.Tables.FirstOrDefaultAsync(h => h.Id == table.Id);
                    if (existTable == null)
                    {
                        _response.statusCode = 0;
                        _response.status = "Failed";
                        _response.message = "Table not found";
                    }
                    else
                    {
                        var TableExist = await context.Tables.FirstOrDefaultAsync(c => c.Name == table.Name && c.Id != table.Id);
                        if (TableExist == null)
                        {
                            existTable.Name = table.Name;
                            existTable.DataModifiedBy = user.Id;
                            existTable.DataModifiedOn = DateTime.Now;
                            await _tableRepository.UpdateAsync(existTable);
                            _response.statusCode = 1;
                            _response.status = "Success";
                            _response.message = "Table updated successfully";
                        }
                        else
                        {
                            _response.statusCode = 0;
                            _response.status = "Failed";
                            _response.message = "Table already exists";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in {ServiceName} at {Time}", nameof(TableService), DateTime.Now);
            }
            return _response;
        }
    }
}

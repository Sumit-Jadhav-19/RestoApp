using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestoApp.Data;
using RestoApp.Models;
using RestoApp.Models.Entities;
using RestoApp.Models.ViewModels;
using RestoApp.Repository;

namespace RestoApp.Services
{
    public class MenuService
    {
        private readonly IRepository<Menu> _menuRepository;
        private readonly ApplicationDbContext context;
        private readonly ILogger<MenuService> _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MenuService(IRepository<Menu> menuRepository,
            ApplicationDbContext context,
            ILogger<MenuService> logger,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _menuRepository = menuRepository;
            this.context = context;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }
        public MenuViewModel GetCategoriesAsync()
        {
            try
            {
                var model = new MenuViewModel
                {
                    Categories = context.Categories
                        .Select(c => new SelectListItem
                        {
                            Value = c.Id.ToString(),
                            Text = c.Name
                        }).ToList()
                };
                model.Categories.Insert(0, new SelectListItem
                {
                    Value = "0",
                    Text = "Select Category"
                });
                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in {ServiceName} at {Time}", nameof(CategoryService), DateTime.Now);
                return new MenuViewModel();
            }
        }
        private async Task<int> GetMaxId()
        {
            if (await context.Menus.AnyAsync())
            {
                return await context.Menus.MaxAsync(e => e.MenuId) + 1;
            }
            return 1;
        }
        public async Task<APIResponseEntity> GetMenusAsync()
        {
            APIResponseEntity _response = new APIResponseEntity();
            try
            {
                var menus = await (from menu in context.Menus
                                   join category in context.Categories
                                   on menu.CategoryId equals category.Id
                                   join user in context.Users on menu.CreatedBy equals user.Id
                                   join modify in context.Users on menu.ModifiedBy equals modify.Id into modifyByGroup
                                   from modify in modifyByGroup.DefaultIfEmpty()
                                   select new
                                   {
                                       menu.MenuId,
                                       menu.Name,
                                       menu.Description,
                                       menu.CategoryId,
                                       CategoryName = category.Name,
                                       menu.Price,
                                       menu.IsHalfAvailable,
                                       menu.HalfPrice,
                                       menu.Discount,
                                       menu.ImagePath,
                                       menu.IsAvailable,
                                       menu.PreparationTime,
                                       menu.IsSpecial,
                                       CreatedUser = (user.FirstName + " " + user.LastName).Trim(),
                                       ModifiedUser = (modify.FirstName + " " + modify.LastName).Trim(),
                                       menu.CreatedOn,
                                       menu.ModifiedOn,
                                       menu.IsActive
                                   }).ToListAsync();
                _response.statusCode = 1;
                _response.status = "Success";
                _response.data = menus;
            }
            catch (Exception ex)
            {
                _response.statusCode = 0;
                _response.status = "Failed";
                _logger.LogError(ex, "An error occurred in {ServiceName} at {Time}", nameof(CategoryService), DateTime.Now);
            }
            return _response;
        }
        public async Task<APIResponseEntity> GetMenuByIdAsync(int id)
        {
            APIResponseEntity _response = new APIResponseEntity();
            try
            {
                var menus = await (from menu in context.Menus
                                   join category in context.Categories
                                   on menu.CategoryId equals category.Id
                                   join user in context.Users on menu.CreatedBy equals user.Id
                                   join modify in context.Users on menu.ModifiedBy equals modify.Id into modifyByGroup
                                   from modify in modifyByGroup.DefaultIfEmpty()
                                   where menu.MenuId == id
                                   select new
                                   {
                                       menu.MenuId,
                                       menu.Name,
                                       menu.Description,
                                       menu.CategoryId,
                                       CategoryName = category.Name,
                                       menu.Price,
                                       menu.IsHalfAvailable,
                                       menu.HalfPrice,
                                       menu.Discount,
                                       menu.ImagePath,
                                       menu.IsAvailable,
                                       menu.PreparationTime,
                                       menu.IsSpecial,
                                       CreatedUser = (user.FirstName + " " + user.LastName).Trim(),
                                       ModifiedUser = (modify.FirstName + " " + modify.LastName).Trim(),
                                       menu.CreatedOn,
                                       menu.ModifiedOn,
                                       menu.IsActive
                                   }).FirstOrDefaultAsync();
                _response.statusCode = 1;
                _response.status = "Success";
                _response.data = menus;
            }
            catch (Exception ex)
            {
                _response.statusCode = 0;
                _response.status = "Failed";
                _logger.LogError(ex, "An error occurred in {ServiceName} at {Time}", nameof(CategoryService), DateTime.Now);
            }
            return _response;
        }
        private async Task<Menu> GetMenuByNameAsync(string name)
        {
            if (await context.Menus.AnyAsync())
            {
                return await context.Menus.FirstOrDefaultAsync(n => n.Name == name);
            }
            return null;
        }
        public async Task<APIResponseEntity> AddMenuAsync(MenuViewModel menu, string fileExtension)
        {
            APIResponseEntity _response = new APIResponseEntity();
            try
            {
                var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
                if (menu.MenuId == 0)
                {
                    menu.CreatedBy = user.Id;
                    menu.MenuId = await GetMaxId();
                    if (!string.IsNullOrEmpty(fileExtension))
                    {
                        menu.ImagePath = menu.MenuId.ToString() + "" + fileExtension;
                    }
                    else
                    {
                        menu.ImagePath = "No-Image.png";
                    }

                    var MenuExist = await GetMenuByNameAsync(menu.Name);
                    if (MenuExist == null)
                    {
                        var Menu = _mapper.Map<Menu>(menu);
                        await _menuRepository.AddAsync(Menu);
                        _response.statusCode = 1;
                        _response.status = "Success";
                        _response.message = "Menu created successfully";
                        _response.data = menu.MenuId.ToString();
                    }
                    else
                    {
                        _response.statusCode = 0;
                        _response.status = "Failed";
                        _response.message = "Menu already exists";
                    }
                }
                else
                {
                    Menu Menu = await _menuRepository.GetByIdAsync(menu.MenuId);
                    if (Menu == null)
                    {
                        _response.statusCode = 0;
                        _response.status = "Failed";
                        _response.message = "Menu not found";
                    }
                    else
                    {
                        var MenuExist = await context.Menus.FirstOrDefaultAsync(c => c.Name == menu.Name && c.MenuId != menu.MenuId);
                        if (MenuExist == null)
                        {
                            Menu.Name = menu.Name;
                            Menu.Description = menu.Description;
                            Menu.CategoryId = menu.CategoryId;
                            Menu.Price = menu.Price;
                            Menu.IsHalfAvailable = menu.IsHalfAvailable;
                            Menu.HalfPrice = menu.HalfPrice ?? 0;
                            Menu.Discount = menu.Discount;
                            Menu.IsAvailable = menu.IsAvailable;
                            Menu.PreparationTime = menu.PreparationTime;
                            Menu.IsSpecial = menu.IsSpecial;
                            Menu.ModifiedBy = user.Id;
                            Menu.ModifiedOn = DateTime.Now;
                            if (!string.IsNullOrEmpty(fileExtension))
                            {
                                Menu.ImagePath = menu.MenuId.ToString() + "" + fileExtension;
                            }
                            await _menuRepository.UpdateAsync(Menu);
                            _response.statusCode = 1;
                            _response.status = "Success";
                            _response.message = "Menu updated successfully";
                            _response.data = menu.MenuId.ToString();
                        }
                        else
                        {
                            _response.statusCode = 0;
                            _response.status = "Failed";
                            _response.message = "Menu already exists";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in {ServiceName} at {Time}", nameof(CategoryService), DateTime.Now);
            }
            return _response;
        }
    }
}

using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestoApp.Data;
using RestoApp.Models;
using RestoApp.Models.Entities;
using RestoApp.Models.ViewModels;
using RestoApp.Repository;

namespace RestoApp.Services
{
    public class CategoryService
    {
        private readonly IRepository<Categories> _categoryRepository;
        private readonly ApplicationDbContext context;
        private readonly ILogger<CategoryService> _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CategoryService(IRepository<Categories> categoryRepository, ApplicationDbContext context, ILogger<CategoryService> logger, IMapper mapper, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _categoryRepository = categoryRepository;
            this.context = context;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }
        private async Task<int> GetMaxId()
        {
            if (await context.Categories.AnyAsync())
            {
                return await context.Categories.MaxAsync(e => e.Id) + 1;
            }
            return 1;
        }
        public async Task<APIResponseEntity> GetCategoriesAsync()
        {
            APIResponseEntity _response = new APIResponseEntity();
            try
            {
                var categories = await _categoryRepository.GetAllAsync();
                _response.statusCode = 1;
                _response.status = "Success";
                _response.data = categories;
            }
            catch (Exception ex)
            {
                _response.statusCode = 0;
                _response.status = "Failed";
                _logger.LogError(ex, "An error occurred in {ServiceName} at {Time}", nameof(CategoryService), DateTime.Now);
            }
            return _response;
        }
        public async Task<APIResponseEntity> GetCategoryByIdAsync(int id)
        {
            APIResponseEntity _response = new APIResponseEntity();
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                _response.statusCode = 1;
                _response.status = "Success";
                _response.data = category;
            }
            catch (Exception ex)
            {
                _response.statusCode = 0;
                _response.status = "Failed";
                _logger.LogError(ex, "An error occurred in {ServiceName} at {Time}", nameof(CategoryService), DateTime.Now);
            }
            return _response;
        }
        private async Task<Categories> GetCategoryByNameAsync(string name)
        {
            if (await context.Categories.AnyAsync())
            {
                return await context.Categories.FirstOrDefaultAsync(n => n.Name == name);
            }
            return new Categories();
        }
        public async Task<APIResponseEntity> AddCategoryAsync(CategoryViewModel category)
        {
            APIResponseEntity _response = new APIResponseEntity();
            try
            {
                var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
                if (category.Id == 0)
                {
                    category.Id = await GetMaxId();
                    category.DataEnteredBy = user.Id;
                    var CategoryExist = await context.Categories.Where(c => c.Name == category.Name && c.IsActive).FirstOrDefaultAsync();
                    if (CategoryExist == null)
                    {
                        var Category = _mapper.Map<Categories>(category);
                        await _categoryRepository.AddAsync(Category);
                        _response.statusCode = 1;
                        _response.status = "Success";
                        _response.message = "Category created successfully";
                    }
                    else
                    {
                        _response.statusCode = 0;
                        _response.status = "Failed";
                        _response.message = "Category already exists";
                    }
                }
                else
                {
                    var existCategory = await GetCategoryByIdAsync(category.Id);
                    if (existCategory.data == null)
                    {
                        _response.statusCode = 0;
                        _response.status = "Failed";
                        _response.message = "Category not found";
                    }
                    else
                    {
                        var CategoryExist = await context.Categories.FirstOrDefaultAsync(c => c.Name == category.Name && c.Id != category.Id);
                        if (CategoryExist == null)
                        {
                            existCategory.data.Name = category.Name;
                            existCategory.data.Description = category.Description;
                            existCategory.data.DataModifiedBy = user.Id;
                            existCategory.data.DataModifiedOn = DateTime.Now;
                            await _categoryRepository.UpdateAsync(existCategory.data);
                            _response.statusCode = 1;
                            _response.status = "Success";
                            _response.message = "Category updated successfully";
                        }
                        else
                        {
                            _response.statusCode = 0;
                            _response.status = "Failed";
                            _response.message = "Category already exists";
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

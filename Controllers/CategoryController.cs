using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RestoApp.Models;
using RestoApp.Models.Entities;
using RestoApp.Models.ViewModels;
using RestoApp.Services;

namespace RestoApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly CategoryService _categoryService;
        public CategoryController(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        [HttpGet]
        public IActionResult Category() => View();
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var response = await _categoryService.GetCategoriesAsync();
            return Json(response);
        }
        [HttpGet]
        public async Task<IActionResult> GetCategoryById(int Id)
        {
            var response = await _categoryService.GetCategoryByIdAsync(Id);
            return Json(response);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(CategoryViewModel category)
        {
            APIResponseEntity _response = new APIResponseEntity();
            if (ModelState.IsValid)
            {
                _response = await _categoryService.AddCategoryAsync(category);
            }
            return Json(_response);
        }
    }
}

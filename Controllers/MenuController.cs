using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestoApp.Models.ViewModels;
using RestoApp.Models;
using RestoApp.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;

namespace RestoApp.Controllers
{
    [Authorize(Policy = "Permission_Menu")]
    public class MenuController : Controller
    {
        private readonly MenuService _menuService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MenuController(MenuService menuService, IWebHostEnvironment webHostEnvironment)
        {
            _menuService = menuService;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var model = _menuService.GetCategoriesAsync();
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> GetMenus()
        {
            var response = await _menuService.GetMenusAsync();
            return Json(response);
        }
        [HttpGet]
        public async Task<IActionResult> GetMenuById(int Id)
        {
            var response = await _menuService.GetMenuByIdAsync(Id);
            return Json(response);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMenu(MenuViewModel Menu)
        {
            APIResponseEntity _response = new APIResponseEntity();
            if (Menu.IsHalfAvailable && (Menu.HalfPrice == null || Menu.HalfPrice <= 0))
            {
                ModelState.AddModelError("HalfPrice", "Please enter a valid Half Menu Price greater than 0.");
                return Json(_response);
            }
            if (ModelState.IsValid)
            {
                string fileExtension = Menu.ImageFile == null ? "" : Path.GetExtension(Menu.ImageFile.FileName);
                _response = await _menuService.AddMenuAsync(Menu, fileExtension);
                if (_response.statusCode == 1)
                {
                    string imagePath = null;

                    if (Menu.ImageFile != null)
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "menuImages");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        string uniqueFileName = _response.data + "" + fileExtension;
                        imagePath = Path.Combine("menuImages", uniqueFileName);

                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await Menu.ImageFile.CopyToAsync(fileStream);
                        }
                    }
                }
            }
            return Json(_response);
        }
    }
}

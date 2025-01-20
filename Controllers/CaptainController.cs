using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestoApp.Models;
using RestoApp.Models.ViewModels;
using RestoApp.Services;

namespace RestoApp.Controllers
{
    [Authorize(Policy = "Permission_Captain")]
    public class CaptainController : Controller
    {
        private readonly CaptainService menuService;

        public CaptainController(CaptainService menuService)
        {
            this.menuService = menuService;
        }
        public async Task<IActionResult> Order(string table)
        {
            if (!string.IsNullOrEmpty(table))
            {
                var menus = await menuService.GetMenusAsync();
                return View(menus);
            }
            else
            {
                return View("Tables");
            }
        }
        public async Task<IActionResult> Tables()
        {
            var tables = await menuService.GetTablesAsync();
            return View(tables);
        }
        public async Task<IActionResult> GetHallTablesById(string tableId)
        {
            var result = await menuService.GetHallTableByIdAsync(tableId);
            return Json(result);
        }
        public async Task<IActionResult> GetPreviousOrders(int tableId)
        {
            var result = await menuService.GetPreviousOrdersAsync(tableId);
            return Json(result);
        }
        public async Task<IActionResult> AddOrders(OrderMasterViewModel order)
        {
            APIResponseEntity _response = new APIResponseEntity();
            if (ModelState.IsValid)
            {
                _response = await menuService.AddOrdersAsync(order);
            }
            return Json(_response);
        }
        public async Task<IActionResult> OrderBill(int tableId)
        {
            var result = await menuService.OrderBillAsync(tableId);
            return Json(result);
        }
    }
}

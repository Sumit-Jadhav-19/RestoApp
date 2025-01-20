using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestoApp.Services;

namespace RestoApp.Controllers
{
    [Authorize(Policy = "Permission_Chef")]
    public class KitchenController : Controller
    {
        private readonly KitchenService _kitchenService;
        public KitchenController(KitchenService kitchenService)
        {
            _kitchenService = kitchenService;
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> GetNotAcceptOrders()
        {
            var result = await _kitchenService.GetNotAcceptOrdersAsync();
            return Json(result);
        }
        public async Task<IActionResult> GetAcceptedOrders()
        {
            var result = await _kitchenService.GetAcceptedOrdersAsync();
            return Json(result);
        }
        public async Task<IActionResult> GetCompletedOrders()
        {
            var result = await _kitchenService.GetCompletedOrdersAsync();
            return Json(result);
        }
        [HttpPost]
        public async Task<IActionResult> AcceptOrder(long OrderId, int OrderCount)
        {
            var result = await _kitchenService.AcceptOrderAsync(OrderId, OrderCount);
            return Json(result);
        }
        [HttpPost]
        public async Task<IActionResult> ReadyOrder(long OrderId, int OrderCount)
        {
            var result = await _kitchenService.ReadyOrderAsync(OrderId, OrderCount);
            return Json(result);
        }
    }
}

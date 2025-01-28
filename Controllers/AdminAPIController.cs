using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestoApp.Services;

namespace RestoApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminAPIController : ControllerBase
    {
        private readonly AdminService _adminService;

        public AdminAPIController(AdminService adminService)
        {
            _adminService = adminService;
        }
        [HttpGet("GetPrintBillData/{OrderId}")]
        public IActionResult GetPrintBill(long OrderId)
        {
            var result =  _adminService.GetBilingDataAsync(OrderId);
            return Ok(result);
        }
    }
}

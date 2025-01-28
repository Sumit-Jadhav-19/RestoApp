using Azure;
using Microsoft.EntityFrameworkCore;
using RestoApp.Controllers;
using RestoApp.Data;
using RestoApp.Models;

namespace RestoApp.Services
{
    public class AdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminService> _logger;
        public AdminService(ApplicationDbContext context, ILogger<AdminService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public APIResponseEntity GetBilingDataAsync(long OrderId)
        {
            APIResponseEntity _response = new APIResponseEntity();
            try
            {
                var result = (from om in _context.OrderMasters
                              join od in _context.OrderDetails on om.OrderId equals od.OrderId
                              join m in _context.Menus on od.MenuId equals m.MenuId
                              join h in _context.Halls on om.HallId equals h.HallId
                              join t in _context.Tables on om.TableId equals t.Id
                              where om.OrderId == OrderId
                                    && om.IsActive
                                    && !om.OrderStatus
                                    && om.IsBillOrder
                                    && !om.IsBillPayed
                                    && string.IsNullOrEmpty(om.PaymentType)
                              group new
                              {
                                  om.OrderId,
                                  om.HallId,
                                  h.HallName,
                                  om.TableId,
                                  tableName = t.Name,
                                  m.Name,
                                  m.Price,
                                  od.Quantity,
                                  od.Size
                              } by new { om.OrderId, om.HallId, om.TableId } into g
                              select new
                              {
                                  OrderId = g.Key.OrderId,
                                  Hall = g.FirstOrDefault().HallName,
                                  Table = g.FirstOrDefault().tableName,
                                  Menus = g.GroupBy(x => new { x.Name, x.Size, x.Price })
                                           .Select(menuGroup => new
                                           {
                                               MenuName = menuGroup.Key.Name,
                                               Size = menuGroup.Key.Size,
                                               Quantity = menuGroup.Sum(x => x.Quantity),
                                               Price = Convert.ToDecimal(menuGroup.Key.Price).ToString("0.00"),
                                               TotalPrice = Convert.ToDecimal(menuGroup.Key.Price * menuGroup.Sum(x => x.Quantity)).ToString("0.00")
                                           })
                                           .ToList(),
                                  Subtotal = Convert.ToDecimal(g.Sum(x => x.Price * x.Quantity)).ToString("0.00"),
                                  Tax = Convert.ToDecimal(g.Sum(x => x.Price * x.Quantity) * Convert.ToDecimal(0.02)).ToString("0.00"),
                                  Total = Convert.ToDecimal(g.Sum(x => x.Price * x.Quantity) * Convert.ToDecimal(1.02)).ToString("0.00")
                              }).ToList();
                _response.status = "Success";
                _response.statusCode = 1;
                _response.data = result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in {ServiceName} at {Time}", nameof(AdminService), DateTime.Now);
            }
            return _response;
        }
    }
}

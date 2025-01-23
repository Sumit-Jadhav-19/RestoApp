using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RestoApp.Data;
using RestoApp.Models;
using RestoApp.Models.Entities;
using RestoApp.Models.ViewModels;
using RestoApp.Repository;
using System;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace RestoApp.Services
{
    public class CaptainService
    {
        private readonly IRepository<OrderMaster> _orderRepository;
        private readonly IRepository<OrderDetails> _orderDetailsRepository;
        private readonly ApplicationDbContext context;
        private readonly ILogger<CaptainService> _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHubContext<ChatHub> _hubContext;

        public CaptainService(ApplicationDbContext context, ILogger<CaptainService> logger,
            IMapper mapper, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor, IRepository<OrderMaster> orderRepository, IRepository<OrderDetails> orderDetailsRepository, IHubContext<ChatHub> hubContext)
        {
            this.context = context;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _orderRepository = orderRepository;
            _orderDetailsRepository = orderDetailsRepository;
            _hubContext = hubContext;
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
                                   group new
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
                                   } by menu.CategoryId into menuGroup
                                   select new
                                   {
                                       CategoryId = menuGroup.Key,
                                       CategoryName = menuGroup.FirstOrDefault().CategoryName,
                                       Menus = menuGroup.ToList()
                                   }).ToListAsync();
                _response.statusCode = 1;
                _response.status = "Success";
                _response.data = menus;
            }
            catch (Exception ex)
            {
                _response.statusCode = 0;
                _response.status = "Failed";
                _logger.LogError(ex, "An error occurred in {ServiceName} at {Time}", nameof(CaptainService), DateTime.Now);
            }
            return _response;
        }
        public async Task<APIResponseEntity> GetTablesAsync()
        {
            APIResponseEntity _response = new APIResponseEntity();
            try
            {
                var tables = await (from t in context.Tables
                                    join h in context.Halls on t.HallId equals h.HallId
                                    group new
                                    {
                                        t.Id,
                                        t.Name,
                                        h.HallId,
                                        h.HallName,
                                        t.IsOccupied
                                    } by new { h.HallId } into hall
                                    select new
                                    {
                                        HallId = hall.Key.HallId,
                                        HallName = hall.FirstOrDefault().HallName,
                                        tables = hall.ToList()
                                    }).ToListAsync();
                _response.statusCode = 1;
                _response.status = "Success";
                _response.data = tables;
            }
            catch (Exception ex)
            {
                _response.statusCode = 0;
                _response.status = "Failed";
                _logger.LogError(ex, "An error occurred in {ServiceName} at {Time}", nameof(CaptainService), DateTime.Now);
            }
            return _response;
        }
        public async Task<APIResponseEntity> GetHallTableByIdAsync(string Id)
        {
            APIResponseEntity _response = new APIResponseEntity();
            try
            {
                if (!string.IsNullOrEmpty(Id))
                {
                    var result = await (from t in context.Tables
                                        join h in context.Halls on t.HallId equals h.HallId
                                        where t.Id == Convert.ToInt32(Id)
                                        select new
                                        {
                                            t.Id,
                                            t.Name,
                                            t.HallId,
                                            h.HallName
                                        }).FirstOrDefaultAsync();
                    _response.statusCode = 1;
                    _response.status = "Success";
                    _response.data = result;
                }
            }
            catch (Exception ex)
            {
                _response.statusCode = 0;
                _response.status = "Failed";
                _logger.LogError(ex, "An error occurred in {ServiceName} at {Time}", nameof(CaptainService), DateTime.Now);
            }
            return _response;
        }
        public async Task<APIResponseEntity> GetPreviousOrdersAsync(int tabelId)
        {
            APIResponseEntity _response = new APIResponseEntity();
            try
            {
                int hallId = Convert.ToInt32(await context.Tables.Where(t => t.Id == tabelId).Select(t => t.HallId).FirstOrDefaultAsync());
                var menus = await (from od in context.OrderDetails
                                   join m in context.Menus on od.MenuId equals m.MenuId
                                   join om in context.OrderMasters on od.OrderId equals om.OrderId
                                   where om.TableId == tabelId && om.IsActive == true && od.IsActive == true && om.OrderStatus == false && om.HallId == hallId
                                   select new
                                   {
                                       om.OrderId,
                                       od.DetailsId,
                                       m.Name,
                                       od.Quantity,
                                       od.Size,
                                       od.IsAccept,
                                       od.OrderStatus
                                   }).ToListAsync();
                _response.statusCode = 1;
                _response.status = "Success";
                _response.data = menus;
            }
            catch (Exception ex)
            {
                _response.statusCode = 0;
                _response.status = "Failed";
                _logger.LogError(ex, "An error occurred in {ServiceName} at {Time}", nameof(CaptainService), DateTime.Now);
            }
            return _response;
        }
        public async Task<APIResponseEntity> AddOrdersAsync(OrderMasterViewModel order)
        {
            APIResponseEntity _response = new APIResponseEntity();
            try
            {
                if (order.OrderDetails.Count > 0)
                {
                    var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
                    if (order.OrderId == 0)
                    {
                        order.DataEnteredBy = user.Id;
                        order.CaptainId = user.Id;
                        order.HallId = await context.Tables.Where(t => t.Id == order.TableId).Select(t => t.HallId).FirstOrDefaultAsync();
                        order.OrderId = Convert.ToInt64(DateTime.Now.ToString("yyMMddHHmmssfff"));
                        var orders = _mapper.Map<OrderMaster>(order);
                        await _orderRepository.AddAsync(orders);

                        var existingTable = await context.Tables.Where(t => t.Id == order.TableId).FirstOrDefaultAsync();
                        if (existingTable != null)
                        {
                            existingTable.IsOccupied = true;
                            context.Tables.Update(existingTable);
                            await context.SaveChangesAsync();
                        }
                    }
                    int orderCount = 1;
                    if (await context.OrderDetails.AnyAsync())
                    {
                        orderCount += await context.OrderDetails.Where(o => o.OrderId == order.OrderId).OrderByDescending(o => o.DetailsId).Select(o => o.OrderCount).FirstOrDefaultAsync();
                    }
                    foreach (var orderDetails in order.OrderDetails)
                    {
                        orderDetails.DetailsId = Convert.ToInt64(DateTime.Now.ToString("yyMMddHHmmssfff"));
                        orderDetails.DataEnteredBy = user.Id;
                        orderDetails.OrderId = order.OrderId;
                        orderDetails.OrderCount = orderCount;
                        orderDetails.Price = await context.Menus.Where(m => m.MenuId == orderDetails.MenuId).Select(m => m.Price).FirstOrDefaultAsync();
                        orderDetails.SubTotal = orderDetails.Price * orderDetails.Quantity;
                        orderDetails.CategoryId = await context.Menus.Where(c => c.MenuId == orderDetails.MenuId).Select(m => m.CategoryId).FirstOrDefaultAsync();
                        var details = _mapper.Map<OrderDetails>(orderDetails);
                        await _orderDetailsRepository.AddAsync(details);
                    }

                    var kitchenRolesUser = await (from con in context.UserConnections
                                                  join u in context.Users on con.UserId equals u.Id
                                                  join ur in context.UserRoles on u.Id equals ur.UserId
                                                  join r in context.Roles on ur.RoleId equals r.Id
                                                  where r.Name.ToUpper() == "CHEF"
                                                  select new
                                                  {
                                                      con.ConnectionId
                                                  }).ToListAsync();
                    foreach (var u in kitchenRolesUser)
                    {
                        await _hubContext.Clients.Client(u.ConnectionId.ToString()).SendAsync("ReceiveMessage", "System", "KitchenOrder");
                    }
                    _response.statusCode = 1;
                    _response.status = "Success";
                    _response.message = "Order added successfully";
                }
            }
            catch (Exception ex)
            {
                _response.statusCode = 0;
                _response.status = "Failed";
                _logger.LogError(ex, "An error occurred in {ServiceName} at {Time}", nameof(CaptainService), DateTime.Now);
            }
            return _response;
        }

        public async Task<APIResponseEntity> OrderBillAsync(int tableId)
        {
            APIResponseEntity _response = new APIResponseEntity();
            try
            {
                var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
                var existingOrder = await context.OrderMasters.Where(o => o.TableId == tableId && !o.OrderStatus).FirstOrDefaultAsync();
                if (existingOrder is not null)
                {
                    var orderDetails = await context.OrderDetails.Where(o => o.OrderId == existingOrder.OrderId && o.OrderStatus.ToUpper() != "C").ToListAsync();
                    if (orderDetails.Count > 0)
                    {
                        _response.statusCode = 0;
                        _response.status = "Failed";
                        _response.message = "Wait for order complete !";
                        return _response;
                    }
                    existingOrder.IsBillOrder = true;
                    existingOrder.DataModifiedOn = DateTime.Now;
                    existingOrder.DataModifiedBy = user.Id;
                    context.OrderMasters.Update(existingOrder);
                    await context.SaveChangesAsync();
                    _response.statusCode = 1;
                    _response.status = "Success";
                    _response.message = "Bill Ordered successfully";
                }
            }
            catch (Exception ex)
            {
                _response.statusCode = 0;
                _response.status = "Failed";
                _logger.LogError(ex, "An error occurred in {ServiceName} at {Time}", nameof(CaptainService), DateTime.Now);
            }
            return _response;
        }
        public async Task<APIResponseEntity> DeletePrevOrderItemAsync(long DetailsId)
        {
            APIResponseEntity _response = new APIResponseEntity();
            try
            {
                var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
                var IsAcceptorderDetails = await context.OrderDetails.Where(o => o.DetailsId == DetailsId && o.IsAccept).FirstOrDefaultAsync();
                if (IsAcceptorderDetails == null)
                {
                    var orderDetails = await context.OrderDetails.Where(o => o.DetailsId == DetailsId && !o.IsAccept).FirstOrDefaultAsync();
                    if (orderDetails is not null)
                    {
                        orderDetails.IsActive = false;
                        orderDetails.OrderStatus = "C";
                        orderDetails.DataModifiedBy = user.Id;  
                        orderDetails.DataModifiedOn = DateTime.Now;
                        context.OrderDetails.Update(orderDetails);
                        await context.SaveChangesAsync();
                        _response.statusCode = 1;
                        _response.status = "Success";
                        _response.message = "Order item Cancled successfully";
                    }
                    else
                    {
                        _response.statusCode = 0;
                        _response.status = "Failed";
                        _response.message = "Order item not found";
                    }
                }
                else
                {
                    _response.statusCode = 0;
                    _response.status = "Failed";
                    _response.message = "Order is accepted you can't cancle this item !";
                }
            }
            catch (Exception ex)
            {
                _response.statusCode = 0;
                _response.status = "Failed";
                _logger.LogError(ex, "An error occurred in {ServiceName} at {Time}", nameof(CaptainService), DateTime.Now);
            }
            return _response;
        }
        public async Task<APIResponseEntity> UpdatePrevOrderItemAsync(long DetailsId, int Qty, string Size)
        {
            APIResponseEntity _response = new APIResponseEntity();
            try
            {
                var orderDetails = await context.OrderDetails.Where(o => o.DetailsId == DetailsId && !o.IsAccept).FirstOrDefaultAsync();
                if (orderDetails is not null)
                {
                    orderDetails.Quantity = Qty;
                    orderDetails.Size = Size;
                    orderDetails.SubTotal = orderDetails.Price * Qty;
                    context.OrderDetails.Update(orderDetails);
                    await context.SaveChangesAsync();
                    var kitchenRolesUser = await (from con in context.UserConnections
                                                  join u in context.Users on con.UserId equals u.Id
                                                  join ur in context.UserRoles on u.Id equals ur.UserId
                                                  join r in context.Roles on ur.RoleId equals r.Id
                                                  where r.Name.ToUpper() == "CHEF"
                                                  select new
                                                  {
                                                      con.ConnectionId
                                                  }).ToListAsync();
                    foreach (var u in kitchenRolesUser)
                    {
                        await _hubContext.Clients.Client(u.ConnectionId.ToString()).SendAsync("ReceiveMessage", "System", "KitchenOrder");
                    }
                    _response.statusCode = 1;
                    _response.status = "Success";
                    _response.message = "Order item updated successfully";
                }
                else
                {
                    _response.statusCode = 0;
                    _response.status = "Failed";
                    _response.message = "Order item is Accepted !";
                }
            }
            catch (Exception ex)
            {
                _response.statusCode = 0;
                _response.status = "Failed";
                _logger.LogError(ex, "An error occurred in {ServiceName} at {Time}", nameof(CaptainService), DateTime.Now);
            }
            return _response;
        }
    }
}

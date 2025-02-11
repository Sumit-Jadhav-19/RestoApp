﻿using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RestoApp.Data;
using RestoApp.Models;
using RestoApp.Models.Entities;
using RestoApp.Repository;

namespace RestoApp.Services
{

    public class KitchenService
    {
        private readonly IRepository<OrderMaster> _orderRepository;
        private readonly IRepository<OrderDetails> _orderDetailsRepository;
        private readonly ApplicationDbContext context;
        private readonly ILogger<KitchenService> _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHubContext<ChatHub> _hubContext;
        public KitchenService(ApplicationDbContext context, ILogger<KitchenService> logger,
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

        public async Task<APIResponseEntity> GetNotAcceptOrdersAsync()
        {
            APIResponseEntity _response = new APIResponseEntity();
            try
            {
                var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
                var orders = await (from om in context.OrderMasters
                                    join od in context.OrderDetails on om.OrderId equals od.OrderId
                                    join m in context.Menus on od.MenuId equals m.MenuId
                                    where od.IsAccept == false && string.IsNullOrEmpty(od.OrderStatus) && od.IsActive
                                    group new
                                    {
                                        od.OrderId,
                                        od.DataEnteredOn,
                                        od.MenuId,
                                        m.Name,
                                        od.Quantity,
                                        od.Size,
                                        od.OrderCount
                                    } by new { od.OrderId, od.OrderCount } into order
                                    select new
                                    {
                                        OrderId = order.Key.OrderId,
                                        OrderCount = order.Key.OrderCount,
                                        OrderDate = order.FirstOrDefault().DataEnteredOn,
                                        orders = order.ToList(),
                                    }).OrderBy(o => o.OrderDate).ToListAsync();
                _response.statusCode = 1;
                _response.status = "Success";
                _response.data = orders;
            }
            catch (Exception ex)
            {
                _response.statusCode = 0;
                _response.status = "Failed";
                _logger.LogError(ex, "An error occurred in {ServiceName} at {Time}", nameof(KitchenService), DateTime.Now);
            }
            return _response;
        }
        public async Task<APIResponseEntity> GetAcceptedOrdersAsync()
        {
            APIResponseEntity _response = new APIResponseEntity();
            try
            {
                var orders = await (from om in context.OrderMasters
                                    join od in context.OrderDetails on om.OrderId equals od.OrderId
                                    join m in context.Menus on od.MenuId equals m.MenuId
                                    where od.IsAccept && !string.IsNullOrEmpty(od.OrderStatus) && od.OrderStatus == "P"
                                    group new
                                    {
                                        od.OrderId,
                                        od.MenuId,
                                        m.Name,
                                        od.Quantity,
                                        od.Size,
                                        om.OrderStatus,
                                        od.DataModifiedOn,
                                        od.OrderCount
                                    } by new { od.OrderId, od.OrderCount } into order
                                    select new
                                    {
                                        OrderId = order.Key.OrderId,
                                        OrderCount = order.Key.OrderCount,
                                        AcceptedDate = order.FirstOrDefault().DataModifiedOn,
                                        OrderStatus = order.FirstOrDefault().OrderStatus,
                                        orders = order.ToList(),
                                    }).OrderBy(o => o.OrderStatus).ToListAsync();
                _response.statusCode = 1;
                _response.status = "Success";
                _response.data = orders;
            }
            catch (Exception ex)
            {
                _response.statusCode = 0;
                _response.status = "Failed";
                _logger.LogError(ex, "An error occurred in {ServiceName} at {Time}", nameof(KitchenService), DateTime.Now);
            }
            return _response;
        }
        public async Task<APIResponseEntity> GetCompletedOrdersAsync()
        {
            APIResponseEntity _response = new APIResponseEntity();
            try
            {
                DateTime date = DateTime.Now;
                var orders = await (from om in context.OrderMasters
                                    join od in context.OrderDetails on om.OrderId equals od.OrderId
                                    join m in context.Menus on od.MenuId equals m.MenuId
                                    where od.IsAccept && !string.IsNullOrEmpty(od.OrderStatus) && od.OrderStatus == "C" && om.OrderDateTime.Date >= date.Date.AddDays(-1) && om.OrderDateTime.Date <= date.Date.AddDays(1)
                                    group new
                                    {
                                        od.OrderId,
                                        od.MenuId,
                                        m.Name,
                                        od.Quantity,
                                        od.Size,
                                        om.OrderStatus,
                                        od.DataModifiedOn,
                                        od.OrderCount,
                                        om.OrderDateTime
                                    } by new { od.OrderId, od.OrderCount } into order
                                    select new
                                    {
                                        OrderId = order.Key.OrderId,
                                        OrderCount = order.Key.OrderCount,
                                        AcceptedDate = order.FirstOrDefault().DataModifiedOn,
                                        OrderStatus = order.FirstOrDefault().OrderStatus,
                                        OrderDatetime = order.FirstOrDefault().OrderDateTime,
                                        orders = order.ToList(),
                                    }).OrderByDescending(o => o.OrderDatetime).ToListAsync();
                _response.statusCode = 1;
                _response.status = "Success";
                _response.data = orders;
            }
            catch (Exception ex)
            {
                _response.statusCode = 0;
                _response.status = "Failed";
                _logger.LogError(ex, "An error occurred in {ServiceName} at {Time}", nameof(KitchenService), DateTime.Now);
            }
            return _response;
        }
        public async Task<APIResponseEntity> AcceptOrderAsync(long OrderId, int orderCount)
        {
            APIResponseEntity _response = new APIResponseEntity();
            try
            {
                var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
                var existingOrder = await context.OrderDetails.Where(o => o.OrderId == OrderId && string.IsNullOrEmpty(o.OrderStatus) && o.OrderCount == orderCount).ToListAsync();
                if (existingOrder != null)
                {
                    foreach (var order in existingOrder)
                    {
                        order.IsAccept = true;
                        order.OrderStatus = "P";
                        order.DataModifiedBy = user.Id;
                        order.DataModifiedOn = DateTime.Now;
                        context.OrderDetails.Update(order);
                        await context.SaveChangesAsync();
                    }
                    var captain = await (from con in context.UserConnections
                                         join om in context.OrderMasters on con.UserId equals om.CaptainId
                                         where om.OrderId == OrderId
                                         select new
                                         {
                                             con.ConnectionId
                                         }).FirstOrDefaultAsync();
                    await _hubContext.Clients.Client(captain.ConnectionId.ToString()).SendAsync("ReceiveMessage", "System", "OREDERACCEPT");
                    _response.statusCode = 1;
                    _response.status = "Success";
                    _response.message = "Order accepeted !";
                    _response.data = existingOrder;
                }
            }
            catch (Exception ex)
            {
                _response.statusCode = 0;
                _response.status = "Failed";
                _logger.LogError(ex, "An error occurred in {ServiceName} at {Time}", nameof(KitchenService), DateTime.Now);
            }
            return _response;
        }
        public async Task<APIResponseEntity> ReadyOrderAsync(long OrderId, int OrderCount)
        {
            APIResponseEntity _response = new APIResponseEntity();
            try
            {
                var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
                var existingOrderDetails = await context.OrderDetails.Where(o => o.OrderId == OrderId && o.OrderCount == OrderCount).ToListAsync();
                if (existingOrderDetails != null)
                {
                    foreach (var order in existingOrderDetails)
                    {
                        order.OrderStatus = "C";
                        context.OrderDetails.Update(order);
                        await context.SaveChangesAsync();
                    }
                    var captain = await (from con in context.UserConnections
                                         join om in context.OrderMasters on con.UserId equals om.CaptainId
                                         where om.OrderId == OrderId
                                         select new
                                         {
                                             con.ConnectionId
                                         }).FirstOrDefaultAsync();
                    var tableDetails = await (from om in context.OrderMasters
                                              where om.OrderId == OrderId
                                              select new
                                              {
                                                  om.TableId,
                                                  table = context.Tables.Where(t => t.Id == om.TableId).Select(t => t.Name).FirstOrDefault(),
                                                  hall=context.Halls.Where(h => h.HallId == om.HallId).Select(h => h.HallName).FirstOrDefault()
                                              }).FirstOrDefaultAsync();
                    await _hubContext.Clients.Client(captain.ConnectionId.ToString()).SendAsync("ReceiveMessage", "System", $"Your order is ready, please pickup and serve. Table {tableDetails.table}- Hall {tableDetails.hall}.");
                    _response.statusCode = 1;
                    _response.status = "Success";
                    _response.message = "Order completed !";
                    _response.data = existingOrderDetails;
                }
            }
            catch (Exception ex)
            {
                _response.statusCode = 0;
                _response.status = "Failed";
                _logger.LogError(ex, "An error occurred in {ServiceName} at {Time}", nameof(KitchenService), DateTime.Now);
            }
            return _response;
        }
    }
}

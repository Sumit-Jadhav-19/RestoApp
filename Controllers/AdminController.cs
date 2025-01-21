using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RestoApp.Data;
using RestoApp.Models;
using RestoApp.Models.Entities;
using RestoApp.Models.ViewModels;
using RestoApp.Services;
using System.Data;
using System.Globalization;

namespace RestoApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly HallService _hallService;
        private readonly TableService _tableService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            HallService hallService, TableService tableService, ILogger<AdminController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            this._hallService = hallService;
            this._tableService = tableService;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
        [Authorize(Policy = "Permission_User")]
        public IActionResult Registration()
        {
            return View();
        }
        [HttpGet]
        public IActionResult GetAllRegisteredUsers()
        {
            //var users = _userManager.Users.ToList();
            var users = from n in _context.Users.ToList()
                        select new RegisterViewModel
                        {
                            UserName = n.UserName,
                            Email = n.Email,
                            FirstName = n.FirstName,
                            LastName = n.LastName,
                            DateOfBirth = n.DateOfBirth,
                            ProfilePictureUrl = n.ProfilePictureUrl,
                            PhoneNumber = n.PhoneNumber,
                            UserId = n.Id.ToString()
                        };
            APIResponseEntity _response = new APIResponseEntity();
            _response.statusCode = 1;
            _response.status = "Success";
            _response.data = users;
            return Json(_response);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            APIResponseEntity _response = new APIResponseEntity();
            if (ModelState.IsValid)
            {
                UserService userService = new UserService(_context);
                int id = await userService.GetMaxUserIdAsync();
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser == null)
                {
                    var user = new ApplicationUser { Id = id, UserName = model.Email, Email = model.Email, FirstName = model.FirstName, LastName = model.LastName, ProfilePictureUrl = "", DateOfBirth = model.DateOfBirth };
                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        //// Optionally sign the user in after registration
                        //return RedirectToAction("Index", "Home");
                        _response.statusCode = 1;
                        _response.status = "Success";
                        _response.message = "User added successfully !";
                    }
                    else
                    {
                        _response.statusCode = 0;
                        _response.status = "Failed";
                        foreach (var error in result.Errors)
                        {
                            _response.message = $"{error.Description}";
                        }
                    }
                }
                else
                {
                    _response.statusCode = 0;
                    _response.status = "Failed";
                    _response.message = "Email Id is Exist!";
                }
            }
            return Json(_response);
        }
        [Authorize(Policy = "Permission_User")]
        public IActionResult UserRoles() => View();
        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            APIResponseEntity _response = new APIResponseEntity();
            var roles = await _roleManager.Roles
                .Where(e => e.Is_Active)
                .Select(e => new { e.Id, e.Name })
                .ToListAsync();
            _response.statusCode = 1;
            _response.status = "Success";
            _response.data = roles;
            return Json(_response);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(RoleViewModel role)
        {
            APIResponseEntity _response = new APIResponseEntity();
            if (ModelState.IsValid)
            {
                if (role.Id == 0)
                {
                    var roleExist = await _roleManager.RoleExistsAsync(role.Name);
                    if (!roleExist)
                    {
                        var id = await _context.Roles.MaxAsync(e => e.Id);
                        var roles = new ApplicationRole { Id = id + 1, Name = role.Name, Is_Active = true, NormalizedName = role.Name.ToUpper(), ConcurrencyStamp = "" };
                        var result = await _roleManager.CreateAsync(roles);

                        if (result.Succeeded)
                        {
                            _response.statusCode = 1;
                            _response.status = "Success";
                            _response.message = "Role created successfully";
                        }
                        else
                        {
                            _response.statusCode = 0;
                            _response.status = "Failed";
                            _response.message = "Error creating role";
                        }
                    }
                    else
                    {
                        _response.statusCode = 0;
                        _response.status = "Failed";
                        _response.message = "Role already exists";
                    }
                }
                else
                {
                    var existRole = await _roleManager.FindByIdAsync(role.Id.ToString());
                    if (existRole == null)
                    {
                        _response.statusCode = 0;
                        _response.status = "Failed";
                        _response.message = "Role not found";
                    }

                    // Update the role name
                    existRole.Name = role.Name;

                    var result = await _roleManager.UpdateAsync(existRole);
                    if (result.Succeeded)
                    {
                        _response.statusCode = 1;
                        _response.status = "Success";
                        _response.message = "Role updated successfully";
                    }
                    else
                    {
                        _response.statusCode = 0;
                        _response.status = "Failed";
                        _response.message = "Error updating role";
                    }
                }

            }
            return Json(_response);
        }
        [HttpGet]
        public async Task<IActionResult> GetRoleById(int id)
        {
            APIResponseEntity _response = new APIResponseEntity();
            var role = await _roleManager.Roles
                .FirstOrDefaultAsync(e => e.Id == id & e.Is_Active);
            _response.statusCode = 1;
            _response.status = "Success";
            _response.data = role;
            return Json(_response);
        }
        [Authorize(Policy = "Permission_User")]
        [HttpGet]
        public IActionResult RolePermissions() => View();

        [HttpGet]
        public IActionResult GetRolesAndPermissions()
        {
            APIResponseEntity _response = new APIResponseEntity();
            var result = (from r in _context.Users
                          join rp in _context.UserPermissions on r.Id equals rp.UserId into rpGroup
                          from rp in rpGroup.DefaultIfEmpty() // Left Join
                          join p in _context.Permissions on rp.PermissionId equals p.Id into pGroup
                          from p in pGroup.DefaultIfEmpty() // Left Join
                          group new { r, p } by new { r.Id, r.UserName } into g
                          select new
                          {
                              RoleId = g.Key.Id,
                              RoleName = g.Key.UserName,
                              Table = g.Count(x => x.p.Name == "Table"),
                              User = g.Count(x => x.p.Name == "User"),
                              Menu = g.Count(x => x.p.Name == "Menu"),
                              Captain = g.Count(x => x.p.Name == "Captain"),
                              Chef = g.Count(x => x.p.Name == "Chef")
                          }).OrderBy(r => r.RoleId).ToList();
            _response.statusCode = 1;
            _response.status = "Success";
            _response.data = result;
            return Json(_response);
        }
        [HttpPost]
        public IActionResult AddRolePermission(int role, int permission)
        {
            APIResponseEntity _response = new APIResponseEntity();
            var existPermission = _context.UserPermissions.Where(r => r.UserId == role && r.PermissionId == permission).FirstOrDefault();
            if (existPermission != null)
            {
                _context.UserPermissions.Remove(existPermission);
                _context.SaveChanges();
            }
            else
            {
                var max = _context.UserPermissions.Max(x => x.Id) + 1;
                UserPermission rolePermission = new UserPermission()
                {
                    Id = max,
                    UserId = role,
                    PermissionId = permission
                };
                _context.UserPermissions.Add(rolePermission);
                _context.SaveChanges();
            }
            return Json(_response);
        }
        [Authorize(Policy = "Permission_Table")]
        [HttpGet]
        public IActionResult Halls() => View();
        [HttpGet]
        public async Task<IActionResult> GetHalls()
        {
            var response = await _hallService.GetHallAsync();
            return Json(response);
        }
        [HttpGet]
        public async Task<IActionResult> GetHallById(int Id)
        {
            var response = await _hallService.GetHallByIdAsync(Id);
            return Json(response);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddHall(HallViewModel hallViewModel)
        {
            APIResponseEntity _response = new APIResponseEntity();
            if (ModelState.IsValid)
            {
                _response = await _hallService.AddHallAsync(hallViewModel);
            }
            return Json(_response);
        }
        [Authorize(Policy = "Permission_Table")]
        public IActionResult Tables() => View(_tableService.GetHallsAsync());
        [HttpGet]
        public async Task<IActionResult> GetTables()
        {
            var response = await _tableService.GetTableAsync();
            return Json(response);
        }
        [HttpGet]
        public async Task<IActionResult> GetTableById(int Id)
        {
            var response = await _tableService.GetTableByIdAsync(Id);
            return Json(response);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTable(TableViewModel TableViewModel)
        {
            APIResponseEntity _response = new APIResponseEntity();
            if (ModelState.IsValid)
            {
                _response = await _tableService.AddTableAsync(TableViewModel);
            }
            return Json(_response);
        }
        [HttpGet]
        public async Task<IActionResult> GetOrderdBills()
        {
            APIResponseEntity _response = new APIResponseEntity();
            try
            {
                var result = await (from om in _context.OrderMasters
                                    join od in _context.OrderDetails on om.OrderId equals od.OrderId
                                    join m in _context.Menus on od.MenuId equals m.MenuId
                                    join h in _context.Halls on om.HallId equals h.HallId
                                    join t in _context.Tables on om.TableId equals t.Id
                                    join u in _context.Users on om.DataEnteredBy equals u.Id
                                    where om.IsBillOrder && !om.IsBillPayed && !om.OrderStatus && string.IsNullOrEmpty(om.PaymentType)
                                    group
                                    new
                                    {
                                        om.OrderId,
                                        om.OrderStatus,
                                        om.HallId,
                                        h.HallName,
                                        om.TableId,
                                        t.Name,
                                        om.DataEnteredBy,
                                        u.UserName,
                                        m.Price,
                                        om.OrderDateTime
                                    } by
                                    new { om.OrderId } into g
                                    select new
                                    {
                                        Order = g.Key.OrderId,
                                        HallName = g.FirstOrDefault().HallName,
                                        TableName = g.FirstOrDefault().Name,
                                        OrderDate = g.FirstOrDefault().OrderDateTime,
                                        Total = g.Sum(x => x.Price),
                                        tables = g.ToList()
                                    }).ToListAsync();
                _response.status = "Success";
                _response.statusCode = 1;
                _response.data = result;
            }
            catch (Exception ex)
            {
                _response.statusCode = 0;
                _response.status = "Failed";
                _logger.LogError(ex, "An error occurred in {ServiceName} at {Time}", nameof(CaptainService), DateTime.Now);
            }
            return Json(_response);
        }
        public async Task<IActionResult> GetOrderdBillsById(long OrderId)
        {
            APIResponseEntity _response = new APIResponseEntity();
            try
            {
                var result = await (from om in _context.OrderMasters
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
                                                     Price = menuGroup.Key.Price,
                                                     TotalPrice = menuGroup.Key.Price * menuGroup.Sum(x => x.Quantity)
                                                 })
                                                 .ToList(),
                                        Subtotal = g.Sum(x => x.Price * x.Quantity),
                                        Tax = Convert.ToDecimal(g.Sum(x => x.Price * x.Quantity)) * Convert.ToDecimal(0.02),
                                        Total = Convert.ToDecimal(g.Sum(x => x.Price * x.Quantity)) * Convert.ToDecimal(1.02)
                                    }).ToListAsync();
                _response.status = "Success";
                _response.statusCode = 1;
                _response.data = result;
            }
            catch (Exception ex)
            {
                _response.statusCode = 0;
                _response.status = "Failed";
                _logger.LogError(ex, "An error occurred in {ServiceName} at {Time}", nameof(CaptainService), DateTime.Now);
            }
            return Json(_response);
        }
        [HttpPost]
        public async Task<IActionResult> PaymentConfirmed(long OrderId, string paymentType)
        {
            APIResponseEntity _response = new APIResponseEntity();
            try
            {
                if (string.IsNullOrEmpty(paymentType) || paymentType == "0")
                {
                    _response.statusCode = 2;
                    _response.status = "pending";
                    _response.message = "please select payment type";
                    return Json(_response);
                }

                var existingOrder = await _context.OrderMasters.FirstOrDefaultAsync(e => e.OrderId == OrderId);
                if (existingOrder != null)
                {
                    existingOrder.OrderStatus = true;
                    existingOrder.IsBillPayed = true;
                    existingOrder.PaymentType = paymentType;
                    existingOrder.DataModifiedBy = 1;
                    existingOrder.DataModifiedOn = DateTime.Now;
                    _context.OrderMasters.Update(existingOrder);
                    await _context.SaveChangesAsync();

                    var existingTable = await _context.Tables.Where(t => t.Id == existingOrder.TableId).FirstOrDefaultAsync();
                    if (existingTable != null)
                    {
                        existingTable.IsOccupied = false;
                        _context.Tables.Update(existingTable);
                        await _context.SaveChangesAsync();
                    }
                    _response.statusCode = 1;
                    _response.status = "Success";
                    _response.message = "Payment Confirmed";
                }
            }
            catch (Exception ex)
            {
                _response.statusCode = 0;
                _response.status = "Failed";
                _logger.LogError(ex, "An error occurred in {ServiceName} at {Time}", nameof(CaptainService), DateTime.Now);
            }
            return Json(_response);
        }

        [HttpGet]
        public async Task<IActionResult> GetTop100Orders()
        {
            APIResponseEntity _response = new APIResponseEntity();
            try
            {
                var result = await (from om in _context.OrderMasters
                                    join h in _context.Halls on om.HallId equals h.HallId
                                    join t in _context.Tables on om.TableId equals t.Id
                                    where om.IsActive
                                    select new
                                    {
                                        om.OrderId,
                                        om.HallId,
                                        h.HallName,
                                        om.TableId,
                                        t.Name,
                                        om.CaptainId,
                                        CaptainName = _context.Users.Where(u => u.Id == om.CaptainId).Select(u => u.FirstName).FirstOrDefault() + " " + _context.Users.Where(u => u.Id == om.CaptainId).Select(u => u.LastName).FirstOrDefault(),
                                        om.OrderDateTime,
                                        om.ChefId,
                                        ChefName = _context.Users.Where(u => u.Id == om.ChefId).Select(u => u.FirstName).FirstOrDefault() + " " + _context.Users.Where(u => u.Id == om.ChefId).Select(u => u.LastName).FirstOrDefault(),
                                        om.OrderStatus,
                                        om.IsBillPayed,
                                        om.PaymentType,
                                        TotalPayment = _context.OrderDetails
                                                        .Where(od => od.OrderId == om.OrderId)
                                                        .Sum(od => od.Price * od.Quantity)
                                    })
                    .OrderByDescending(x => x.OrderDateTime)
                    .Take(100)
                    .ToListAsync();
                _response.status = "Success";
                _response.statusCode = 1;
                _response.data = result;
            }
            catch (Exception ex)
            {
                _response.statusCode = 0;
                _response.status = "Failed";
                _logger.LogError(ex, "An error occurred in {ServiceName} at {Time}", nameof(CaptainService), DateTime.Now);
            }
            return Json(_response);
        }
        [HttpGet]
        public async Task<IActionResult> GetOrderDetailsById(long OrderId)
        {
            APIResponseEntity _response = new APIResponseEntity();
            try
            {
                var result = await (from od in _context.OrderDetails
                                    join m in _context.Menus on od.MenuId equals m.MenuId
                                    where od.OrderId == OrderId
                                    select new
                                    {
                                        od.DetailsId,
                                        od.OrderId,
                                        od.CategoryId,
                                        Category = (from c in _context.Categories where c.Id == m.CategoryId && c.IsActive select c.Name).FirstOrDefault(),
                                        od.MenuId,
                                        m.Name,
                                        od.Quantity,
                                        od.Size,
                                        od.Price,
                                        od.SubTotal,
                                        od.OrderStatus
                                    }).ToListAsync();
                _response.status = "Success";
                _response.statusCode = 1;
                _response.data = result;
            }
            catch (Exception ex)
            {
                _response.statusCode = 0;
                _response.status = "Failed";
                _logger.LogError(ex, "An error occurred in {ServiceName} at {Time}", nameof(CaptainService), DateTime.Now);
            }
            return Json(_response);
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardData(string type = "")
        {
            APIResponseEntity _response = new APIResponseEntity();
            try
            {
                DashboardViewModel dashboardView = new DashboardViewModel();
                if (string.IsNullOrEmpty(type) || type.ToUpper() == "ALL")
                {
                    dashboardView.totalSale = Convert.ToString(await _context.OrderDetails.SumAsync(od => od.SubTotal)).Replace("-", "");
                    dashboardView.totalSaleDetail = Convert.ToString(await _context.OrderDetails.SumAsync(od => od.SubTotal)).Replace("-", "");
                    dashboardView.totalSalePer = Convert.ToString("100").Replace("-", "");
                    dashboardView.isSaleIncreased = true;

                    dashboardView.totalItemSale = Convert.ToString(await _context.OrderDetails.SumAsync(od => od.Quantity)).Replace("-", "");
                    dashboardView.totalItemSaleDetail = Convert.ToString(await _context.OrderDetails.SumAsync(od => od.Quantity)).Replace("-", "");
                    dashboardView.totalItemSalePer = Convert.ToString("100").Replace("-", "");
                    dashboardView.isItemSaleIncreased = true;

                    dashboardView.totalNetProfit = Convert.ToString(await _context.OrderDetails.SumAsync(od => od.SubTotal)).Replace("-", "");
                    dashboardView.totalNetDetail = Convert.ToString(await _context.OrderDetails.SumAsync(od => od.SubTotal)).Replace("-", "");
                    dashboardView.totalNetPer = Convert.ToString("100").Replace("-", "");
                    dashboardView.isNetProfitIncreased = true;

                    dashboardView.totalCustomer = Convert.ToString(await _context.OrderMasters.CountAsync()).Replace("-", "");
                    dashboardView.totalCustomerDetail = Convert.ToString(await _context.OrderMasters.CountAsync()).Replace("-", "");
                    dashboardView.totalCustomerPer = Convert.ToString("100").Replace("-", "");
                    dashboardView.isCustomerIncreased = true;
                }
                else if (type.ToUpper() == "MONTHLY")
                {
                    DateTime endDate = DateTime.Now;
                    DateTime startDateCurrentMonth = new DateTime(endDate.Year, endDate.Month, 1);
                    DateTime startDatePreviousMonth = startDateCurrentMonth.AddMonths(-1);
                    DateTime endDatePreviousMonth = startDateCurrentMonth.AddDays(-1);

                    var salesCurrentMonth = await _context.OrderDetails
                        .Where(od => od.DataEnteredOn >= startDateCurrentMonth && od.DataEnteredOn <= endDate)
                        .SumAsync(od => od.SubTotal);
                    var salesPreviousMonth = await _context.OrderDetails
                        .Where(od => od.DataEnteredOn >= startDatePreviousMonth && od.DataEnteredOn <= endDatePreviousMonth)
                        .SumAsync(od => od.SubTotal);

                    var totalSales = await _context.OrderDetails.SumAsync(od => od.SubTotal);

                    decimal salesChangePercentage = 100;
                    if (salesPreviousMonth > 0)
                    {
                        salesChangePercentage = ((salesCurrentMonth - salesPreviousMonth) / salesPreviousMonth) * 100;
                    }

                    dashboardView.totalSale = Convert.ToString(salesCurrentMonth).Replace("-", "");
                    dashboardView.totalSaleDetail = Convert.ToString(salesCurrentMonth - salesPreviousMonth).Replace("-", "");
                    dashboardView.totalSalePer = Convert.ToString(salesChangePercentage).Replace("-", "");
                    dashboardView.isSaleIncreased = salesChangePercentage > 0;
                    dashboardView.totalNetProfit = Convert.ToString(salesCurrentMonth).Replace("-", "");
                    dashboardView.totalNetDetail = Convert.ToString(salesCurrentMonth - salesPreviousMonth).Replace("-", "");
                    dashboardView.totalNetPer = Convert.ToString(salesChangePercentage).Replace("-", "");
                    dashboardView.isNetProfitIncreased = salesChangePercentage > 0;

                    var itemSalesCurrentMonth = await _context.OrderDetails
                        .Where(od => od.DataEnteredOn >= startDateCurrentMonth && od.DataEnteredOn <= endDate)
                        .SumAsync(od => od.Quantity);
                    var itemSalesPreviousMonth = await _context.OrderDetails
                        .Where(od => od.DataEnteredOn >= startDatePreviousMonth && od.DataEnteredOn <= endDatePreviousMonth)
                        .SumAsync(od => od.Quantity);

                    var totalItemSales = await _context.OrderDetails.SumAsync(od => od.Quantity);

                    decimal itemSalesChangePercentage = 100;
                    if (itemSalesPreviousMonth > 0)
                    {
                        itemSalesChangePercentage = ((itemSalesCurrentMonth - itemSalesPreviousMonth) / itemSalesPreviousMonth) * 100;
                    }

                    dashboardView.totalItemSale = Convert.ToString(itemSalesCurrentMonth).Replace("-", "");
                    dashboardView.totalItemSaleDetail = Convert.ToString(itemSalesCurrentMonth - itemSalesPreviousMonth).Replace("-", "");
                    dashboardView.totalItemSalePer = Convert.ToString(itemSalesChangePercentage).Replace("-", "");
                    dashboardView.isItemSaleIncreased = itemSalesChangePercentage > 0;

                    var totalCustomersCurrentMonth = await _context.OrderMasters
                        .Where(om => om.OrderDateTime >= startDateCurrentMonth && om.OrderDateTime <= endDate)
                        .CountAsync();
                    var totalCustomersPreviousMonth = await _context.OrderMasters
                        .Where(om => om.OrderDateTime >= startDatePreviousMonth && om.OrderDateTime <= endDatePreviousMonth)
                        .CountAsync();
                    decimal customersChangePercentage = 100;
                    if (totalCustomersPreviousMonth > 0)
                    {
                        customersChangePercentage = ((totalCustomersCurrentMonth - totalCustomersPreviousMonth) / totalCustomersPreviousMonth) * 100;
                    }
                    dashboardView.totalCustomer = Convert.ToString(totalCustomersCurrentMonth).Replace("-", "");
                    dashboardView.totalCustomerDetail = Convert.ToString(totalCustomersCurrentMonth - totalCustomersPreviousMonth).Replace("-", "");
                    dashboardView.totalCustomerPer = Convert.ToString(customersChangePercentage).Replace("-", "");
                    dashboardView.isCustomerIncreased = customersChangePercentage > 0;
                }
                else if (type.ToUpper() == "WEEKLY")
                {
                    // Get the current date
                    DateTime endDate = DateTime.Now;

                    // Get the start of the current week (using Sunday as the first day of the week)
                    DateTime startDateCurrentWeek = endDate.AddDays(-(int)endDate.DayOfWeek); // Sunday of the current week
                    DateTime startDatePreviousWeek = startDateCurrentWeek.AddDays(-7); // Sunday of the previous week
                    DateTime endDatePreviousWeek = startDateCurrentWeek.AddDays(-1); // Saturday of the previous week

                    // Calculate sales for the current week
                    var salesCurrentWeek = await _context.OrderDetails
                        .Where(od => od.DataEnteredOn >= startDateCurrentWeek && od.DataEnteredOn <= endDate)
                        .SumAsync(od => od.SubTotal);

                    // Calculate sales for the previous week
                    var salesPreviousWeek = await _context.OrderDetails
                        .Where(od => od.DataEnteredOn >= startDatePreviousWeek && od.DataEnteredOn <= endDatePreviousWeek)
                        .SumAsync(od => od.SubTotal);

                    // Calculate total sales for comparison (optional, could be based on different date range)
                    var totalSales = await _context.OrderDetails.SumAsync(od => od.SubTotal);

                    // Calculate percentage change between the current and previous week
                    decimal salesChangePercentage = 100;
                    if (salesPreviousWeek > 0)
                    {
                        salesChangePercentage = ((salesCurrentWeek - salesPreviousWeek) / salesPreviousWeek) * 100;
                    }

                    // Set values in the dashboard view for total sales
                    dashboardView.totalSale = Convert.ToString(salesCurrentWeek).Replace("-", "");
                    dashboardView.totalSaleDetail = Convert.ToString(salesCurrentWeek - salesPreviousWeek).Replace("-", "");
                    dashboardView.totalSalePer = Convert.ToString(salesChangePercentage.ToString("F2")).Replace("-", "");
                    dashboardView.isSaleIncreased = salesChangePercentage > 0;
                    dashboardView.totalNetProfit = Convert.ToString(salesCurrentWeek).Replace("-", "");
                    dashboardView.totalNetDetail = Convert.ToString(salesCurrentWeek - salesPreviousWeek).Replace("-", "");
                    dashboardView.totalNetPer = Convert.ToString(salesChangePercentage.ToString("F2")).Replace("-", "");
                    dashboardView.isNetProfitIncreased = salesChangePercentage > 0;

                    // Calculate item sales for the current and previous week
                    var itemSalesCurrentWeek = await _context.OrderDetails
                        .Where(od => od.DataEnteredOn >= startDateCurrentWeek && od.DataEnteredOn <= endDate)
                        .SumAsync(od => od.Quantity);

                    var itemSalesPreviousWeek = await _context.OrderDetails
                        .Where(od => od.DataEnteredOn >= startDatePreviousWeek && od.DataEnteredOn <= endDatePreviousWeek)
                        .SumAsync(od => od.Quantity);

                    decimal itemSalesChangePercentage = 100;
                    if (itemSalesPreviousWeek > 0 && itemSalesCurrentWeek > itemSalesPreviousWeek)
                    {
                        itemSalesChangePercentage = ((itemSalesCurrentWeek - itemSalesPreviousWeek) / itemSalesPreviousWeek) * 100;
                    }
                    else
                    {
                        itemSalesChangePercentage = ((itemSalesPreviousWeek - itemSalesCurrentWeek) / itemSalesCurrentWeek) * 100;
                    }

                    dashboardView.totalItemSale = Convert.ToString(itemSalesCurrentWeek).Replace("-", "");
                    dashboardView.totalItemSaleDetail = Convert.ToString(itemSalesCurrentWeek - itemSalesPreviousWeek).Replace("-", "");
                    dashboardView.totalItemSalePer = Convert.ToString(itemSalesChangePercentage.ToString("F2")).Replace("-", "");
                    dashboardView.isItemSaleIncreased = itemSalesCurrentWeek > itemSalesPreviousWeek;

                    // Calculate total customers for the current and previous week
                    var totalCustomersCurrentWeek = await _context.OrderMasters
                        .Where(om => om.OrderDateTime >= startDateCurrentWeek && om.OrderDateTime <= endDate)
                        .CountAsync();

                    var totalCustomersPreviousWeek = await _context.OrderMasters
                        .Where(om => om.OrderDateTime >= startDatePreviousWeek && om.OrderDateTime <= endDatePreviousWeek)
                        .CountAsync();

                    decimal customersChangePercentage = 100;
                    if (totalCustomersPreviousWeek > 0 && totalCustomersCurrentWeek > totalCustomersPreviousWeek)
                    {
                        customersChangePercentage = ((totalCustomersCurrentWeek - totalCustomersPreviousWeek) / totalCustomersPreviousWeek) * 100;
                    }
                    else
                    {
                        customersChangePercentage = ((totalCustomersPreviousWeek - totalCustomersCurrentWeek) / totalCustomersPreviousWeek) * 100;
                    }

                    dashboardView.totalCustomer = Convert.ToString(totalCustomersCurrentWeek).Replace("-", "");
                    dashboardView.totalCustomerDetail = Convert.ToString(totalCustomersCurrentWeek - totalCustomersPreviousWeek).Replace("-", "");
                    dashboardView.totalCustomerPer = Convert.ToString(customersChangePercentage.ToString("F2")).Replace("-", "");
                    dashboardView.isCustomerIncreased = totalCustomersCurrentWeek > totalCustomersPreviousWeek;

                }
                else
                {
                    // Get the current date (today)
                    DateTime endDate = DateTime.Now;

                    // Get the start of today and yesterday
                    DateTime startDateToday = new DateTime(endDate.Year, endDate.Month, endDate.Day); // Today at 00:00:00
                    DateTime startDateYesterday = startDateToday.AddDays(-1); // Yesterday at 00:00:00
                    DateTime endDateYesterday = startDateToday.AddDays(-1); // Yesterday at 23:59:59

                    // Calculate sales for today
                    var salesToday = await _context.OrderDetails
                        .Where(od => od.DataEnteredOn >= startDateToday && od.DataEnteredOn <= endDate)
                        .SumAsync(od => od.SubTotal);

                    // Calculate sales for yesterday
                    var salesYesterday = await _context.OrderDetails
                        .Where(od => od.DataEnteredOn >= startDateYesterday && od.DataEnteredOn <= endDateYesterday)
                        .SumAsync(od => od.SubTotal);

                    // Calculate total sales for comparison (optional, could be based on different date range)
                    var totalSales = await _context.OrderDetails.SumAsync(od => od.SubTotal);

                    // Calculate percentage change between today and yesterday
                    decimal salesChangePercentage = 100;
                    if (salesYesterday > 0)
                    {
                        salesChangePercentage = ((salesToday - salesYesterday) / salesYesterday) * 100;
                    }

                    // Set values in the dashboard view for total sales
                    dashboardView.totalSale = Convert.ToString(salesToday).Replace("-", "");
                    dashboardView.totalSaleDetail = Convert.ToString(salesToday - salesYesterday).Replace("-", "");
                    dashboardView.totalSalePer = Convert.ToString(salesChangePercentage.ToString("F2")).Replace("-", "");
                    dashboardView.isSaleIncreased = salesChangePercentage > 0;
                    dashboardView.totalNetProfit = Convert.ToString(salesToday).Replace("-", "");
                    dashboardView.totalNetDetail = Convert.ToString(salesToday - salesYesterday).Replace("-", "");
                    dashboardView.totalNetPer = Convert.ToString(salesChangePercentage.ToString("F2")).Replace("-", "");
                    dashboardView.isNetProfitIncreased = salesChangePercentage > 0;

                    // Calculate item sales for today and yesterday
                    var itemSalesToday = await _context.OrderDetails
                        .Where(od => od.DataEnteredOn >= startDateToday && od.DataEnteredOn <= endDate)
                        .SumAsync(od => od.Quantity);

                    var itemSalesYesterday = await _context.OrderDetails
                        .Where(od => od.DataEnteredOn >= startDateYesterday && od.DataEnteredOn <= endDateYesterday)
                        .SumAsync(od => od.Quantity);

                    decimal itemSalesChangePercentage = 100;
                    if (itemSalesYesterday > 0)
                    {
                        itemSalesChangePercentage = ((itemSalesToday - itemSalesYesterday) / itemSalesYesterday) * 100;
                    }


                    dashboardView.totalItemSale = Convert.ToString(itemSalesToday).Replace("-", "");
                    dashboardView.totalItemSaleDetail = Convert.ToString(itemSalesToday - itemSalesYesterday).Replace("-", "");
                    dashboardView.totalItemSalePer = Convert.ToString(itemSalesChangePercentage.ToString("F2")).Replace("-", "");
                    dashboardView.isItemSaleIncreased = itemSalesToday > itemSalesYesterday;

                    // Calculate total customers for today and yesterday
                    var totalCustomersToday = await _context.OrderMasters
                        .Where(om => om.OrderDateTime >= startDateToday && om.OrderDateTime <= endDate)
                        .CountAsync();

                    var totalCustomersYesterday = await _context.OrderMasters
                        .Where(om => om.OrderDateTime >= startDateYesterday && om.OrderDateTime <= endDateYesterday)
                        .CountAsync();

                    decimal customersChangePercentage = 100;
                    if (totalCustomersYesterday > 0)
                    {
                        customersChangePercentage = ((totalCustomersToday - totalCustomersYesterday) / totalCustomersYesterday) * 100;
                    }
                    dashboardView.totalCustomer = Convert.ToString(totalCustomersToday).Replace("-", "");
                    dashboardView.totalCustomerDetail = Convert.ToString(totalCustomersToday - totalCustomersYesterday).Replace("-", "");
                    dashboardView.totalCustomerPer = Convert.ToString(customersChangePercentage.ToString("F2")).Replace("-", "");
                    dashboardView.isCustomerIncreased = totalCustomersToday > totalCustomersYesterday;

                }

                _response.statusCode = 1;
                _response.status = "Success";
                _response.data = dashboardView;
            }
            catch (Exception ex)
            {
                _response.statusCode = 0;
                _response.status = "Failed";
                _logger.LogError(ex, "An error occurred in {ServiceName} at {Time}", nameof(CaptainService), DateTime.Now);
            }
            return Json(_response);
        }
        public async Task<IActionResult> GetChartData(string type)
        {
            APIResponseEntity _response = new APIResponseEntity();
            try
            {
                List<string> months = new List<string>();
                List<decimal> sales = new List<decimal>();

                if (string.IsNullOrEmpty(type))
                {
                    // Get the current year
                    int currentYear = DateTime.Now.Year;

                    // Get sales data grouped by month for the current year
                    var monthlySalesData = await _context.OrderDetails
                        .Where(od => od.DataEnteredOn.Year == currentYear)
                        .GroupBy(od => od.DataEnteredOn.Month)  // Group by month
                        .Select(g => new
                        {
                            Month = g.Key,  // Month number (1-12)
                            TotalSales = g.Sum(od => od.SubTotal)  // Sum of sales for that month
                        })
                        .OrderBy(g => g.Month)  // Optional: Sort by month
                        .ToListAsync();

                    foreach (var data in monthlySalesData)
                    {
                        // Add the month name to the list (e.g., "January", "February", etc.)
                        months.Add(new DateTime(currentYear, data.Month, 1).ToString("MMMM"));

                        // Add the total sales for the month to the list
                        sales.Add(data.TotalSales);
                    }
                }
                else if (type == "Week")
                {
                    // Get the current year
                    int currentYear = DateTime.Now.Year;

                    // First, fetch the data from the database and filter by the current year
                    var orderDetails = await _context.OrderDetails
                        .Where(od => od.DataEnteredOn.Year == currentYear)
                        .ToListAsync();  // Load data into memory

                    // Now, group by week and calculate the total sales in memory
                    var weeklySalesData = orderDetails
                        .GroupBy(od => new
                        {
                            Week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(od.DataEnteredOn, CalendarWeekRule.FirstDay, DayOfWeek.Sunday),
                            Year = od.DataEnteredOn.Year
                        })
                        .Select(g => new
                        {
                            Week = g.Key.Week,
                            Year = g.Key.Year,
                            TotalSales = g.Sum(od => od.SubTotal)
                        })
                        .OrderBy(g => g.Week)  // Optional: Sort by week number
                        .ToList();  // Now the data is grouped and summed in memory


                    foreach (var data in weeklySalesData)
                    {
                        // Format the week range (e.g., "Week 1 (Jan 01 - Jan 07)")
                        months.Add($"Week {data.Week} ({new DateTime(data.Year, 1, 1).AddDays((data.Week - 1) * 7).ToString("MMM dd")} - {new DateTime(data.Year, 1, 1).AddDays(data.Week * 7 - 1).ToString("MMM dd")})");

                        // Add the total sales for the week
                        sales.Add(data.TotalSales);
                    }

                }
                else
                {

                    DateTime currentDate = DateTime.Now.Date;
                    DateTime startDate = currentDate.AddDays(-7);
                    var dayWiseSalesData = await _context.OrderDetails
                        .Where(od => od.DataEnteredOn >= startDate && od.DataEnteredOn <= currentDate.AddDays(1))
                        .GroupBy(od => od.DataEnteredOn.Date)
                        .Select(g => new
                        {
                            Date = g.Key,
                            TotalSales = g.Sum(od => od.SubTotal)
                        })
                        .OrderBy(g => g.Date)
                        .ToListAsync();

                    for (int i = 6; i >= 0; i--)
                    {
                        months.Add(currentDate.AddDays(-i).ToString("ddd"));

                        var salesData = dayWiseSalesData.Where(x => x.Date == currentDate.AddDays(-i)).Select(x => x.TotalSales).FirstOrDefault();
                        sales.Add(salesData);
                    }
                }
                _response.statusCode = 1;
                _response.status = "Success";
                _response.data = new { months, sales };
            }
            catch (Exception ex)
            {
                _response.statusCode = 0;
                _response.status = "Failed";
                _logger.LogError(ex, "An error occurred in {ServiceName} at {Time}", nameof(CaptainService), DateTime.Now);
            }
            return Json(_response);
        }
    }
}

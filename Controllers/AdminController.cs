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
        public async Task<IActionResult> PaymentConfirmed(long OrderId,string paymentType)
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
                                        CaptainName = _context.Users.Where(u => u.Id == om.CaptainId).Select(u => u.FirstName).FirstOrDefault()+" "+_context.Users.Where(u=>u.Id==om.CaptainId).Select(u=>u.LastName).FirstOrDefault(),
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
    }
}

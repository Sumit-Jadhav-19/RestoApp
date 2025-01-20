using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestoApp.Data;
using RestoApp.Models.Entities;
using RestoApp.Repository;
using RestoApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//add Identity with role
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
//add connectionString
builder.Services.AddDbContext<ApplicationDbContext>(option =>
option.UseSqlServer(builder.Configuration.GetConnectionString("DefualtConnection")));
//add dependancy Injections
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
//builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register custom authorization services
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
// Register the service for categories
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<MenuService>();
builder.Services.AddScoped<HallService>();
builder.Services.AddScoped<TableService>();
builder.Services.AddScoped<CaptainService>();
builder.Services.AddScoped<KitchenService>();
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


async Task SeedRolesAndUsers(WebApplication app)
{
    using var scope = app.Services.CreateScope();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles = { "Admin", "User" };

    // Ensure roles are created
    int i = 1;
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var roleResult = await roleManager.CreateAsync(new ApplicationRole {Id=i, Name = role });
            if (!roleResult.Succeeded)
            {
                // Log any errors during role creation
                foreach (var error in roleResult.Errors)
                {
                    Console.WriteLine($"Error creating role {role}: {error.Description}");
                }
            }
            i++;
        }
    }

    // Create the default admin user
    var adminEmail = "admin@example.com";
    var adminPassword = "Admin@123"; // Ensure this meets your password policy

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        var newUser = new ApplicationUser
        {
            Id=1,
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FirstName = adminEmail, LastName = adminEmail,
            ProfilePictureUrl=""
        };

        // Do NOT manually set Id; let ASP.NET Identity handle it
        var userResult = await userManager.CreateAsync(newUser, adminPassword);

        if (userResult.Succeeded)
        {
            var roleResult = await userManager.AddToRoleAsync(newUser, "Admin");

            if (!roleResult.Succeeded)
            {
                // Log any errors when adding the user to the role
                foreach (var error in roleResult.Errors)
                {
                    Console.WriteLine($"Error assigning role 'Admin' to user: {error.Description}");
                }
            }
        }
        else
        {
            // Log the errors from user creation
            foreach (var error in userResult.Errors)
            {
                Console.WriteLine($"Error creating user: {error.Description}");
            }
        }
    }
}


await SeedRolesAndUsers(app);
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<ChatHub>("/chatHub");
app.Run();

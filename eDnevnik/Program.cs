using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using eDnevnik.Data;
using eDnevnik.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. KONFIGURACIJA KONTEKSTA I IDENTITY
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<Korisnik>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// 2. RAZOR PAGES I KONTROLA PRISTUPA
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeAreaFolder("Identity", "/Account");

    // Dozvoli samo Login i Register bez autentifikacije
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Login");
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// 3. SEED ULOGA I ADMIN KORISNIKA
async Task CreateRoles(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<Korisnik>>();

    string[] roleNames = { "Administrator", "Nastavnik", "Ucenik", "Roditelj" };

    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    var adminEmail = "admin@ednevnik.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        var admin = new Korisnik
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            Ime = "Admin",
            Prezime = "Glavni"
        };

        var result = await userManager.CreateAsync(admin, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Administrator");
        }
    }
}

// 4. PIPELINE KONFIGURACIJA
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 5. DEFAULT RUTA PREUSMJERAVA NA LOGIN ILI HOME
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Redirect}/{action=Index}/{id?}");

app.MapRazorPages();

// 6. SEED
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await CreateRoles(services);
}

app.Run();

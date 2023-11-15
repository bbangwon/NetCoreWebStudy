using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using NetCore.Services.Data;
using NetCore.Services.Interfaces;
using NetCore.Services.Svcs;
using NetCore.Utilities.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IUser, UserService>();

//�ſ������� ���α���
builder.Services.AddAuthentication(defaultScheme:CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
         {
             options.AccessDeniedPath = "/Membership/Forbidden";
             options.LoginPath = "/Membership/Login";
        });

builder.Services.AddAuthorization();

Common.SetDataProtection(builder.Services, @"C:\study\NetCore\Keys", "NetCore", Enums.CryptoType.CngCbc);

// DB��������, Migrations ������Ʈ ����
builder.Services.AddDbContext<CodeFirstDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        mig => mig.MigrationsAssembly("NetCore.Migrations")
        );
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//�ſ������� (�̵���� ���)
app.UseAuthentication();

app.Run();

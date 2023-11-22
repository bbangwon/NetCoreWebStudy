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
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<DbFirstDbInitializer>();

//신원보증과 승인권한
builder.Services.AddAuthentication(defaultScheme:CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
         {
             options.AccessDeniedPath = "/Membership/Forbidden";
             options.LoginPath = "/Membership/Login";
        });

builder.Services.AddAuthorization();

Common.SetDataProtection(builder.Services, @"C:\study\NetCore\Keys", "NetCore", Enums.CryptoType.CngCbc);

// DB접속정보, Migrations 프로젝트 지정
builder.Services.AddDbContext<CodeFirstDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        mig => mig.MigrationsAssembly("NetCore.Migrations")
        );
});

builder.Logging.AddFile(options =>
{
    options.LogDirectory = "Logs";  //로그저장폴더
    options.FileName = "log-";      //로그파일접두어 log-20231121.txt
    options.FileSizeLimit = null;   //로그파일최대크기 (기본 10MB)
    options.RetainedFileCountLimit = null; //로그파일보유갯수 (기본 2개)
});

//로그파일 작성
builder.Services.AddLogging(logging => {
    logging.AddConfiguration(builder.Configuration.GetSection(key: "Logging"));
    logging.AddConsole();
    logging.AddDebug();
});

builder.Services.AddSession(options =>
{
    options.Cookie.Name = "NetCore.Session";
    //세션 제한시간(기본값 20분)
    options.IdleTimeout = TimeSpan.FromMinutes(30);
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

//신원보증만 (미들웨어 등록)
app.UseAuthentication();

//세션 지정
app.UseSession();

//초기데이터 등록
using var scope = app.Services.CreateScope();
var dbInitializer = scope.ServiceProvider
    .GetRequiredService<DbFirstDbInitializer>();

int rowAffected = dbInitializer.PlantSeedData();


app.Run();

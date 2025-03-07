
using HelloChat.Hubs;
using Microsoft.EntityFrameworkCore;
using HelloChat.Data;
using Microsoft.AspNetCore.Identity;
using HelloChat.Services.IServices;
using HelloChat.Services;
using HelloChat.Repositories.IRepositories;
using HelloChat.Repositories;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnectionString");

builder.Services.AddDbContext<HelloChatDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<HelloChatDbContext>();

// Add services to the container.
//builder.Services.AddDbContext<HelloChatDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString")));
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IAppRepository, AppRepository>();
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 5;
    options.Password.RequireDigit = false;
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
app.MapRazorPages();
app.MapHub<ChatHub>("/chatHub");
app.Run();

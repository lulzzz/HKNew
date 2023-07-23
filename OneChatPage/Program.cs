using HKDB.Data;
using Microsoft.EntityFrameworkCore;
using OneChatPage.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
var HKDB = builder.Configuration.GetConnectionString("HKDB");

builder.Services.AddDbContext<HKContext>(options =>
                options.UseSqlServer(HKDB)
);

builder.Services.AddScoped<GptService>();
builder.Services.AddScoped<TranslateService>();
builder.Services.AddScoped<SimService>();


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

app.Run();

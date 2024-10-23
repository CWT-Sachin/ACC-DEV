using ACC_DEV.Data;
using ACC_DEV.DataOperation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Added by Nish
builder.Services.AddDbContext<FtlcolomboAccountsContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("constring"));
});

// Added by Nish
builder.Services.AddDbContext<FtlcolombOperationContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("operationConstring"));
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

app.Run();

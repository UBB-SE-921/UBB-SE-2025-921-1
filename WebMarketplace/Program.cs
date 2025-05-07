using Microsoft.EntityFrameworkCore;
using Server.DBConnection;
using Server.Repository;
using SharedClassLibrary.Helper;
using SharedClassLibrary.IRepository;
using SharedClassLibrary.ProxyRepository;
using SharedClassLibrary.Service;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
var connectionString = AppConfig.GetConnectionString("MyLocalDb");
builder.Services.AddDbContext<MarketPlaceDbContext>(options =>
    options.UseSqlServer(connectionString)
        .EnableSensitiveDataLogging()
);

// Register ShoppingCart services
builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();
builder.Services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();

// Register Contract Renewal related services
// Register repositories
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IContractRenewalRepository, ContractRenewalRepository>();
builder.Services.AddScoped<IPDFRepository, PDFRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// Register services
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IContractRenewalService, ContractRenewalService>();
builder.Services.AddScoped<IPDFService, PDFService>();
builder.Services.AddScoped<INotificationContentService, NotificationContentService>();



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

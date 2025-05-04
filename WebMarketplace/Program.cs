using Microsoft.EntityFrameworkCore;
using Server.DBConnection;
using SharedClassLibrary.Helper;
using SharedClassLibrary.Service;
using SharedClassLibrary.Service.Web;
using WebMarketplace.Services;
using SharedIProductService = SharedClassLibrary.Service.IProductService;
using WebIProductService = SharedClassLibrary.Service.Web.IWebProductService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register the custom services
builder.Services.AddScoped<IOrderHistoryService, OrderHistoryService>();
builder.Services.AddScoped<IOrderSummaryService, OrderSummaryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<SharedIProductService, ProductService>();
builder.Services.AddScoped<IDummyWalletService, DummyWalletService>();

// Register web services
builder.Services.AddScoped<WebIProductService, WebProductService>();
builder.Services.AddScoped<SharedClassLibrary.Service.IWaitlistService, SharedClassLibrary.Service.WaitlistService>();
builder.Services.AddScoped<SharedClassLibrary.Service.INotificationService, SharedClassLibrary.Service.NotificationService>();

// Ensure singleton registration of notification service for consistent state
builder.Services.Remove(builder.Services.FirstOrDefault(
    d => d.ServiceType == typeof(SharedClassLibrary.Service.INotificationService)));
builder.Services.AddSingleton<SharedClassLibrary.Service.INotificationService, SharedClassLibrary.Service.NotificationService>();

// Register backward compatibility adapters
builder.Services.AddScoped<WebMarketplace.Services.IProductService>(sp =>
    new ProductServiceAdapter(sp.GetRequiredService<WebIProductService>()));
builder.Services.AddScoped<WebMarketplace.Services.IWaitlistService>(sp =>
    new WaitlistServiceAdapter(sp.GetRequiredService<SharedClassLibrary.Service.IWaitlistService>()));
builder.Services.AddScoped<WebMarketplace.Services.INotificationService>(sp =>
    new NotificationServiceAdapter(sp.GetRequiredService<SharedClassLibrary.Service.INotificationService>()));

var connectionString = AppConfig.GetConnectionString("MyLocalDb");
builder.Services.AddDbContext<MarketPlaceDbContext>(options =>
    options.UseSqlServer(connectionString)
        .EnableSensitiveDataLogging()
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage(); // Add this for detailed error pages in development
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add session middleware
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

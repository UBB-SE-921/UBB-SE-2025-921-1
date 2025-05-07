using Microsoft.EntityFrameworkCore;
using Server.DBConnection;
using SharedClassLibrary.Helper;
using SharedClassLibrary.Service;
using SharedClassLibrary.IRepository; // Add this if needed for repository implementations
using SharedClassLibrary.ProxyRepository; // Add this if needed for proxy repository implementations

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
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IDummyWalletService, DummyWalletService>();



// Register user and buyer services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBuyerService, BuyerService>();

// Register repositories if needed
builder.Services.AddScoped<IUserRepository, UserProxyRepository>(sp => 
    new UserProxyRepository(AppConfig.GetBaseApiUrl()));
builder.Services.AddScoped<IBuyerRepository, BuyerProxyRepository>(sp => 
    new BuyerProxyRepository(AppConfig.GetBaseApiUrl()));

// Register remaining services
builder.Services.AddScoped<IWaitlistService, WaitlistService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Ensure singleton registration of notification service for consistent state
builder.Services.Remove(builder.Services.FirstOrDefault(
    d => d.ServiceType == typeof(INotificationService)));
builder.Services.AddSingleton<INotificationService, NotificationService>();

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

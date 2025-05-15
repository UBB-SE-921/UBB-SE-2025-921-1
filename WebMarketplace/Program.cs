using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedClassLibrary.Helper;
using SharedClassLibrary.IRepository;
using SharedClassLibrary.ProxyRepository;
using SharedClassLibrary.Service;
using System;
using Microsoft.EntityFrameworkCore;
using Server.DBConnection;
using Server.Repository;
using SharedClassLibrary.Helper;
using SharedClassLibrary.IRepository;
using SharedClassLibrary.Service;


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

// Get the base API URL for proxy repositories
string baseApiUrl = AppConfig.GetBaseApiUrl();

// First, register services
builder.Services.AddScoped<IOrderHistoryService, OrderHistoryService>();
builder.Services.AddScoped<IOrderSummaryService, OrderSummaryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IDummyWalletService, DummyWalletService>();
builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();
builder.Services.AddScoped<ISellerService, SellerService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IContractRenewalService, ContractRenewalService>();
builder.Services.AddScoped<IPDFService, PDFService>();
builder.Services.AddScoped<INotificationContentService, NotificationContentService>();
builder.Services.AddScoped<IBuyerAddressService, BuyerAddressService>();

// Register user and buyer services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBuyerService, BuyerService>();

// Register repositories if needed
builder.Services.AddScoped<IUserRepository, UserProxyRepository>(sp => 
    new UserProxyRepository(AppConfig.GetBaseApiUrl()));
builder.Services.AddScoped<IBuyerRepository, BuyerProxyRepository>(sp => 
    new BuyerProxyRepository(AppConfig.GetBaseApiUrl()));



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

// Register seller services
builder.Services.AddScoped<ISellerService, SellerService>();
builder.Services.AddSingleton<ISellerRepository>(provider => new SellerProxyRepository(baseApiUrl));

// Ensure singleton registration of notification service for consistent state
builder.Services.Remove(builder.Services.FirstOrDefault(
    d => d.ServiceType == typeof(INotificationService)));
builder.Services.AddSingleton<INotificationService, NotificationService>();

// Then, register repositories
builder.Services.AddSingleton<IUserRepository>(provider => new UserProxyRepository(baseApiUrl));
builder.Services.AddSingleton<ISellerRepository>(provider => new SellerProxyRepository(baseApiUrl));
builder.Services.AddSingleton<IShoppingCartRepository>(provider => new ShoppingCartProxyRepository(baseApiUrl));
builder.Services.AddSingleton<IOrderRepository>(provider => new OrderProxyRepository(baseApiUrl));
builder.Services.AddSingleton<IOrderHistoryRepository>(provider => new OrderHistoryProxyRepository(baseApiUrl));
builder.Services.AddSingleton<IOrderSummaryRepository>(provider => new OrderSummaryProxyRepository(baseApiUrl));
builder.Services.AddSingleton<IProductRepository>(provider => new ProductProxyRepository(baseApiUrl));
builder.Services.AddSingleton<IContractRepository>(provider => new ContractProxyRepository(baseApiUrl));
builder.Services.AddSingleton<IContractRenewalRepository>(provider => new ContractRenewalProxyRepository(baseApiUrl));
builder.Services.AddSingleton<IPDFRepository>(provider => new PDFProxyRepository(baseApiUrl));
builder.Services.AddSingleton<INotificationRepository>(provider => new NotificationProxyRepository(baseApiUrl));

// IMPORTANT: Remove database context - this should not be used with proxy repositories
// REMOVE THESE LINES:
// var connectionString = AppConfig.GetConnectionString("MyLocalDb");
// builder.Services.AddDbContext<MarketPlaceDbContext>(options =>
//     options.UseSqlServer(connectionString)
//         .EnableSensitiveDataLogging()
// );

// Register Order services
builder.Services.AddScoped<ITrackedOrderService, TrackedOrderService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Register ShoppingCart services
builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();

//Notification Services
builder.Services.AddScoped<INotificationContentService, NotificationContentService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add session middleware
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedClassLibrary.Helper;
using SharedClassLibrary.IRepository;
using SharedClassLibrary.ProxyRepository;
using SharedClassLibrary.Service;
using System;

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
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISellerService, SellerService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IContractRenewalService, ContractRenewalService>();
builder.Services.AddScoped<IPDFService, PDFService>();
builder.Services.AddScoped<INotificationContentService, NotificationContentService>();
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
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

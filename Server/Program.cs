using Server.DBConnection;
using Microsoft.EntityFrameworkCore;
using SharedClassLibrary.Helper;
using Server.Repository;
using SharedClassLibrary.IRepository;

var builder = WebApplication.CreateBuilder(args);
var connectionString = AppConfig.GetConnectionString("MyLocalDb");

// Add services to the container.
builder.Services.AddDbContext<MarketPlaceDbContext>(options =>
    options.UseSqlServer(connectionString)
        .EnableSensitiveDataLogging()
);

// Register all repositories
builder.Services.AddScoped<DatabaseConnection>();
builder.Services.AddScoped<IBuyerRepository, BuyerRepository>();
builder.Services.AddScoped<IContractRenewalRepository, ContractRenewalRepository>();
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
/// builder.Services.AddScoped<IOrderHistoryRepository, OrderHistoryRepository>(); // NOT IMPLEMENTED YET, will not implement because Product needs refactoring (not my job), request my help after this was done please -Alex
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderSummaryRepository, OrderSummaryRepository>();
builder.Services.AddScoped<IPDFRepository, PDFRepository>();
builder.Services.AddScoped<ISellerRepository, SellerRepository>();
builder.Services.AddScoped<ITrackedOrderRepository, TrackedOrderRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IWaitListRepository, WaitListRepository>();
builder.Services.AddScoped<IDummyCardRepository, DummyCardRepository>();
builder.Services.AddScoped<IDummyWalletRepository, DummyWalletRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

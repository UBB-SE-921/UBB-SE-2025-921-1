using Server.DBConnection;
using Microsoft.EntityFrameworkCore;
using Server.Helper;
using Server.Repository;
using SharedClassLibrary.IRepository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = AppConfig.GetConnectionString("MyLocalDb");
builder.Services.AddDbContext<MarketPlaceDbContext>(options => options.UseSqlServer(connectionString));

// Register all repositories
builder.Services.AddScoped<IBuyerRepository, BuyerRepository>();
builder.Services.AddScoped<IContractRenewalRepository, ContractRenewalRepository>();
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
/// builder.Services.AddScoped<IOrderHistoryRepository, OrderHistoryRepository>(); // NOT IMPLEMENTED YET, will not implement because DummyProduct needs refactoring (not my job), request my help after this was done please -Alex
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderSummaryRepository, OrderSummaryRepository>();
builder.Services.AddScoped<IPDFRepository, PDFRepository>();
builder.Services.AddScoped<ISellerRepository, SellerRepository>();
/// builder.Services.AddScoped<ITrackedOrderRepository, TrackedOrderRepository>(); // NOT IMPLEMENTED YET, will implement -Alex
builder.Services.AddScoped<IUserRepository, UserRepository>();
/// builder.Services.AddScoped<IWaitListRepository, WaitListRepository>(); // NOT IMPLEMENTED YET, will implement -Alex
/// builder.Services.AddScoped<IDummyCardRepository, DummyCardRepository>(); // NOT IMPLEMENTED YET, will implement -Alex
/// builder.Services.AddScoped<IDummyWalletRepository, DummyWalletRepository>(); // NOT IMPLEMENTED YET, will implement -Alex
/// builder.Services.AddScoped<IDummyProductRepository, DummyProductRepository>(); // NOT IMPLEMENTED YET, still has DummyProduct but I think it is manageable, only minor modifications need to be made -Alex

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

using MarketPlace924.Repository;
using Server.DBConnection;
using Server.Helper;
using SharedClassLibrary.IRepository;

var builder = WebApplication.CreateBuilder(args);
var connectionString = AppConfig.GetConnectionString("MyLocalDb");

// Add services to the container.
builder.Services.AddScoped<DatabaseConnection>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBuyerRepository, BuyerRepository>();
builder.Services.AddScoped<ISellerRepository, SellerRepository>();
builder.Services.AddScoped<IContractRenewalRepository>(provider =>
{
    return new ContractRenewalRepository(connectionString!);
});
builder.Services.AddScoped<IContractRepository>(provider =>
{
    return new ContractRepository(connectionString!);
});
builder.Services.AddScoped<IPDFRepository>(provider =>
{
    return new PDFRepository(connectionString!);
});
builder.Services.AddScoped<INotificationRepository>(provider =>
{
    return new NotificationRepository(connectionString!);
});
builder.Services.AddScoped<IWaitListRepository>(provider =>
{
    return new WaitListRepository(connectionString!);
});
builder.Services.AddScoped<IOrderHistoryRepository>(provider =>
{
    return new OrderHistoryRepository(connectionString!);
});

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

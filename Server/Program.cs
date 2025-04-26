using Server.DBConnection;
using SharedClassLibrary.IRepository;
using Microsoft.EntityFrameworkCore;
using Server.Helper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = AppConfig.GetConnectionString("MyLocalDb");
builder.Services.AddDbContext<MarketPlaceDbContext>(options => options.UseSqlServer(connectionString));

// Register your IUserRepository implementation
// Assuming your implementation class is named UserRepository
builder.Services.AddScoped<DatabaseConnection>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

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

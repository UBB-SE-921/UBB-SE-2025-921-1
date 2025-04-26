using Server.DBConnection;
using SharedClassLibrary.IRepository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register your IUserRepository implementation
// Assuming your implementation class is named UserRepository
builder.Services.AddScoped<DatabaseConnection>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBuyerRepository, BuyerRepository>();

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

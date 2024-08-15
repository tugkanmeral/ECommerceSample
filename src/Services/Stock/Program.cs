using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var postgreConnectionStr = builder.Configuration.GetConnectionString("PostgreSQLConnection");
ArgumentNullException.ThrowIfNull(postgreConnectionStr, nameof(postgreConnectionStr));

builder.Services.AddDbContext<StockDbContext>(options =>
    options.UseNpgsql(postgreConnectionStr));

builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IMessageProducer, RabbitMQProducer>();
builder.Services.AddHostedService<RabbitMQConsumer>();

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

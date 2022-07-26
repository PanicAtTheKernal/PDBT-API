using Microsoft.EntityFrameworkCore;
using PDBT.Data;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
MariaDbServerVersion serverVersion = new MariaDbServerVersion(new Version(10,5));

builder.Services.AddControllers();
builder.Services.AddDbContext<PdbtContext>(opt => opt
    .UseMySql(builder.Configuration.GetConnectionString("PDBT"), serverVersion)
    .UseValidationCheckConstraints()
    .UseEnumCheckConstraints());
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
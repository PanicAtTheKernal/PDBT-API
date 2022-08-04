using Microsoft.EntityFrameworkCore;
using PDBT.Data;
using PDBT.Repository;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
MariaDbServerVersion serverVersion = new MariaDbServerVersion(new Version(10,5));

#region Repositories
builder.Services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddTransient<IIssueRepository, IssueRepository>();
builder.Services.AddTransient<ILabelRepository, LabelRepository>();
builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
#endregion


builder.Services.AddControllers();
builder.Services.AddDbContext<PdbtContext>(opt => opt
    .UseMySql(builder.Configuration.GetConnectionString("PDBT"), serverVersion)
    .UseValidationCheckConstraints()
    .UseEnumCheckConstraints()
    .LogTo(Console.WriteLine, LogLevel.Information)
    .EnableSensitiveDataLogging()
    .EnableDetailedErrors());
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
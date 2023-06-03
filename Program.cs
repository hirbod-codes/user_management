using Microsoft.Extensions.Options;
using user_management.Data;
using MongoDB.Driver;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<MongoContext>(builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<IMongoClient>(new MongoClient(builder.Configuration.GetSection("MongoDB").GetValue<string>("ConnectionString")));

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

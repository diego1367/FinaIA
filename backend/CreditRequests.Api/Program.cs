using CreditRequests.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddDbContext<CreditRequestsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CreditRequests")));
builder.Services.AddCors(options => options.AddPolicy("AngularLocal", policy =>
    policy.WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors("AngularLocal");
app.MapControllers();
app.Run();

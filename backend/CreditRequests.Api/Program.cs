using CreditRequests.Application.Interfaces;
using CreditRequests.Application.Services;
using CreditRequests.Infrastructure;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(options => options.SwaggerDoc("v1", new OpenApiInfo
{
    Title = "API de solicitudes de crédito",
    Version = "v1"
}));
builder.Services.AddScoped<ICreditRequestService, CreditRequestService>();
builder.Services.AddCreditRequestsInfrastructure(builder.Configuration.GetConnectionString("CreditRequests"));
builder.Services.AddCors(options => options.AddPolicy("AngularLocal", policy =>
    policy.WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("./v1/swagger.json", "Solicitudes de crédito v1"));
}

app.UseCors("AngularLocal");
app.MapControllers();
app.Run();

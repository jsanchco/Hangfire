using Hangfire;
using HangFire.CommonApi.Extensions;
using HangFire.CommonApi.Helpers.Errors;
using HangFire.Web.HostedServices;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureSerilog();

// Add services to the container.
builder.Services.ConfigureCors();

//Hangfire
var configuration = builder.Configuration;
builder.Services.AddHangfire(x => x.UseSqlServerStorage(configuration.GetConnectionString("HangfireConnection")));
builder.Services.AddHangfireServer();

builder.Services.AddControllers();

builder.Services.AddValidationErrors();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<HandlerManagerServices>();
builder.Services.AddHostedService(
    provider => provider.GetRequiredService<HandlerManagerServices>());

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseHttpsRedirection();

app.UseHangfireDashboard("/dashboard");

app.UseAuthorization();

app.MapControllers();

app.Run();

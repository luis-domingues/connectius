using Connectius.Application;
using Connectius.Infrastructure;
using Connectius.Presentation.Common.Errors;
using Microsoft.AspNetCore.Mvc.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddSingleton<ProblemDetailsFactory, ConnectiusProblemDetailsFactory>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler("/error");

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
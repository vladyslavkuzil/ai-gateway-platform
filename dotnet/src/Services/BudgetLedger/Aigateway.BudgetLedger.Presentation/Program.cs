using Aigateway.BudgetLedger.Application.Repositories;
using Aigateway.BudgetLedger.Application.Services;
using Aigateway.BudgetLedger.Presentation.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddSingleton<IBudgetRepository, InMemoryBudgetRepository>();
builder.Services.AddScoped<IBudgetService, BudgetService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

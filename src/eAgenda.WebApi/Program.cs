using eAgenda.Aplicacao;
using eAgenda.Infra;
using eAgenda.Infra.Compartilhado.Orm;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();
builder.Services.AddInfraRepositories(builder.Configuration, builder.Logging);
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var dbcontext = scope.ServiceProvider.GetRequiredService<EAgendaDbContext>();

    dbcontext.Database.Migrate();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

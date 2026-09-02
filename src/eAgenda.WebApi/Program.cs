using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using eAgenda.Aplicacao;
using eAgenda.Infra;
using eAgenda.Infra.Compartilhado.Orm;
using eAgenda.WebApi.Compartilhado;
using eAgenda.WebApi.Compartilhado.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfraRepositories(builder.Configuration, builder.Logging);
builder.Services.AddApplicationServices();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName)
             .Get<JwtOptions>() ?? new JwtOptions();

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = string.Empty,

            ValidateAudience = true,
            ValidAudience = string.Empty,

            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),

            NameClaimType = ClaimTypes.NameIdentifier,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        string? type = ProblemDetailsTypes.ObterPorStatus(context.ProblemDetails.Status);

        if (type is not null)
            context.ProblemDetails.Type = type;

        // TraceId
        context.ProblemDetails.Extensions["traceId"] =
            Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
    };
});

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var dbContext = scope.ServiceProvider.GetRequiredService<EAgendaDbContext>();

    dbContext.Database.Migrate();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApi();
app.MapControllers();

app.Run();
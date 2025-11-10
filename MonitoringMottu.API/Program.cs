using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MonitoringMottu.API.Extensions;
using MonitoringMottu.Domain.Interfaces;
using MonitoringMottu.Infrastructure.Context;
using MonitoringMottu.Infrastructure.Repositories;

namespace MonitoringMottu.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 🔹 Configurações básicas
            builder.Configuration
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            // 🔹 Controllers + JSON
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });

            // 🔹 Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(x =>
            {
                x.SwaggerDoc("v1", new OpenApiInfo()
                {
                    Title = builder.Configuration["Swagger:Title"] ?? "MonitoringMottu API",
                    Description = "API para o trabalho da disciplina de .NET",
                    Contact = new OpenApiContact
                    {
                        Name = "Equipe MonitoringMottu",
                        Email = "monitoringmottu@fiap.com.br"
                    }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                x.IncludeXmlComments(xmlPath);
            });

            // 🔹 Banco Oracle
            builder.Services.AddDbContext<MonitoringMottuContext>(options =>
                options.UseOracle(builder.Configuration.GetConnectionString("OracleConnection")));

            // 🔹 Repositórios
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddScoped<IMotoRepository, MotoRepository>();
            builder.Services.AddScoped<IGaragemRepository, GaragemRepository>();

            // 🔹 HealthChecks (antes do Build!)
            builder.Services.AddChecks(builder.Configuration);

            // 🔹 Build
            var app = builder.Build();

            // 🔹 Middleware padrão
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            // 🔹 Endpoints
            app.MapControllers();

            app.MapHealthChecks("/health-check", new HealthCheckOptions
            {
                ResponseWriter = HealthCheckExtensions.WriteResponse
            });

            app.Run();
        }
    }
}
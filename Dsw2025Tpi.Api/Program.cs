using Dsw2025Tpi.Api.Middleware;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Application.Mapper;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Data.Repositories;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Enums;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Dsw2025Tpi.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowReactApp", policy =>
            {
                policy.WithOrigins("http://localhost:5173")
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });
        // -------------------------------------------

        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddScoped<IOrderService, OrderService>();
        builder.Services.AddScoped<TokenService>();

        builder.Services.AddScoped<IRepository, EfRepository>();

        builder.Services.AddScoped<IEntityMapper<Product, ProductModel.ProductResponse>, ProductMapper>();
        builder.Services.AddScoped<IEntityMapper<Order, OrderModel.OrderResponse>, OrderMapper>();

        builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 1;
            options.Password.RequiredUniqueChars = 0;
        })
        .AddEntityFrameworkStores<AuthenticateContext>()
        .AddDefaultTokenProviders();

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
            };
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddHealthChecks();

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Dsw2025Tpi API", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,
                    },
                    new List<string>()
                }
            });
        });

        var dbServer = builder.Configuration["ConnectionSettings:Server"];
        var dbName = builder.Configuration["ConnectionSettings:Database"];

        var connectionString = $"Server={dbServer};Database={dbName};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        builder.Services.AddDbContext<Dsw2025TpiContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        builder.Services.AddDbContext<AuthenticateContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });


        var app = builder.Build();

        // --- SEEDING ---
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var roles = Enum.GetNames(typeof(Roles));

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            try
            {
                var context = services.GetRequiredService<Dsw2025TpiContext>();
                context.Seedwork<Customer>("Sources\\customers.json");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error durante el seeding: {ex.Message}");
            }
        }
        // ----------------

        app.UseMiddleware<ExceptionHandler>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseCors("AllowReactApp");
        // ----------------------------------

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthChecks("/healthcheck");

        app.Run();
    }
}
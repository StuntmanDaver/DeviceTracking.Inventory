using DeviceTracking.Inventory.Api.Middleware;
using DeviceTracking.Inventory.Application.Services.Authentication;
using DeviceTracking.Inventory.Application.Mapping;
using DeviceTracking.Inventory.Application.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddFluentValidation(fv =>
    {
        fv.RegisterValidatorsFromAssemblyContaining<CreateInventoryItemDtoValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<CreateLocationDtoValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<CreateReceiptDtoValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<InventoryItemQueryDtoValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<LocationQueryDtoValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<InventoryTransactionQueryDtoValidator>();
        fv.RegisterValidatorsFromAssemblyContaining<SupplierQueryDtoValidator>();
        fv.ImplicitlyValidateChildProperties = true;
        fv.ImplicitlyValidateRootCollectionElements = true;
    });

// Add API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

// Add Swagger/OpenAPI
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Device Tracking Inventory API",
        Version = "v1",
        Description = "Enterprise-grade barcode-based inventory management system API",
        Contact = new OpenApiContact
        {
            Name = "Device Tracking Team",
            Email = "support@devicetracking.com"
        }
    });

    // Include XML documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Add JWT Bearer authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add authentication and authorization
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "DeviceTracking.Inventory",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "DeviceTracking.Inventory.Api",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:Key"] ?? "DefaultSecretKeyForDevelopmentOnly")),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Add authorization with inventory policies
builder.Services.AddInventoryAuthorization();

// Register application services
builder.Services.AddScoped<IAuthenticationService, DeviceTrackingAuthenticationService>();
builder.Services.AddScoped<RoleMappingService>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add logging
builder.Services.AddLogging();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    // Enable Swagger in development
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Device Tracking Inventory API v1");
        c.RoutePrefix = "swagger";
    });
}

// Use CORS
app.UseCors("AllowAll");

// Use request logging
app.UseRequestLogging();

// Use global exception handler
app.UseGlobalExceptionHandler();

// Use authentication middleware
app.UseAuthenticationMiddleware();

// Use authorization
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Add health check endpoint
app.MapGet("/health", () => "Healthy");

// Add ready endpoint
app.MapGet("/ready", () => "Ready");

app.Run();
using DemoFYP.EF;
using DemoFYP.Middlewares;
using Microsoft.EntityFrameworkCore;
using DemoFYP.Extension;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using DemoFYP.Repositories;
using DemoFYP.Repositories.IRepositories;
using System.Text.Json;
using DemoFYP.Authorization;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

// Register MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 33))));
builder.Services.AddDbContextFactory<AppDbContext>(options => options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 33))), ServiceLifetime.Scoped);

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Register IServices
builder.AutoRegisterServices(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<IJwtRepository, JwtRepository>();

// Register Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var jwtRepository = context.HttpContext.RequestServices.GetRequiredService<IJwtRepository>();

            var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var accessTokenFromJwt = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (userId != null)
            {
                var userToken = await jwtRepository.GetUserTokenByUserId(Guid.Parse(userId));

                if (userToken == null) {
                    context.Fail("You haven't login yet");
                    return;
                }

                if (userToken.AccessToken != accessTokenFromJwt)
                {
                    context.Fail("Token mismatch");
                    return;
                }

                if (userToken.IsRevoked)
                {
                    context.Fail("Token is revoked");
                    return;
                }
            }
        },
        OnChallenge = context =>
        {
            if (!context.Response.HasStarted)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                var result = JsonSerializer.Serialize(new
                {
                    code = 401,
                    message = context?.AuthenticateFailure?.Message
                });
                return context.Response.WriteAsync(result);
            }

            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/orderHub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        },
    };
});

builder.Services.AddAuthorization(options =>
{
    var allPermissions = new[] {
        "Create_User", "Update_User", "Delete_User",
        "Create_Product", "Update_Product", "Delete_Product",
        "Reset_Password", "AP_Revoke_User", "AP_Create_Role",
        "Read_Cart", "Update_Cart", "AP_Create_User",
        "AP_Reinstate_user", "AP_Read_Product", "AP_Read_ProductDetail",
        "AP_Update_Product", "AP_Delete_Product", "AP_Reset_Password",
        "AP_Read_Order", "AP_Update_Order", "AP_Read_Payment",
        "AP_Update_Payment", "AP_Cancel_Order", "AP_Cancel_Order_Item",
        "Confirm_Cancel", "Reject_Cancel", "AP_Update_User", "AP_Publish_Product",
        "AP_Read_PaymentDetail"
    };

    foreach (var permission in allPermissions)
    {
        options.AddPolicy(permission, policy =>
            policy.Requirements.Add(new PermissionRequirement(permission)));
    }
});

builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

// Register Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    // Add JWT Bearer Authentication Definition
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter JWT Bearer Token",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
    });

    // Apply security requirement to use Bearer Token
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
            new string[] { }
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Register Exception Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseRouting();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<OrderHub>("/orderHub");
app.UseStaticFiles();

app.Run();

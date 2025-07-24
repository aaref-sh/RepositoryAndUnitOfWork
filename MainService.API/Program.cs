using Helper;
using Helper.Helpers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using MainService.API.Extensions;
using MainService.Application.Extensions;
using MainService.Presistance.Extensions;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

try
{
    ServiceLocator.SetServiceProvider(builder.Services.BuildServiceProvider());

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var jwt = builder.Configuration.GetSection("JWT").Get<JWT>()!;
    builder.Services.AddAuthorization();
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwt.Issuer,
                ValidAudience = jwt.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret))
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {   
                    if(!context.Request.Cookies["bearer_jwt"].IsNullOrEmpty())
                        context.Token ??= context.Request.Cookies["bearer_jwt"];
                    return Task.CompletedTask;
                }
            };
        }).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.LoginPath = "/login";
        });

    builder.Host.UseSerilog((context, configuration) => configuration
        .WriteTo.Console(levelSwitch: new(Serilog.Events.LogEventLevel.Warning))
        .WriteTo.File(
            Path.Combine(Directory.GetCurrentDirectory(), "Logs", "log-.txt"),
            rollingInterval: RollingInterval.Day,
            levelSwitch: new(Serilog.Events.LogEventLevel.Debug)
        ));


    builder.Services.AddHttpClient();

    builder.Services.AddPresistenceExtensions(builder.Configuration);
    builder.Services.AddApplicationExtensions();
    builder.Services.AddAPIExtensions();

    var app = builder.Build();

    app.ApplyMigrations();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.MapStaticAssets();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger(options =>
        {
            options.RouteTemplate = "/openapi/{documentName}.json";
        });
        app.MapScalarApiReference();
   
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();


    app.MapControllers();
    
    app.Run();

}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
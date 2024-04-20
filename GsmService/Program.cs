using System.Reflection;
using System.Text;
using System.Text.Json;
using Docker.DotNet;
using GsmApi;
using GsmApi.Authentication;
using GsmApi.Extensions;
using GsmApi.Hubs;
using GsmApi.Jobs;
using GsmApi.Repositories;
using GsmApi.Services;
using GsmCore.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddWindowsService();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IServerService, ServerService>();
builder.Services.AddScoped<IServerRepository, ServerRepository>();
builder.Services.AddScoped<ISettingRepository, SettingRepository>();
builder.Services.AddSingleton<SteamCmdClient>();
builder.Services.AddSingleton<ResticUtil>();
builder.Services.AddSingleton<RconClient>();
builder.Services.AddTransient<CronJob>();
builder.Services.AddTransient<TaskJob>();

var dockerUri = OperatingSystem.IsLinux()
    ? new Uri("unix:///var/run/docker.sock")
    : new Uri("npipe://./pipe/docker_engine");

var dockerClient = new DockerClientConfiguration(dockerUri).CreateClient();
builder.Services.AddSingleton<IDockerClient>(dockerClient);

builder.Services.AddQuartz(q =>
{
    q.UseSimpleTypeLoader();
    q.UseInMemoryStore();
    q.UseDefaultThreadPool(tp => { tp.MaxConcurrency = 10; });
});


builder.Services.AddDbContext<GsmDbContext>();
builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<GsmDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddCronJobScheduling();
builder.Services.AddCronJobs(Assembly.GetExecutingAssembly());

// Adding Authentication
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })

    // Adding Jwt Bearer
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JWT:ValidAudience"],
            ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
        };
    });

//set json serializer defaults
JsonSerializerOptions options = new()
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true
};

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();

app.MapHub<GsmHub>("/hub");

// Run db migrations
using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<GsmDbContext>();
await dbContext.Database.MigrateAsync();

app.RunCronJobScheduling();
app.Run();
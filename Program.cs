using System.Reflection;
using oed_authz.Interfaces;
using oed_authz.Services;
using oed_authz.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<ConnectionStrings>(builder.Configuration.GetSection("ConnectionStrings"));

builder.Services.AddSingleton<IAltinnEventHandlerService, AltinnEventHandlerService>();
builder.Services.AddSingleton<IOedRoleRepositoryService, OedRoleRepositoryService>();
builder.Services.AddSingleton<IPolicyInformationPointService, PolicyInformationPointService>();

builder.Services.AddControllers();

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly());
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseDeveloperExceptionPage();
//app.UseAuthentication();
app.MapControllers();

app.Run();
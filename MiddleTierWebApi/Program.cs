using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

ConfigurationSection AzureAd = (ConfigurationSection)builder.Configuration.GetSection("AzureAd");
ConfigurationSection DownstreamWebApi = (ConfigurationSection)builder.Configuration.GetSection("DownstreamWebApi");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(AzureAd)
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddDownstreamApi("DownstreamWebApi", DownstreamWebApi)
    .AddInMemoryTokenCaches();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

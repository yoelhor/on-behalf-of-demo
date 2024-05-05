using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// This is required to be instantiated before the OpenIdConnectOptions starts getting configured.
// By default, the claims mapping will map claim names in the old format to accommodate older SAML applications.
// For instance, 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role' instead of 'roles' claim.
// This flag ensures that the ClaimsIdentity claims collection will be built from the claims in the token
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

// Sign-in users with the Microsoft identity platform

ConfigurationSection AzureAd = (ConfigurationSection)builder.Configuration.GetSection("AzureAd");
ConfigurationSection MiddleTierWebApi = (ConfigurationSection)builder.Configuration.GetSection("MiddleTierWebApi");

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(AzureAd, OpenIdConnectDefaults.AuthenticationScheme)
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddDownstreamApi("MiddleTierWebApi", MiddleTierWebApi)
    .AddInMemoryTokenCaches();

builder.Services.AddRazorPages().AddMvcOptions(options =>
  {
//    var policy = new AuthorizationPolicyBuilder()
//                  //.RequireAuthenticatedUser()
//                  .Build();
//    options.Filters.Add(new AuthorizeFilter(policy));
  }).AddMicrosoftIdentityUI();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

 app.MapRazorPages();
 app.MapControllers();

app.Run();

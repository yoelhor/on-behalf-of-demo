using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;


namespace MiddleTierWebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MiddleTierController : ControllerBase
{
    private readonly IConfiguration _configuration;
    readonly IAuthorizationHeaderProvider _authorizationHeaderProvider;

    public MiddleTierController(IConfiguration configuration, IAuthorizationHeaderProvider authorizationHeaderProvider)
    {
        _configuration = configuration;
        _authorizationHeaderProvider = authorizationHeaderProvider;
    }

    [HttpGet()]
    [Authorize]
    [RequiredScopeOrAppPermission(
           AcceptedScope = new[] { "Account.Read" })]
    public async Task<string> GetAsync()
    {

        string MiddleTierApiAccessToken = string.Empty;

        // Read app settings
        string baseUrl = _configuration.GetSection("DownstreamWebApi:BaseUrl").Value!;
        string[] scopes = _configuration.GetSection("DownstreamWebApi:Scopes").Get<string[]>();
        string endpoint = _configuration.GetSection("DownstreamWebApi:Endpoint").Value!;

        // Set the scope full URI
        for (int i = 0; i < scopes.Length; i++)
        {
            scopes[i] = $"{baseUrl}/{scopes[i]}";
        }

        try
        {
            // Get an access token to call the MiddleTier Web API (the first API in line)
            MiddleTierApiAccessToken = await _authorizationHeaderProvider.CreateAuthorizationHeaderForUserAsync(scopes);
        }
        catch (MicrosoftIdentityWebChallengeUserException ex)
        {
            if (ex.MsalUiRequiredException.ErrorCode == "user_null")
            {
                return "Error (MID_1): The token cache does not contain the token to access the web APIs. To get the access token, sign-out and sign-in again.";
            }
            else
            {
                return "Error (MID_2): " + ex.MsalUiRequiredException.Message;
            }
        }
        catch (System.Exception ex)
        {
            return "Error (MID_3): " + ex.Message;
        }

        try
        {
            // Use the access token to call the Account API.
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", MiddleTierApiAccessToken);
            return "Result from middle-tier API : calling downstream API... " + await client.GetStringAsync(endpoint);

        }
        catch (System.Exception ex)
        {
            return "Error (MID_4):"  + ex.Message;
        }
    }
}
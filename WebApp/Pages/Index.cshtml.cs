using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;

namespace WebApp.Pages;

//[Authorize]
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IConfiguration _configuration;
    readonly IAuthorizationHeaderProvider _authorizationHeaderProvider;

    public string MiddleTierApiMessage {get; set;}
    public string MiddleTierApiAccessToken {get; set;}

    public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration, IAuthorizationHeaderProvider authorizationHeaderProvider)
    {
        _logger = logger;
        _configuration = configuration;
        _authorizationHeaderProvider = authorizationHeaderProvider;
    }

    public async Task<IActionResult> OnGetAsync()
    {

        // Read app settings
        string baseUrl = _configuration.GetSection("MiddleTierWebApi:BaseUrl").Value!;
        string[] scopes = _configuration.GetSection("MiddleTierWebApi:Scopes").Get<string[]>();
        string endpoint = _configuration.GetSection("MiddleTierWebApi:Endpoint").Value!;


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
                MiddleTierApiMessage = "Error (APP_1): The token cache does not contain the token to access the web APIs. To get the access token, sign-out and sign-in again.";
            }
            else
            {
                MiddleTierApiMessage = "Error (APP_2): " + ex.MsalUiRequiredException.Message;
                return Page();
            }
        }
        catch (System.Exception ex)
        {
            MiddleTierApiMessage = "Error (APP_3): " + ex.Message;
            return Page();
        }


        try
        {
            // Use the access token to call the Account API.
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", MiddleTierApiAccessToken);
            MiddleTierApiMessage = "Web app " +  await client.GetStringAsync(endpoint);

        }
        catch (System.Exception ex)
        {
            MiddleTierApiMessage = "Error (APP_4): " + ex.Message;
        }

        return Page();
    }
}

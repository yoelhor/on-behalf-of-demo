using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;


namespace MiddleTierWebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DownstreamController : ControllerBase
{
    public DownstreamController()
    {

    }

    [HttpGet()]
    [Authorize]
    [RequiredScopeOrAppPermission(
           AcceptedScope = new[] { "Account.Payment" })]
    public async Task<string> GetAsync()
    {
        return "Result from Downstream Web API " + DateTime.Now.ToString();
    }
}
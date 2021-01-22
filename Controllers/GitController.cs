using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AzureDevopsAngularOauth.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GitController : ControllerBase
    {
        
        private readonly ILogger<GitController> _logger;

        public GitController(ILogger<GitController> logger)
        {
            _logger = logger;
        }


        [HttpGet,Authorize]
        public IActionResult Get() 
        {
            var claims = User.Claims;
            StringBuilder claimsBuilder = new StringBuilder();
            foreach (var claim in claims) claimsBuilder.AppendLine($"Type:{claim.Type},Value:{claim.Value}");
            return Ok(new { @message = $"Successfully hit with claims: {claimsBuilder}" });
        }

        [HttpPost("pull")]
        public IActionResult PullRequest([FromBody]JsonElement content)
        {
            Debug.WriteLine($"Pull request -> Azure Dev Ops messsage:{content.ToString()}");
            return Ok(new { @pullRequest=content});
        }

        [HttpPost("commit")]
        public IActionResult CommitAction([FromBody]JsonElement content)
        {
            Debug.WriteLine($"Commit -> Azure Dev Ops messsage:{content.ToString()}");
            return Ok(new { @commitMessage = content });
        }

    }
}

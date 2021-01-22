using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AzureDevopsAngularOauth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OauthController : ControllerBase
    {
        private readonly OauthOptions _oauthOptions;
        private readonly JwtOptions _jwtOptions;

        public OauthController(IOptionsSnapshot<OauthOptions> oauthOptions, IOptions<JwtOptions> jwtOptions) 
        {
            _oauthOptions = oauthOptions.Value;
            _jwtOptions = jwtOptions.Value;
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback(string code, string state) 
        {
            Debug.Write($"Oauth callback code:{code}. State:{state}");
            if (string.IsNullOrEmpty(code)) 
            {
                return BadRequest(new { @Error = "No code, brah" });
            }
          
            var token = await ExchangeForAzureToken(code);
            var amrockJwt = GenerateAmrockJwt(token);
            return Ok(amrockJwt);
        }

        [HttpGet("login")]
        public IActionResult Login() 
        {
            string state = "none";
            string url = $"https://app.vssps.visualstudio.com/oauth2/authorize?client_id={_oauthOptions.AppId}&response_type=Assertion&state={state}&scope={_oauthOptions.Scope}&redirect_uri={_oauthOptions.CallbackUrl}";
            return Ok(url);
        }

        [HttpGet("loginredirect")]
        public IActionResult LoginRedirect()
        {
            string state = "none";
            string url = $"https://app.vssps.visualstudio.com/oauth2/authorize?client_id={_oauthOptions.AppId}&response_type=Assertion&state={state}&scope={_oauthOptions.Scope}&redirect_uri={_oauthOptions.CallbackUrl}";
            return Redirect(url);
        }

        [HttpGet("refresh"), Authorize]
        public async Task<IActionResult> RefreshAzureToken() 
        {
            //get previous token from jwt
            var refreshTokenClaim = User.Claims.FirstOrDefault(x => x.Type == OauthOptions.RefreshTokenClaimType);
            if (refreshTokenClaim == null) 
            {
                return BadRequest(new { @Error = "Missing refresh token." });
            }
            string refreshToken = refreshTokenClaim.Value;
            string newToken = await RefreshAzureToken(refreshToken);
            var amrockJwt = GenerateAmrockJwt(newToken);
            return Ok(amrockJwt);

        }

        private async Task<string> RefreshAzureToken(string refreshToken) 
        {
            var body = $"client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer&client_assertion={HttpUtility.UrlEncode(_oauthOptions.ClientSecret)}&grant_type=refresh_token&assertion={HttpUtility.UrlEncode(refreshToken)}&redirect_uri={_oauthOptions.CallbackUrl}";
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://app.vssps.visualstudio.com");
            var request = new HttpRequestMessage(HttpMethod.Post, "oauth2/token");
            request.Content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return string.Empty;


        }

        private async Task<string> ExchangeForAzureToken(string token) 
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://app.vssps.visualstudio.com");
            var request = new HttpRequestMessage(HttpMethod.Post, "oauth2/token");
            var body = $"client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer&client_assertion={HttpUtility.UrlEncode(_oauthOptions.ClientSecret)}&grant_type=urn:ietf:params:oauth:grant-type:jwt-bearer&assertion={HttpUtility.UrlEncode(token)}&redirect_uri={_oauthOptions.CallbackUrl}";
            request.Content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode) 
            {
                return await response.Content.ReadAsStringAsync();
            }
            return string.Empty;
        }


        private string GenerateAmrockJwt(string azureToken, List<Claim> claims = null ) 
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            var tokenHandler = new JwtSecurityTokenHandler();
            //parse asure token json 
            AzureJwt azureJwt = JsonSerializer.Deserialize<AzureJwt>(azureToken);

            //create list of claims, with the azure refresh token included
            claims = claims ?? new List<Claim>();
            claims.AddRange(new List<Claim>() { new Claim("AzureRefresh", azureJwt.refresh_token), new Claim("AzureToken", azureJwt.access_token) });

            var tokeOptions = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.Now.AddSeconds(int.Parse(azureJwt.expires_in) - 60),  //set expiration to expire before the azure token would
                signingCredentials: signinCredentials
            );
            var tokenString = tokenHandler.WriteToken(tokeOptions);
            return tokenString;
        }

    }
}
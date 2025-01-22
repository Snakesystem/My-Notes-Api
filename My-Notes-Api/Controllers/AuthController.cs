using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using My_Notes_Api.Services;
using System.Net;
using System.Text.RegularExpressions;

namespace My_Notes_Api.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;
        private readonly IConfiguration _configuration;
        public AuthController(IAuthService serive, IConfiguration configuration)
        {
            _service = serive;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            string remote_addr = HttpContextServerVariableExtensions.GetServerVariable(this.HttpContext, "REMOTE_ADDR");
            string remote_host = "";
            try
            {
                remote_host = Dns.GetHostEntry(HttpContextServerVariableExtensions.GetServerVariable(this.HttpContext, "REMOTE_HOST")).HostName;
            }
            catch (Exception)
            {

            }

            request.IPAddress = remote_addr;
            request.CompName = remote_host;

            try
            {
                ActionResult result = new ActionResult();

                if (string.IsNullOrEmpty(request.UserName))
                {
                    result.result = false;
                    result.message = "Username Empty";
                    return Ok(result);
                }
                if (string.IsNullOrEmpty(request.Password))
                {
                    result.result = false;
                    result.message = "Password Empty";
                    return Ok(result);
                }
                if (string.IsNullOrEmpty(request.Email))
                {
                    result.result = false;
                    result.message = "Email Empty";
                    return Ok(result);
                }
                if (!string.IsNullOrEmpty(request.Email))
                {
                    Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                    Match match = regex.Match(request.Email);
                    if (!match.Success)
                    {
                        result.result = false;
                        result.message = "Format Email Not Correct";
                        return Ok(result);

                    }
                }

                result = await _service.Register(request);

                if(result.result)
                {
                    return Ok(result);
                } else
                {
                    return BadRequest(result);
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}

using FirebaseAuth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FirebaseAuth.Controllers;
[ApiController]
[Route("api/[controller]")]
public class ValuesController : ControllerBase
{
    public ValuesController()
    {

    }
    
    [HttpGet]
    [Route("/getData")]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> UserData()
    {
        return Ok("authnticated");
    }
}

using Firebase.Auth;
using FirebaseAuth.Models;
using FireSharp.Interfaces;
using FireSharp.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FirebaseAuth.Controllers;
[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IConfiguration configuration;
    FirebaseAuthProvider auth;
    IFirebaseConfig config = new FireSharp.Config.FirebaseConfig
    {
        AuthSecret = "6zl3AoGAWe6zwXg2GMFzFSHud9skiPXkVOrwAHG3",
        BasePath = "https://fir-auth-df02c-default-rtdb.firebaseio.com"
    };
    IFirebaseClient client;
    public AccountController(IConfiguration _configuration)
    {
        configuration = _configuration;
        auth = new FirebaseAuthProvider(new FirebaseConfig(_configuration.GetValue<string>("Firebase:apikey")));
        client = new FireSharp.FirebaseClient(config);
        

    }
    [HttpGet]
    [Route("/SignUp")]
    public async Task<IActionResult> SignUp()
    {
        try
        {
            var email = "test@gmail.com";
            var password = "Test@123";
            //create the user
            await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            Users user = new Users
            {
                Email = email,
                Password = password,
                RoleId =  Convert.ToInt32(RoleType.Admin)
            };
            AddUser(user);
            return Ok();
        }
        catch (FirebaseAuthException ex)
        {
            return Ok();
        }

    }
    private string BuildToken(string key, string issuer, string audience, string Email)
    {
        var byteKey = Encoding.ASCII.GetBytes(key);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("Id", Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, Email),
                new Claim(JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "Admin"),
             }),
            Expires = DateTime.UtcNow.AddMinutes(5),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials
            (new SymmetricSecurityKey(byteKey),
            SecurityAlgorithms.HmacSha512Signature)
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var stringToken = tokenHandler.WriteToken(token);
        return stringToken;
    }
    [HttpGet]
    [Route("/SignIn")]
    public async Task<IActionResult> SignIn()
    {
        try
        {
            var email = "test@gmail.com";
            var password = "Test@123";
            var token   = BuildToken(configuration["Jwt:Key"], configuration["Jwt:Issuer"], configuration["Jwt:Audience"], email);
            //create the user

            return Ok(token);
        }
        catch (FirebaseAuthException ex)
        {
            return Ok();
        }

    }

    private SetResponse AddUser(Users user)
    {
        var data = user;
        if (user.RoleId == 0)
        {
            data.RoleId = Convert.ToInt32(RoleType.User);
        }
        var response = client.Push("User/", data);
        data.Id = response.Result.name;
        var setResponse = client.Set("User/" + data.Id, data);
        return setResponse;
    }





    [HttpPost]
    [Authorize(Roles = nameof(RoleType.Admin))]
    public async Task<IActionResult> Create(Users user)
    {
        try
        {
            var data = user;
            var setResponse = AddUser(data);

            if (setResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok("Added Succesfully");
            }
            else
            {
                return Ok( "Something went wrong!!");
            }
        }
        catch (Exception ex)
        {
            return Ok(ex.Message);
        }
    }

}

using Firebase.Auth;
using FirebaseAuth.Models;
using FireSharp.Interfaces;
using FireSharp.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FirebaseAuth.Controllers;
[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IConfiguration configuration;
    FirebaseAuthProvider auth;
    IFirebaseConfig config;
    IFirebaseClient client;
    public UserController(IConfiguration _configuration)
    {
        configuration = _configuration;
        auth = new FirebaseAuthProvider(new FirebaseConfig(configuration.GetValue<string>("Firebase:apikey")));
        
        config = new FireSharp.Config.FirebaseConfig
        {
            AuthSecret = configuration.GetValue<string>("Firebase:authSecret"),
            BasePath = configuration.GetValue<string>("Firebase:basePath")
        };
        client = new FireSharp.FirebaseClient(config);

    }

    [HttpGet]
    [Route("/getUserById")]
    [Authorize(Roles = nameof(RoleType.Admin)+","+nameof(RoleType.User))]
    public IActionResult getUserById(string id)
    {
        var response = client.Get("User/" + id);
        var data = JsonConvert.DeserializeObject<Users>(response.Body);
        return Ok(data);
    }

    [HttpGet]
    [Route("/getAllusers")]
    [Authorize(Roles = nameof(RoleType.User))]
    public IActionResult getAllusers()
    {
        var response = client.Get("User");
        var list = new List<Users>();
        var data = JsonConvert.DeserializeObject<dynamic>(response.Body);
        if (response != null)
        {
            foreach (var item in data)
            {
                list.Add(JsonConvert.DeserializeObject<Users>(((JProperty)item).Value.ToString()));
            }
        }
        return Ok(list);
    }
    [HttpPost]
    [Route("/ManageUser")]
    [Authorize(Roles = nameof(RoleType.Admin))]
    public IActionResult ManageUser(Users user)
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
                return Ok("Something went wrong!!");
            }
        }
        catch (Exception ex)
        {
            return Ok(ex.Message);
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
}

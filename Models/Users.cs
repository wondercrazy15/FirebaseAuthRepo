using Newtonsoft.Json.Linq;

namespace FirebaseAuth.Models;

public class Users
{
    public string? Id { get; set; }
    public string? UserName { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public int RoleId { get; set; }
}

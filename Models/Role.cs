namespace FirebaseAuth.Models;

public class Role
{
    public int Id { get; set; }

    public string? Name { get; set; }
}

public enum RoleType
{
    Admin = 1,
    User = 2
}

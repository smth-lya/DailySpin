using DailySpin.ORM;
using System.Diagnostics.CodeAnalysis;

namespace DailySpin.Domain;

[Table("users")]
public sealed class User
{
    public Guid Id { get; set; }

    public string Username { get; set; }
    
    [Column("password_hash")]
    public string PasswordHash { get; set; }

    [Column("first_name")]
    public string? FirstName { get; set; }

    [Column("last_name")]
    public string? LastName { get; set; }

    [Column("created_at")]
    public DateTime CreateAt { get; set; }

    public User() { }

    [SetsRequiredMembers]
    public User(string username, string passwordHash)
    {
        Id = Guid.NewGuid();
        Username = username;
        PasswordHash = passwordHash;
        CreateAt = DateTime.Now;
    }
}

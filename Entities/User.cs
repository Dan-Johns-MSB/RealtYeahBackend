using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RealtYeahBackend.Entities;

public partial class User
{
    public int UserId { get; set; }

    public string Login { get; set; } = null!;

    public byte[] Password { get; set; } = null!;

    public string Role { get; set; } = null!;

    public int EmployeeId { get; set; }

    [JsonIgnore]
    public virtual Employee? Employee { get; set; } = null!;
}

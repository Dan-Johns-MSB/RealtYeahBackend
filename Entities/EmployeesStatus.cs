using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RealtYeahBackend.Entities;

public partial class EmployeesStatus
{
    public string Status { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}

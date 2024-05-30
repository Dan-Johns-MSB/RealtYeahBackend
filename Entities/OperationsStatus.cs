using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RealtYeahBackend.Entities;

public partial class OperationsStatus
{
    public string Status { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Operation> Operations { get; set; } = new List<Operation>();
}

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RealtYeahBackend.Entities;

public partial class ClientsStatus
{
    public string Status { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<ClientsStatusesAssignment> ClientsStatusesAssignments { get; set; } = new List<ClientsStatusesAssignment>();
}

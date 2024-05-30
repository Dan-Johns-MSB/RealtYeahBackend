using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RealtYeahBackend.Entities;

public partial class EstateObject
{
    public int EstateObjectId { get; set; }

    public string Address { get; set; } = null!;

    public string Type { get; set; } = null!;

    public float Area { get; set; }
    public float Price { get; set; }

    public string Photo { get; set; } = null!;

    public string Status { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Operation> Operations { get; set; } = new List<Operation>();

    [JsonIgnore]
    public virtual ObjectsStatus? StatusNavigation { get; set; } = null!;
}

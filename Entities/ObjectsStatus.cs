using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RealtYeahBackend.Entities;

public partial class ObjectsStatus
{
    public string Status { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<EstateObject> EstateObjects { get; set; } = new List<EstateObject>();
}

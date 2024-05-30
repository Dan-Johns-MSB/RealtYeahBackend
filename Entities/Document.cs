using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RealtYeahBackend.Entities;

public partial class Document
{
    public int DocumentId { get; set; }

    public string Name { get; set; } = null!;

    public string File { get; set; } = null!;

    public int OperationId { get; set; }

    [JsonIgnore]
    public virtual Operation Operation { get; set; } = null!;
}

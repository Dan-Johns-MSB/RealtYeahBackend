using System.Text.Json.Serialization;

namespace RealtYeahBackend.Entities;

public partial class ClientsStatusesAssignment
{
    public int ClientId { get; set; }

    public string Status { get; set; } = null!;

    public int OperationId { get; set; }

    public string? Requirements { get; set; }

    [JsonIgnore]
    public virtual Operation? Operation { get; set; } = null!;

    [JsonIgnore]
    public virtual Client? ClientNavigation { get; set; } = null!;

    [JsonIgnore]
    public virtual ClientsStatus? StatusNavigation { get; set; } = null!;
}

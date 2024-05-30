using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RealtYeahBackend.Entities;

public partial class Operation
{
    public int OperationId { get; set; }

    public string Name { get; set; } = null!;

    [JsonConverter(typeof(CustomDateTimeConverter))]
    public DateTime Date { get; set; }

    public int CounteragentLead { get; set; }

    public int? CounteragentSecondary { get; set; }

    public int? EstateObjectId { get; set; }

    public int HostId { get; set; }

    public string Status { get; set; } = null!;

    public string? ActType { get; set; }

    public int? FkOperationLead { get; set; }

    public int? FkOperationSecondary { get; set; }

    [JsonIgnore]
    public virtual Operation? FkOperationLeadNavigation { get; set; }

    [JsonIgnore]
    public virtual Operation? FkOperationSecondaryNavigation { get; set; }

    [JsonIgnore]
    public virtual ICollection<Operation> InverseFkOperationLeadNavigation { get; set; } = new List<Operation>();

    [JsonIgnore]
    public virtual ICollection<Operation> InverseFkOperationSecondaryNavigation { get; set; } = new List<Operation>();

    [JsonIgnore]
    public virtual ICollection<ClientsStatusesAssignment> ClientsStatusesAssignments { get; set; } = new List<ClientsStatusesAssignment>();

    [JsonIgnore]
    public virtual Client? CounteragentLeadNavigation { get; set; } = null!;

    [JsonIgnore]
    public virtual Client? CounteragentSecondaryNavigation { get; set; }

    [JsonIgnore]
    public virtual Employee? Host { get; set; } = null!;

    [JsonIgnore]
    public virtual EstateObject? EstateObjectNavigation { get; set; } = null!;

    [JsonIgnore]
    public virtual OperationsStatus? StatusNavigation { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public class CustomDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }
}

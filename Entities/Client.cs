using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RealtYeahBackend.Entities;

public partial class Client
{
    public int ClientId { get; set; }

    public string PassportNumber { get; set; } = null!;

    public string TaxpayerNumber { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string Email { get; set; } = null!;

    [JsonConverter(typeof(CustomDateTimeConverter))]
    public DateTime Birthdate { get; set; }

    public string Birthplace { get; set; } = null!;

    public string Gender { get; set; } = null!;

    public string Photo { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<ClientsStatusesAssignment> ClientsStatusesAssignments { get; set; } = new List<ClientsStatusesAssignment>();

    [JsonIgnore]
    public virtual ICollection<Operation> OperationCounteragentLeadNavigations { get; set; } = new List<Operation>();

    [JsonIgnore]
    public virtual ICollection<Operation> OperationCounteragentSecondaryNavigations { get; set; } = new List<Operation>();

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

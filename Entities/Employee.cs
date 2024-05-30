using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RealtYeahBackend.Entities;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string Name { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string Job { get; set; } = null!;

    [JsonConverter(typeof(CustomDateTimeConverter))]
    public DateTime Birthdate { get; set; }

    public string Address { get; set; } = null!;

    [JsonConverter(typeof(CustomDateTimeConverter))]
    public DateTime Hiredate { get; set; }

    [JsonConverter(typeof(CustomDateTimeConverter))]
    public DateTime Promotedate { get; set; }

    [JsonConverter(typeof(CustomDateTimeConverter))]
    public DateTime? Firedate { get; set; }

    public string Status { get; set; } = null!;

    public string Photo { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Operation> Operations { get; set; } = new List<Operation>();
    [JsonIgnore]
    public virtual EmployeesStatus? StatusNavigation { get; set; } = null!;
    [JsonIgnore]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}

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
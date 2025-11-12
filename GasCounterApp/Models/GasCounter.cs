using SQLite;

namespace GasCounterApp.Models;

[Table("gas_counters")]
public class GasCounter
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public string? CounterId { get; set; }

    public string? CustomerName { get; set; }

    public string? StreetName { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public double? GpsAccuracy { get; set; }

    public string? State { get; set; }

    public string? Notes { get; set; }

    public bool IsChecked { get; set; }

    public DateTime? LastCheckedDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public GasCounter()
    {
        CreatedDate = DateTime.Now;
        ModifiedDate = DateTime.Now;
        IsChecked = false;
    }
}

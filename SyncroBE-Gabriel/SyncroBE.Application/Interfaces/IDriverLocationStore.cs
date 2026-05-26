namespace SyncroBE.Application.Interfaces;

public record DriverLocationEntry(
    int DriverId,
    string DriverName,
    double Latitude,
    double Longitude,
    DateTime UpdatedAt
);

public interface IDriverLocationStore
{
    void Set(int driverId, string driverName, double latitude, double longitude);
    IReadOnlyList<DriverLocationEntry> GetActive(TimeSpan maxAge);
    void Remove(int driverId);
}

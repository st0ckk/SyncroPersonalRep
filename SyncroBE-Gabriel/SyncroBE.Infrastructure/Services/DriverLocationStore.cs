using SyncroBE.Application.Interfaces;
using System.Collections.Concurrent;

namespace SyncroBE.Infrastructure.Services;

/// <summary>
/// Almacén en memoria (sin persistencia en DB) de la última ubicación
/// conocida de cada chofer activo. Se limpia automáticamente al reiniciar.
/// </summary>
public class DriverLocationStore : IDriverLocationStore
{
    private readonly ConcurrentDictionary<int, DriverLocationEntry> _entries = new();

    public void Set(int driverId, string driverName, double latitude, double longitude)
    {
        _entries[driverId] = new DriverLocationEntry(
            driverId,
            driverName,
            latitude,
            longitude,
            DateTime.UtcNow
        );
    }

    public IReadOnlyList<DriverLocationEntry> GetActive(TimeSpan maxAge)
    {
        var cutoff = DateTime.UtcNow - maxAge;

        return _entries.Values
            .Where(e => e.UpdatedAt >= cutoff)
            .OrderBy(e => e.DriverName)
            .ToList();
    }

    public void Remove(int driverId) => _entries.TryRemove(driverId, out _);
}

using MottoSprint.Models;

namespace MottoSprint.Services;

public interface IParkingService
{
    Task<IEnumerable<ParkingSpot>> GetAllSpotsAsync();
    Task<ParkingSpot?> GetSpotByIdAsync(int id);
    Task<ParkingSpot?> GetSpotByNumberAsync(string spotNumber);
    Task<ParkingSpot> CreateSpotAsync(ParkingSpot spot);
    Task<bool> OccupySpotAsync(int id, string vehiclePlate);
    Task<bool> FreeSpotAsync(int id);
    Task<IEnumerable<ParkingSpot>> GetAvailableSpotsAsync();
    Task<IEnumerable<ParkingSpot>> GetOccupiedSpotsAsync();
}
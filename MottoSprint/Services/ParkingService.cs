using Microsoft.EntityFrameworkCore;
using MottoSprint.Data;
using MottoSprint.Models;

namespace MottoSprint.Services;

public class ParkingService : IParkingService
{
    private readonly MottoSprintDbContext _context;

    public ParkingService(MottoSprintDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ParkingSpot>> GetAllSpotsAsync()
    {
        return await _context.ParkingSpots
            .OrderBy(s => s.SpotNumber)
            .ToListAsync();
    }

    public async Task<ParkingSpot?> GetSpotByIdAsync(int id)
    {
        return await _context.ParkingSpots.FindAsync(id);
    }

    public async Task<ParkingSpot?> GetSpotByNumberAsync(string spotNumber)
    {
        return await _context.ParkingSpots
            .FirstOrDefaultAsync(s => s.SpotNumber == spotNumber);
    }

    public async Task<ParkingSpot> CreateSpotAsync(ParkingSpot spot)
    {
        _context.ParkingSpots.Add(spot);
        await _context.SaveChangesAsync();
        return spot;
    }

    public async Task<bool> OccupySpotAsync(int id, string vehiclePlate)
    {
        var spot = await _context.ParkingSpots.FindAsync(id);
        if (spot == null || spot.IsOccupied) return false;

        spot.IsOccupied = true;
        spot.VehiclePlate = vehiclePlate;
        spot.OccupiedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> FreeSpotAsync(int id)
    {
        var spot = await _context.ParkingSpots.FindAsync(id);
        if (spot == null || !spot.IsOccupied) return false;

        spot.IsOccupied = false;
        spot.VehiclePlate = null;
        spot.OccupiedAt = null;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ParkingSpot>> GetAvailableSpotsAsync()
    {
        return await _context.ParkingSpots
            .Where(s => !s.IsOccupied)
            .OrderBy(s => s.SpotNumber)
            .ToListAsync();
    }

    public async Task<IEnumerable<ParkingSpot>> GetOccupiedSpotsAsync()
    {
        return await _context.ParkingSpots
            .Where(s => s.IsOccupied)
            .OrderBy(s => s.SpotNumber)
            .ToListAsync();
    }
}
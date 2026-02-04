namespace MedVehicle.MongoDB;

using MedVehicle.Models;
using MedVehicle.Infrastructure; 
using global::MongoDB.Driver;

public class CarService
{
    // Reference to the Cars collection in MongoDB
    private readonly IMongoCollection<Car> _carsCollection;
    private readonly IWebHostEnvironment _environment;

    // Constructor that accepts the MongoDB context
    public CarService(NamedCollection dbContext, IWebHostEnvironment environment)
    {
        _carsCollection = dbContext.GetCollection<Car>("Cars");
        _environment = environment;
    }

    //  CRUD Operations
    public async Task<List<Car>> GetCarsAsync() => 
        await _carsCollection.Find(_ => true).ToListAsync();

    //  Create a new car
    public async Task CreateAsync(Car newCar) => 
        await _carsCollection.InsertOneAsync(newCar);
        
    // READ: Get by PublicId
    public async Task<Car?> GetByPublicIdAsync(string publicId) =>
        await _carsCollection.Find(x => x.PublicId == publicId).FirstOrDefaultAsync();

    // UPDATE: Find by PublicId, then replace
    public async Task UpdateByPublicIdAsync(string publicId, Car updatedCar) =>
        await _carsCollection.ReplaceOneAsync(x => x.PublicId == publicId, updatedCar);

    // DELETE: Find by PublicId
    public async Task RemoveByPublicIdAsync(string publicId)
    {
        // Find the car first (so we can get the filename)
        var car = await _carsCollection.Find(x => x.PublicId == publicId).FirstOrDefaultAsync();

        if (car is null) return; // If it doesn't exist, we're done

        // Delete the physical image file if it exists
        if (!string.IsNullOrEmpty(car.ImageFileName))
        {
            var filePath = Path.Combine(_environment.WebRootPath, "uploads", car.ImageFileName);
            
            try 
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                // Log error, but don't stop the DB deletion just because a file is locked/missing
                Console.WriteLine($"Could not delete file {car.ImageFileName}: {ex.Message}");
            }
        }

        // Delete the record from MongoDB
        await _carsCollection.DeleteOneAsync(x => x.PublicId == publicId);
    }

}
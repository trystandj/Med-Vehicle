namespace Med_Vehicle.Infrastructure;

using global::MongoDB.Driver;

public interface NamedCollection
{
    //get a collection for any class (Car, User, etc.) by name. This is used by services to get their collections. that way there
    // is only one place where the database connection is handled.
    IMongoCollection<Collection> GetCollection<Collection>(string name);
}

// Implementation of the MongoDB context
public class MongoDbContext : NamedCollection
{
    private readonly IMongoDatabase _database;

    public MongoDbContext()
    {
        // Load connection settings from environment variables
        var connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
        var databaseName = Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME");

        if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(databaseName))
        {
            throw new Exception("MongoDB environment variables are not set!");
        }

        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    //Provide collecttion name when requested. For example: carservice calls GetCollection<Car>("Cars")
    public IMongoCollection<Collection> GetCollection<Collection>(string name) 
        => _database.GetCollection<Collection>(name);
}
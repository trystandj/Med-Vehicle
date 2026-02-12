namespace Med_Vehicle.Models;

using System.ComponentModel.DataAnnotations;
using global::MongoDB.Bson;
using global::MongoDB.Bson.Serialization.Attributes;

public class VehicleModifications
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string CarId { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    public string PublicId { get; set; } = Guid.NewGuid().ToString("n");

    [Required(ErrorMessage = "Modification name is required")]
    [MaxLength(100, ErrorMessage = "Modification name cannot exceed 100 characters.")]
    public string Modification { get; set; } = default!;

    [Required(ErrorMessage = "Date installed is required")]
    public DateTime DateInstalled { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? Notes { get; set; }
}
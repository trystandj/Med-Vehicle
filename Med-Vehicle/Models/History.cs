namespace Med_Vehicle.Models;

using System.ComponentModel.DataAnnotations;
using global::MongoDB.Bson;
using global::MongoDB.Bson.Serialization.Attributes;

public class History
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string CarId { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    public string PublicId { get; set; } = Guid.NewGuid().ToString("n");

    [Required(ErrorMessage = "Event description is required")]
    [MaxLength(100, ErrorMessage = "Event description cannot exceed 100 characters.")]
    public string Event { get; set; } = default!;

    [Required(ErrorMessage = "Date is required")]
    public DateTime DateOfEvent { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? Notes { get; set; }
    
}
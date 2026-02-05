namespace Med_Vehicle.Models;
using System.ComponentModel.DataAnnotations;

using global:: MongoDB.Bson;
using global:: MongoDB.Bson.Serialization.Attributes;

public class Car
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string PublicId { get; set; } = Guid.NewGuid().ToString("n");
   
    [Required(ErrorMessage = "Make is required"), MaxLength(50)]
    public string Make { get; set; } = default!;

    [Required(ErrorMessage = "Model is required"), MaxLength(50)]
    public string Model { get; set; } = default!;

    [Required(ErrorMessage = "Year is required"), Range(1886, 2026, ErrorMessage = "Please enter a valid production year.")]
    public int Year { get; set; }

    [Required(ErrorMessage = "VIN is required"), RegularExpression(@"^[A-HJ-NPR-Z0-9]{17}$", ErrorMessage = "Please enter a valid 17-character VIN.")]
    public string Vin { get; set; } = default!;

    [Required(ErrorMessage = "Color is required"), MaxLength(30)]
    public string Color { get; set; } = default!;

    [Range(0, 1000000, ErrorMessage = "Mileage must be between 0 and 1,000,000.")]
    public int Mileage { get; set; }

    public bool IsElectric { get; set; }

    [MaxLength(200)]
    public string? Description { get; set; }

    public string? ImageFileName { get; set; }
}
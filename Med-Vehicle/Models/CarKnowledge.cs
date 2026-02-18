public class CarKnowledge
{
    public List<Brand>? Brands { get; set; }
    public List<BestCar>? Best_Cars_Last_10_Years { get; set; }
    public List<ElectricVehicle>? Electric_Vehicles { get; set; }
    public List<SportsCar>? Sports_Cars { get; set; }
    public List<CarHistory>? Car_History { get; set; }
}

public class BestCar
{
    public string Model { get; set; } = "";
    public string Category { get; set; } = "";
    public string Reason { get; set; } = "";
}

public class ElectricVehicle
{
    public string Model { get; set; } = "";
    public int Range_Km { get; set; }
    public string Description { get; set; } = "";
}

public class CarHistory
{
    public string Event { get; set; } = "";
    public string Description { get; set; } = "";
}


public class Brand
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Recommended_Fuel { get; set; } = "";
    public List<string>? Common_Problems { get; set; }

    public string? Country { get; set; }
    public int Founded { get; set; }

    public string? Vehicle_Category { get; set; }
    public string? Average_Maintenance_Cost { get; set; }
}



public class VehicleType
{
    public string Type { get; set; }
    public List<string> Common_Problems { get; set; }
    public string Fuel_Advice { get; set; }
}

public class SportsCar
{
    public string Model { get; set; }
    public int Horsepower { get; set; }
    public int Torque_Nm { get; set; }
    public double Zero_To_100_Kmh { get; set; }
    public int Top_Speed_Kmh { get; set; }
    public string Engine { get; set; }
    public string? Nickname { get; set; }
    public string Description { get; set; } = "";
}

public class GeneralProblem
{
    public string Symptom { get; set; }
    public List<string> Possible_Causes { get; set; }
    public string Recommendation { get; set; }
}

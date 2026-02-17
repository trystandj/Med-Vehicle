using System.Text.Json;
using System.Linq;

public class FakeAIService
{
    private readonly IWebHostEnvironment _env;
    private CarKnowledge? _data;

    // 🧠 Memory
    private string? _userName;
    private Brand? _lastBrand;
    private readonly Random _random = new();

    public FakeAIService(IWebHostEnvironment env)
    {
        _env = env;
    }

    private void LoadData()
    {
        if (_data != null)
            return;

        var path = Path.Combine(_env.WebRootPath, "data", "carKnowledge.json");

        if (!File.Exists(path))
        {
            _data = new CarKnowledge
            {
                Brands = new List<Brand>()
            };
            return;
        }

        var json = File.ReadAllText(path);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        _data = JsonSerializer.Deserialize<CarKnowledge>(json, options)
                ?? new CarKnowledge();

        _data.Brands ??= new List<Brand>();
        _data.Best_Cars_Last_10_Years ??= new List<BestCar>();
        _data.Electric_Vehicles ??= new List<ElectricVehicle>();
        _data.Sports_Cars ??= new List<SportsCar>();
        _data.Car_History ??= new List<CarHistory>();
    }

    private string GetRandomIntro()
    {
        var intros = new List<string>
        {
            "Great question!",
            "Here’s what I found:",
            "Sure, let me help you with that:",
            "Absolutely!"
        };

        return intros[_random.Next(intros.Count)];
    }

    public Task<string> AskAsync(string message)
    {
        LoadData();

        if (string.IsNullOrWhiteSpace(message))
            return Task.FromResult("Please enter a valid automotive question.");

        message = message.ToLower().Trim();

        // =========================
        // 👋 GREETING (FIXED)
        // =========================
        if (message == "hi" || message == "hello" || message == "hey" || message == "supp")
        {
            if (!string.IsNullOrEmpty(_userName))
                return Task.FromResult($"Hello {_userName}! 👋\n\nHow can I assist you today with your vehicle?");

            return Task.FromResult("Hello! 👋 What is your name?");
        }

        if (message.StartsWith("my name is"))
        {
            _userName = message.Replace("my name is", "").Trim();
            return Task.FromResult($"Nice to meet you {_userName}! 🚗\n\nHow can I help you today?");
        }

        // =========================
        // 📘 HELP
        // =========================
        if (message.Contains("help") || message.Contains("what can you do"))
        {
            return Task.FromResult(
                "I specialize in automotive topics 🚗.\n\n" +
                "You can ask about:\n" +
                "- A brand (BMW, Toyota, Honda)\n" +
                "- Fuel type\n" +
                "- Common problems\n" +
                "- Electric vehicles\n" +
                "- Sports cars\n" +
                "- History\n" +
                "- Budget recommendations\n\n" +
                "If you need human support, just type 'support'.");
        }

        // =========================
        // 📞 SUPPORT
        // =========================
        if (message.Contains("support") || message.Contains("agent") || message.Contains("human"))
        {
            return Task.FromResult(
                "📞 You will be connected with a specialist.\n\n" +
                "Email: support@medvehicle.com\n" +
                "Phone: +1 800-555-1234");
        }

        // =========================
        // 🆚 BRAND COMPARISON (MOVED UP)
        // =========================
        if (message.Contains("vs")  || message.Contains("compare"))
        {
            var parts = message.Split("vs", StringSplitOptions.TrimEntries);

            if (parts.Length == 2)
            {
                var brand1 = _data.Brands.FirstOrDefault(b => parts[0].Contains(b.Name.ToLower()));
                var brand2 = _data.Brands.FirstOrDefault(b => parts[1].Contains(b.Name.ToLower()));

                if (brand1 != null && brand2 != null)
                {
                    return Task.FromResult(
                        $"🆚 {brand1.Name} vs {brand2.Name}\n\n" +
                        $"🚗 Category:\n{brand1.Name}: {brand1.Vehicle_Category}\n{brand2.Name}: {brand2.Vehicle_Category}\n\n" +
                        $"💰 Maintenance:\n{brand1.Name}: {brand1.Average_Maintenance_Cost}\n{brand2.Name}: {brand2.Average_Maintenance_Cost}\n\n" +
                        $"🌍 Country:\n{brand1.Name}: {brand1.Country}\n{brand2.Name}: {brand2.Country}");
                }
            }
        }

        // =========================
        // 🔧 BASIC MAINTENANCE
        // =========================
        if (message.Contains("oil"))
            return Task.FromResult("🛢 Oil Change Advice:\n\nChange oil every 5,000–7,000 km depending on vehicle and oil type.");

        if (message.Contains("brake"))
            return Task.FromResult("🛑 Brake Maintenance:\n\nIf you hear squeaking or grinding, inspect brake pads immediately.");

        if (message.Contains("battery"))
            return Task.FromResult("🔋 Battery Info:\n\nMost car batteries last 3–5 years.");

        // =========================
        // 💵 BUDGET
        // =========================
        if (message.Contains("budget") || message.Contains("affordable"))
        {
            var affordable = _data.Brands
                .Where(b => b.Average_Maintenance_Cost == "Low");

            var response = "💵 Budget-Friendly Brands:\n\n";

            foreach (var b in affordable)
                response += $"🚗 {b.Name}\n";

            return Task.FromResult(response);
        }

        // =========================
        // 📊 RANKING
        // =========================
        if (message.Contains("maintenance ranking") || message.Contains("cheapest"))
        {
            var ranking = _data.Brands
                .OrderBy(b => b.Average_Maintenance_Cost switch
                {
                    "Low" => 1,
                    "Medium" => 2,
                    "High" => 3,
                    _ => 4
                });

            var response = "📊 Maintenance Cost Ranking:\n\n";

            foreach (var b in ranking)
                response += $"🚗 {b.Name} → {b.Average_Maintenance_Cost}\n";

            return Task.FromResult(response);
        }

        // =========================
        // ⚡ ELECTRIC
        // =========================
        if (message.Contains("electric"))
        {
            var response = "⚡ Popular Electric Vehicles:\n\n";

            foreach (var ev in _data.Electric_Vehicles)
            {
                response += $"🔋 {ev.Model}\nRange: {ev.Range_Km} km\n{ev.Description}\n\n";
            }

            return Task.FromResult(response);
        }

        // =========================
        // 🏎 SPORTS
        // =========================
        if (message.Contains("sports car") || message.Contains("supercar"))
        {
            var response = "🏎 Notable Sports Cars:\n\n";

            foreach (var car in _data.Sports_Cars)
            {
                response += $"🔥 {car.Model}";
                if (!string.IsNullOrEmpty(car.Nickname))
                    response += $" ({car.Nickname})";
                response += $"\n{car.Description}\n\n";
            }

            return Task.FromResult(response);
        }

        // =========================
        // 📜 HISTORY
        // =========================
        if (message.Contains("history"))
        {
            var response = "📜 Automotive History:\n\n";

            foreach (var h in _data.Car_History)
                response += $"🕰 {h.Event}\n{h.Description}\n\n";

            return Task.FromResult(response);
        }

        // =========================
        // 🏆 BEST CARS
        // =========================
        if (message.Contains("best car"))
        {
            var response = "🏆 Best Cars of the Last 10 Years:\n\n";

            foreach (var car in _data.Best_Cars_Last_10_Years)
                response += $"🚗 {car.Model} ({car.Category})\nReason: {car.Reason}\n\n";

            return Task.FromResult(response);
        }

        // =========================
        // 🔎 BRAND DETECTION
        // =========================
        var brand = _data.Brands
            .FirstOrDefault(b => message.Contains(b.Name.ToLower()));

        if (brand != null)
        {
            _lastBrand = brand;

            if (message.Contains("country") || message.Contains("from"))
                return Task.FromResult($"{brand.Name} is based in {brand.Country}.");

            if (message.Contains("maintain") || message.Contains("expensive"))
                return Task.FromResult($"💰 {brand.Name} maintenance cost: {brand.Average_Maintenance_Cost}");

            if (message.Contains("founded") || message.Contains("when"))
                return Task.FromResult($"{brand.Name} was founded in {brand.Founded}.");

            if (message.Contains("problem") || message.Contains("issue"))
                return Task.FromResult($"🚗 {brand.Name} – Common Problems:\n\n- " +
                                       string.Join("\n- ", brand.Common_Problems ?? new List<string>()));

            if (message.Contains("fuel") || message.Contains("gas"))
                return Task.FromResult($"⛽ {brand.Name} Recommended Fuel:\n\n{brand.Recommended_Fuel}");

            return Task.FromResult(
                $"{GetRandomIntro()}\n\n🚗 Brand: {brand.Name}\n\n📖 {brand.Description}");
        }

        // =========================
        // 🧠 MEMORY CONTEXT
        // =========================
        if (_lastBrand != null)
        {
            if (message.Contains("fuel"))
                return Task.FromResult($"⛽ {_lastBrand.Name} Recommended Fuel:\n\n{_lastBrand.Recommended_Fuel}");

            if (message.Contains("problem"))
                return Task.FromResult($"🚗 {_lastBrand.Name} – Common Problems:\n\n- " +
                                       string.Join("\n- ", _lastBrand.Common_Problems ?? new List<string>()));
        }

        // =========================
        // DEFAULT
        // =========================
        return Task.FromResult(
            "I'm here to help with automotive topics 🚗.\n\n" +
            "Try asking about a brand, electric cars, sports cars, history, ranking, or maintenance.");
    }
}

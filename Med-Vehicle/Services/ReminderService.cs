using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Med_Vehicle.Models;
using Med_Vehicle.Infrastructure;
using MongoDB.Driver;

namespace Med_Vehicle.Services;

public class ReminderService
{
    private readonly IMongoCollection<Car> _cars;
    private readonly IMongoCollection<User> _users;

    public ReminderService(NamedCollection dbContext)
    {
        _cars = dbContext.GetCollection<Car>("Cars");
        _users = dbContext.GetCollection<User>("Users");
    }

    public async Task<List<ReminderItem>> GetDueRemindersAsync(DateTime now)
    {
        var filter = Builders<Car>.Filter.Where(c => c.Registration != null
            && c.Registration.WantsReminder
            && !c.Registration.ReminderSent
            && c.Registration.ReminderDate <= now);

        var cars = await _cars.Find(filter).ToListAsync();

        var result = new List<ReminderItem>();

        foreach (var car in cars)
        {
            string ownerName = "Unknown";
            string ownerEmail = "";

            if (!string.IsNullOrEmpty(car.UserId))
            {
                var user = await _users.Find(u => u.Id == car.UserId).FirstOrDefaultAsync();
                if (user != null)
                {
                    ownerName = $"{user.FirstName} {user.LastName}".Trim();
                    ownerEmail = user.Email;
                }
            }

            result.Add(new ReminderItem
            {
                OwnerName = string.IsNullOrEmpty(ownerName) ? "Unknown" : ownerName,
                OwnerEmail = ownerEmail ?? string.Empty,
                CarInfo = $"{car.Year} {car.Make} {car.Model}",
                ReminderDate = car.Registration!.ReminderDate
            });
        }

        return result;
    }

    public async Task<int> SendDueRemindersAsync(DateTime now)
    {
        // This method marks reminders as sent and returns how many were marked.
        var filter = Builders<Car>.Filter.Where(c => c.Registration != null
            && c.Registration.WantsReminder
            && !c.Registration.ReminderSent
            && c.Registration.ReminderDate <= now);

        var cars = await _cars.Find(filter).ToListAsync();

        int count = 0;
        foreach (var car in cars)
        {
            if (car.Registration is null) continue;

            // Mark reminder sent
            car.Registration.ReminderSent = true;
            await _cars.ReplaceOneAsync(c => c.Id == car.Id, car);
            count++;
        }

        return count;
    }
}

using System;

namespace Med_Vehicle.Models;

public class ReminderItem
{
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string CarInfo { get; set; } = string.Empty;
    public DateTime ReminderDate { get; set; }
}

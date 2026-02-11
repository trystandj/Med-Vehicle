namespace YourApp.Models;

public class RegistrationInfo
{
    public DateTime LastRegistrationDate { get; set; }

    public int RegistrationLengthYears { get; set; }

    public DateTime ExpirationDate { get; set; }

    public DateTime ReminderDate { get; set; }

    public bool WantsReminder { get; set; }

    public bool ReminderSent { get; set; }
}

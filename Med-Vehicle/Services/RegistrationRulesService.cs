using YourApp.Models;

namespace YourApp.Services;

public class RegistrationRulesService
{
    public void CalculateRegistrationDates(RegistrationInfo info)
    {
        info.ExpirationDate = info.LastRegistrationDate.AddYears(info.RegistrationLengthYears);
        info.ReminderDate = info.ExpirationDate.AddMonths(-1);
    }
}

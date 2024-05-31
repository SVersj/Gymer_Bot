using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;

namespace gymer
{
    public class ReminderService
    {
        private readonly Database _database;
        private readonly TelegramBotClient _botClient;

        public ReminderService(Database database, TelegramBotClient botClient)
        {
            _database = database;
            _botClient = botClient;
        }

        public async Task SendReminders()
        {
            var reminders = _database.Reminders
                .Where(r => r.ReminderDate.Date == DateTime.Now.Date)
                .ToList();

            foreach (var reminder in reminders)
            {
                await _botClient.SendTextMessageAsync(reminder.UserId, $"Напоминание: {reminder.Message}");
            }
        }
    }
}

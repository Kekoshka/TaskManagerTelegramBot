using TaskManagerTelegramBot.Classes;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace TaskManagerTelegramBot
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly string Token = "";
        TelegramBotClient _client;
        List<User> _users;
        Timer _timer;
        List<string> _messeges = new()
        {
            "Здравствуйте, приветствуем в напоминаторе, мы будем напоминать вам о событиях и мероприятиях, добавьте бота в список контактов и настройте уведомления!",
            
            "Укажите дату и время напоминания в следующем формате:" +
            "\n<i><b>12:51 26.04.2025</b>" +
            "Напомни о том, что я хотел сходить в магазин.",
            
            "Кажется, что-то не получилось," +
            "Укажите дату и время напоминания в следующем формате:" +
            "\n<i><b>12:51 26.04.2025</b>" +
            "Напомни о том, что я хотел сходить в магазин.",

            "",

            "Задачи пользователя не найдены",

            "Событие удалено",

            "Все события удалены."
        };
        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                await Task.Delay(1000, stoppingToken);
            }
        }
        public bool CheckFormatDateTime(string value, out DateTime time) => DateTime.TryParse(value, out time);
        private static ReplyKeyboardMarkup GetButtons()
        {
            List<KeyboardButton> keyboardButtons = new List<KeyboardButton>();
            keyboardButtons.Add(new KeyboardButton("Удалить все задачи"));
            return new ReplyKeyboardMarkup { Keyboard = new List<List<KeyboardButton>>() { keyboardButtons } };
        }
        public static InlineKeyboardMarkup DeleteEvent(string message)
        {
            List<InlineKeyboardButton> inlineKeyboardButtons = new List<InlineKeyboardButton>();
            inlineKeyboardButtons.Add(new InlineKeyboardButton("Удалить"));
            return new InlineKeyboardMarkup(inlineKeyboardButtons);
        }

    }
}

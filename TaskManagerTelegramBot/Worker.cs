using TaskManagerTelegramBot.Classes;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
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
        public async void SendMessage(long chatId, int typeMessage)
        {
            if(typeMessage != 3)
            {
                await _client.SendMessage(chatId,
                    _messeges[typeMessage], 
                    ParseMode.Html,
                    replyMarkup: GetButtons());
            }
            else if(typeMessage == 3)
            {
                await _client.SendMessage(chatId,"Указанное время и дата не могут быть установлены," +
                    $"потому-что сейчас уже {DateTime.Now.ToString("HH.mm dd.MM.yyyy")}");
            }
        }
        public async void Command(long chatId, string command)
        {
            var lowerCommand = command.ToLower();
            if (lowerCommand == "/start") SendMessage(chatId, 0);
            else if (lowerCommand == "/create_task") SendMessage(chatId, 1);
            else if (lowerCommand == "/list_tasks")
            {
                User user = _users.Find(u => u.Id == chatId);
                if(user is null) SendMessage(chatId, 4);
                else if(user.Events.Count == 0) SendMessage(chatId, 4);
                else
                {
                    foreach (Event ev in user.Events)
                        await _client.SendMessage(chatId,
                            $"Уведомить пользователя: {ev.Time.ToString("HH.mm dd.MM.yyyy")}" +
                            $"\nСообщение: {ev.Message}",
                            replyMarkup: DeleteEvent(ev.Message));
                }
            }
        }
    }
}

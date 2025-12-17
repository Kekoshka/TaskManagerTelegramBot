using TaskManagerTelegramBot.Classes;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TaskManagerTelegramBot
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly string _token = "7844986009:AAERICRYFE-lBAW9m9_TUHbm4no31cEXQAc";
        TelegramBotClient _client;
        List<Classes.User> _users = new();
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
            _client = new TelegramBotClient(_token);
            _client.StartReceiving(HandleUpdateAsync, HandleErrorAsync, null, new CancellationTokenSource().Token);
            TimerCallback timerCallback = new TimerCallback(Tick);
            _timer = new Timer(timerCallback, 0, 0, 60 * 1000);
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
            if (typeMessage != 3)
            {
                await _client.SendMessage(chatId,
                    _messeges[typeMessage],
                    ParseMode.Html,
                    replyMarkup: GetButtons());
            }
            else if (typeMessage == 3)
            {
                await _client.SendMessage(chatId, "Указанное время и дата не могут быть установлены," +
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
                var user = _users.Find(u => u.Id == chatId);
                if (user is null) SendMessage(chatId, 4);
                else if (user.Events.Count == 0) SendMessage(chatId, 4);
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
        private void GetMessages(Message message)
        {
            Console.WriteLine($"Получено сообщение: {message.Text} от пользователя: {message.Chat.Username}");
            long userId = message.Chat.Id;
            string userMessage = message.Text;

            if (message.Text.Contains("/")) Command(message.Chat.Id, message.Text);
            else if(message.Text.Equals("Удалить все задачи"))
            {
                var user = _users.Find(u => u.Id == message.Chat.Id);
                if (user is null) SendMessage(message.Chat.Id, 4);
                else if (user.Events.Count == 0) SendMessage(user.Id, 4);
                else
                {
                    user.Events = new List<Event>();
                    SendMessage(user.Id, 6);
                }
            }
            else
            {
                var user = _users.Find(u => u.Id == message.Chat.Id);
                if(user is null)
                {
                    user = new Classes.User(message.Chat.Id);
                    _users.Add(user);
                }
                string[] info = message.Text.Split('\n');
                if(info.Length < 2)
                {
                    SendMessage(message.Chat.Id, 2);
                    return;
                }
                DateTime time;
                if (CheckFormatDateTime(info[0], out time) == false)
                {
                    SendMessage(message.Chat.Id, 2);
                    return;
                }
                if (time < DateTime.Now) SendMessage(message.Chat.Id, 3);
                user.Events.Add(new Event(time, message.Text.Replace(time.ToString("HH.mm dd.MM.yyyy") + "\n", "")));
            }
        }
        private async Task HandleUpdateAsync(ITelegramBotClient client,
            Update update,
            CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message) GetMessages(update.Message);

            else if(update.Type == UpdateType.CallbackQuery)
            {
                CallbackQuery query = update.CallbackQuery;
                var user = _users.Find(u => u.Id == query.Message.Chat.Id);
                var ev = user.Events.Find(u => u.Message == query.Data);
                user.Events.Remove(ev);
                SendMessage(query.Message.Chat.Id, 5);
            }
        }

        private async Task HandleErrorAsync(
            ITelegramBotClient client,
            Exception ex,
            HandleErrorSource source,
            CancellationToken cancellationToken) =>
            Console.WriteLine("Ошибка" + ex.Message);

        public async void Tick(Object obj)
        {
            string TimeNow = DateTime.Now.ToString("HH:mm dd.MM.yyyy");
            foreach(var user in _users)
                for(int i = 0;i < user.Events.Count; i++)
                {
                    if (user.Events[i].Time.ToString("HH:mm dd.MM.yyyy") != TimeNow) continue;
                    await _client.SendMessage(user.Id,
                        "Напоминание: " + user.Events[i].Message);
                    user.Events.Remove(user.Events[i]);
                }
        }
    }
}

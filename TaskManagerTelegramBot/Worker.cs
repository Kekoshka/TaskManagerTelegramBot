using Microsoft.EntityFrameworkCore;
using TaskManagerTelegramBot.Classes;
using TaskManagerTelegramBot.Context;
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
        Timer _timer;
        List<string> _messeges = new()
        {
            "Здравствуйте, приветствуем в напоминаторе, мы будем напоминать вам о событиях и мероприятиях, добавьте бота в список контактов и настройте уведомления!",

            "Укажите дату и время напоминания в следующем формате:" +
            "\n<i><b>12:51 26.04.2025</b>" +
            "\nНапомни о том, что я хотел сходить в магазин.</i>",

            "Кажется, что-то не получилось," +
            "Укажите дату и время напоминания в следующем формате:" +
            "\n<i><b>12:51 26.04.2025</b></i>" +
            "\nНапомни о том, что я хотел сходить в магазин.",

            "",

            "Задачи пользователя не найдены",

            "Событие удалено",

            "Все события удалены.",

            "Кажется, что-то не получилось," +
            "напишите команду в следующем формате:" +
            "/create_repeatable_task" +
            "\n<i><b>18:00 </b></i>" +
            "\nНапомни о том, что нужно покормить собаку."
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
        public bool CheckFormatTimeOnly(string value, out TimeOnly time) => TimeOnly.TryParse(value, out time);

        private static ReplyKeyboardMarkup GetButtons()
        {
            List<KeyboardButton> keyboardButtons = new List<KeyboardButton>();
            keyboardButtons.Add(new KeyboardButton("Удалить все задачи"));
            return new ReplyKeyboardMarkup { Keyboard = new List<List<KeyboardButton>>() { keyboardButtons } };
        }
        public static InlineKeyboardMarkup DeleteEvent(string message)
        {
            List<InlineKeyboardButton> inlineKeyboardButtons = new List<InlineKeyboardButton>();
            inlineKeyboardButtons.Add(new InlineKeyboardButton("Удалить",message));
            return new InlineKeyboardMarkup(inlineKeyboardButtons);
        }
        public async void SendMessage(long chatId, int typeMessage)
        {
            if (typeMessage != 3)
            {
                await _client.SendMessage(
                    chatId,
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
            using var context = new ApplicationContext();

            var lowerCommand = command.ToLower();
            if (lowerCommand == "/start") SendMessage(chatId, 0);
            else if (lowerCommand == "/create_task") SendMessage(chatId, 1);
            else if (lowerCommand == "/list_tasks")
            {
                var user = context.Users.Include(u => u.RepeatableEvents).Include(u => u.Events).FirstOrDefault(u => u.Id == chatId);
                if (user is null) SendMessage(chatId, 4);
                else if (user.Events.Count == 0 && user.RepeatableEvents.Count == 0) SendMessage(chatId, 4);
                else
                {
                    foreach (Event ev in user.Events)
                        await _client.SendMessage(chatId,
                            $"Уведомить пользователя: {ev.Time.ToString("HH.mm dd.MM.yyyy")}" +
                            $"\nСообщение: {ev.Message}",
                            replyMarkup: DeleteEvent(ev.Id.ToString()));
                    foreach (var ev in user.RepeatableEvents)
                        await _client.SendMessage(chatId,
                            $"Уведомить пользователя: {ev.Time.ToString("HH:mm")}" +
                            $"\nСообщение: {ev.Message}",
                            replyMarkup: DeleteEvent(ev.Id.ToString()));

                }
            }
            else if (lowerCommand.Split('\n')[0] == "/create_repeatable_task")
            {

                var user = context.Users.Include(u => u.RepeatableEvents).FirstOrDefault(u => u.Id == chatId);
                if (user is null)
                {
                    user = new Classes.User() { Id = chatId };
                    context.Users.Add(user);
                }
                string[] info = command.Split('\n');
                if (info.Length < 3)
                {
                    SendMessage(chatId, 7);
                    return;
                }
                TimeOnly time;
                if (CheckFormatTimeOnly(info[1], out time) == false)
                {
                    SendMessage(chatId, 7);
                    return;
                }
                user.RepeatableEvents.Add(new RepeatableEvent() {Time = time, Message = info[2] });
                context.SaveChanges();
            }
        }
        private void GetMessages(Message message)
        {
            using var context = new ApplicationContext();
            
            Console.WriteLine($"Получено сообщение: {message.Text} от пользователя: {message.Chat.Username}");
            long userId = message.Chat.Id;
            string userMessage = message.Text;

            if (message.Text.Contains("/")) Command(message.Chat.Id, message.Text);
            else if(message.Text.Equals("Удалить все задачи"))
            {
                var user = context.Users.Include(u => u.Events).Include(u => u.RepeatableEvents).First(u => u.Id == message.Chat.Id);
                if (user is null) SendMessage(message.Chat.Id, 4);
                else if (user.Events.Count == 0) SendMessage(user.Id, 4);
                else
                {
                    user.Events.Clear();
                    user.RepeatableEvents.Clear();
                    SendMessage(user.Id, 6);
                    context.SaveChanges();
                }
            }
            else
            {
                var user = context.Users.Include(u => u.Events).FirstOrDefault(u => u.Id == userId);
                if(user is null)
                {
                    user = new Classes.User() { Id = message.Chat.Id };
                    context.Users.Add(user);
                    context.SaveChanges();
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
                user.Events.Add(new Event() { Time = time, Message = message.Text.Replace(time.ToString("HH.mm dd.MM.yyyy") + "\n", "")});
                context.SaveChanges();
            }
        }
        private async Task HandleUpdateAsync(ITelegramBotClient client,
            Update update,
            CancellationToken cancellationToken)
        {
            using var context = new ApplicationContext();

            if (update.Type == UpdateType.Message) GetMessages(update.Message);

            else if(update.Type == UpdateType.CallbackQuery)
            {
                CallbackQuery query = update.CallbackQuery;
                var user = context.Users.Include(u => u.Events).Include(u => u.RepeatableEvents).First(u => u.Id == query.Message.Chat.Id);
                var ev = user.Events.FirstOrDefault(u => u.Id == Guid.Parse(query.Data));
                var rEv = user.RepeatableEvents.FirstOrDefault(u => u.Id == Guid.Parse(query.Data));
                if(ev is not null)
                    user.Events.Remove(ev);
                if (rEv is not null)
                    user.RepeatableEvents.Remove(rEv);
                SendMessage(query.Message.Chat.Id, 5);
            }
            context.SaveChanges();
        }

        private async Task HandleErrorAsync(
            ITelegramBotClient client,
            Exception ex,
            HandleErrorSource source,
            CancellationToken cancellationToken) =>
            Console.WriteLine("Ошибка" + ex.Message);

        public async void Tick(Object obj)
        {
            using var context = new ApplicationContext();
            string TimeNow = DateTime.Now.ToString("HH:mm dd.MM.yyyy");
            foreach(var user in context.Users.Include(u => u.RepeatableEvents).Include(u => u.Events))
            {
                for (int i = 0; i < user.Events.Count; i++)
                {
                    if (user.Events.ToList()[i].Time.ToString("HH:mm dd.MM.yyyy") != TimeNow) continue;
                    await _client.SendMessage(user.Id, "Напоминание: " + user.Events.ToList()[i].Message);
                    user.Events.Remove(user.Events.ToList()[i]);
                }
                foreach(var ev in user.RepeatableEvents)
                {
                    if(ev.Time == TimeOnly.Parse(DateTime.Now.ToString("HH:mm")))
                    await _client.SendMessage(user.Id, "Напоминание: " + ev.Message);
                }
            }
            context.SaveChanges();
        }
    }
}

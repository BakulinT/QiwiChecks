using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

using ServiceApi.QiwiJs;

namespace ServiceApi.TelegramBot
{
    class TelegramBotApi
    {
        private static Config config { get; set; }
        private static string nameBot = "@receiptsAh_bot";
        private static TelegramBotClient bot;

        public static async Task StartBot(Config js)
        {
            config = js;
            Console.Title = "Telegram Bot";

            bot = new TelegramBotClient(config.BotToken);

            using var cts = new CancellationTokenSource();
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[] { UpdateType.Message, UpdateType.CallbackQuery },
                Limit = 50
            };

            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken: cts.Token);

            var me = await bot.GetMeAsync();

            Console.WriteLine($"> Старт для @{me.Username}");
            Console.ReadLine();
            Console.WriteLine("> Stop bot...");

            cts.Cancel();
        }
        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message!.Type == MessageType.Text)
                await Command(botClient, update, cancellationToken);
            else if (update.Type == UpdateType.CallbackQuery)
                await CallbackQueryAdoption(botClient, update, cancellationToken);
        }
        private static async Task Command(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            string answer = "";
            long chatId = update.Message.Chat.Id;

            string messageText = update.Message.Text.Split('@')[0];

            if (messageText[0] != '/')
                return;
            switch (messageText)
            {
                case "/start":
                    answer = "\U0001F680 БотКвитанции запущен. Нажмите /help, чтобы узнать возможности бота.";
                    break;
                case "/help":
                    answer = "Список команд:" +
                        "\n /help - помощь" +
                        "\n /receipt - платежи Qiwi";
                    break;
                case "/receipt":
                    await QiwiAdd(botClient, cancellationToken, chatId);
                    return;
                default:
                    answer = "Такой команды нет!";
                    break;
            }
            if (answer.Length > 0 )
            {
                Message message = await botClient.SendTextMessageAsync(
                     chatId: chatId,
                     text: answer,
                     cancellationToken: cancellationToken);
                return;
            }

            Console.WriteLine(update.Message);
        }
        private static async Task CallbackQueryAdoption(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            long chatId = update.CallbackQuery.Message.Chat.Id;
            string[] date = update.CallbackQuery.Data.Split('_');
            if (date[0] == "stopPrint")
            {
                Message message = await botClient.SendTextMessageAsync(
                     chatId: chatId,
                     text: "Остановка...",
                     cancellationToken: cancellationToken);
                return;
            }
            if (date[0] == "nextIdQ")
                await QiwiAdd(botClient, cancellationToken, chatId, date[1], date[2]);
        }
        private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
        private static async Task QiwiAdd(ITelegramBotClient botClient, CancellationToken cancellationToken, long chatId, string nextTxnId=null, string nextTxnDate=null)
        {
            QiwiDate qiwi = QiwiApi.OperationHistory(config.QiwiKey, config.phone, nextTxnId, nextTxnDate);
            int count;

            while (true)
            {
                count = 0;
                foreach (data q in qiwi.data)
                {
                    Stream ms = JpgConvert.CreateImageQiwi(q);
                    if (ms.Length < 1)
                        continue;
                    count++;
                    Message message = await botClient.SendDocumentAsync(
                        chatId: chatId,
                        document: new InputOnlineFile(ms, $"{q.account}.jpeg"));
                    Thread.Sleep(250);
                }
                nextTxnDate = qiwi.nextTxnDate;
                nextTxnId = qiwi.nextTxnId;
                if (nextTxnDate is null || nextTxnId is null)
                    break;
                if (count < 6)
                {
                    qiwi = QiwiApi.OperationHistory(config.QiwiKey, config.phone, nextTxnId, nextTxnDate.Replace(":", "%3A").Replace("+", "%2B"));
                }
                else
                    break;
            }
            if (qiwi.nextTxnId is null || qiwi.nextTxnDate is null)
                return;
            InlineKeyboardMarkup inlineKeyboard = new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(text: "Продолжить", callbackData: $"nextIdQ_{qiwi.nextTxnId}_{qiwi.nextTxnDate.Replace(":", "%3A").Replace("+", "%2B")}"),
                        InlineKeyboardButton.WithCallbackData(text: "Стоп", callbackData: "stopPrint"),
                    }
                };

            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Продолжить вывод Qiwi?",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }
    }
}

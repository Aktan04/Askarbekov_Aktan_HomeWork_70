using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TgBot;

public class Bot
{
    private readonly TelegramBotClient _bot;

    public Bot(string token)
    {
        _bot = new TelegramBotClient(token);
    }
    
    public void StartBot()
    {
        _bot.StartReceiving(HandleUpdate, HandleError);
        while (true)
        {
            Console.WriteLine("Bot is worked all right");
            Thread.Sleep(int.MaxValue);
        }
    }
    
    private async Task HandleUpdate(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        if (update.Type == UpdateType.Message && update.Message!.Type == MessageType.Text)
        {
            var message = update.Message;

            switch (message.Text)
            {
                case "/start":
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Добро пожаловать в игру 'Камень, ножницы, бумага'!\n" +
                              "Команды:\n" +
                              "/start - начать работу с ботом\n" +
                              "/help - показать правила игры\n" +
                              "/game - начать игру"
                    );
                    break;

                case "/help":
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Правила игры:\n" +
                              "Камень побеждает ножницы, ножницы побеждают бумагу, бумага побеждает камень.\n" +
                              "Используйте команду /game, чтобы начать игру."
                    );
                    break;

                case "/game":
                    await StartGame(botClient, message.Chat.Id);
                    break;
            }
        }
        else if (update.Type == UpdateType.CallbackQuery)
        {
            await HandleQuery(botClient, update.CallbackQuery!);
        }
    }

    private async Task HandleQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        var userChoice = callbackQuery.Data;

        if (userChoice == "repeat")
        {
            await StartGame(botClient, callbackQuery.Message.Chat.Id);
        }
        else if (userChoice == "end")
        {
            await botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: "Спасибо за игру! До новых встреч!"
            );
        }
        else
        {
            var botChoice = GetRandomChoice();
            var result = DetermineWinner(userChoice, botChoice);

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Повторить", "repeat"),
                    InlineKeyboardButton.WithCallbackData("Завершить", "end")
                }
            });

            await botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: $"Вы выбрали: {userChoice}\nБот выбрал: {botChoice}\n{result}\n\nХотите сыграть еще раз?",
                replyMarkup: keyboard
            );
        }
    }
    
    private async Task StartGame(ITelegramBotClient botClient, long chatId)
    {
        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Камень", "Камень"),
                InlineKeyboardButton.WithCallbackData("Ножницы", "Ножницы"),
                InlineKeyboardButton.WithCallbackData("Бумага", "Бумага")
            }
        });

        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Выберите свой ход:",
            replyMarkup: keyboard
        );
    }

    private string GetRandomChoice()
    {
        var choices = new[] { "Камень", "Ножницы", "Бумага" };
        var random = new Random();
        return choices[random.Next(choices.Length)];
    }


    private string DetermineWinner(string userChoice, string botChoice)
    {
        if (userChoice == botChoice)
        {
            return "Ничья!";
        }

        return (userChoice, botChoice) switch
        {
            ("Камень", "Ножницы") => "Камень побеждает ножницы. Вы выиграли!",
            ("Камень", "Бумага") => "Бумага побеждает камень. Бот выиграл!",
            ("Ножницы", "Бумага") => "Ножницы побеждают бумагу. Вы выиграли!",
            ("Ножницы", "Камень") => "Камень побеждает ножницы. Бот выиграл!",
            ("Бумага", "Камень") => "Бумага побеждает камень. Вы выиграли!",
            ("Бумага", "Ножницы") => "Ножницы побеждают бумагу. Бот выиграл!",
            _ => "Произошла ошибка. Попробуйте еще раз."
        };
    }
    
    async Task HandleError(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
    {
        await Console.Error.WriteLineAsync(exception.Message);
    }
}
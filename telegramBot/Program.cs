using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using telegramBot;

internal class Program
{
    static string firstName = "Jasur";
    static string lastname = "Mamatqulov";
    static string phone = "+9987777777";
    static TelegramBotClient telegramBotClient = new TelegramBotClient("7172964202:AAFiwY1DATgpeeKyZ1Z1tLxwusuCvM-7_eE");
    private static void Main(string[] args)
    {

        telegramBotClient.StartReceiving(
            updateHandler: UpdateHandlerAsync,
            pollingErrorHandler: ErrorHandlerAsync
            );

        Console.ReadKey();
    }
    static Task ErrorHandlerAsync(ITelegramBotClient client, Exception exception, CancellationToken token)
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
   
    static async Task UpdateHandlerAsync(ITelegramBotClient client, Update update, CancellationToken token)
    {
        return;

        if (update.InlineQuery is not null)
        {
            string str = update.InlineQuery.Query;

            List<InlineQueryResult> res = new List<InlineQueryResult>();

            var skills = await Auth.SearchSkillByNameAsync(str);


            foreach (var skill in skills)
            {
                var article = new InlineQueryResultArticle(skill.Id.ToString(), skill.Name,
                new InputTextMessageContent(skill.Id.ToString() + $" {skill.Name}"));
                res.Add(article);
            }

            await client.AnswerInlineQueryAsync(update.InlineQuery.Id, res.ToArray(), isPersonal: true, cacheTime: 300);
        }

        else if (update.Message is not null)
        {
            if (update.Message.Type is MessageType.Text)
            {
                if (update.Message.Text == "/start")
                {
                    long telegId = update.Message.From.Id;
                    if (!await Auth.LoginAsync(telegId))
                    {
                        await telegramBotClient.SendTextMessageAsync(update.Message.Chat.Id, "Ism-familiyangizni kiriting(Valiyev Ali)");
                    }
                }
                else if(int.TryParse(update.Message.Text.Split(' ')[0], out int id))
                {
                    EditMessage(client, update.Message.Chat.Id, update.Message.MessageId, update.Message.Text);
                    var listemp =  await Auth.SearchEmployeesBySkillNameAsync(id);
                    if (listemp != null)
                    {
                        string messageText = "";
                        int number = 0;
                        foreach (var text in listemp)
                        {
                            messageText += $"<b>{++number}.Name : </b>{text.Name}\n" +
                                             $"<b>Phone number : </b>{text.PhoneNumber}\n" +
                                             $"<b>Address : </b> {text.Address}\n\n";
                        }
                        await client.SendTextMessageAsync(update.Message.Chat.Id, messageText.ToString(), parseMode: ParseMode.Html);
                        
                    }
                }
                else if(update.Message.Text.Split(' ').Length > 1)
                {
                    lastname = update.Message.Text.Split(' ')[0];
                    firstName = update.Message.Text.Split(' ')[1];

                    await telegramBotClient.SendTextMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text: "Telefon raqamizni ulashing",
                        replyMarkup:new ReplyKeyboardMarkup
                            (
                                KeyboardButton.WithRequestContact("Telefon raqamizni ulashish")
                            )
                        {
                            ResizeKeyboard = true,
                            OneTimeKeyboard = true
                        }
                        );
                }
                else
                {
                    await SendMessageWithSearchButton(client, update.Message.Chat.Id);
                }
            }

            else if (update.Message.Type is MessageType.Contact)
            {
                phone = update.Message.Contact.PhoneNumber;

                Client createdClient = new Client()
                {
                    LastName = lastname,
                    Name = firstName,
                    PhoneNumber = phone,
                    TelegramId = update.Message.From.Id
                };

                await Auth.RegistrAsync(createdClient);
                await client.SendTextMessageAsync(update.Message.Chat.Id, "Muvaffaqiyatli ro'yxatdan o'tdingiz.");
                await SendMessageWithSearchButton(client, update.Message.Chat.Id);
            }
        }
    }
    private static async Task SendMessageWithSearchButton(ITelegramBotClient botClient, ChatId chatId)
    {
        string messageText = "Qidiruvni boshlash uchun quyidagi qidiruv tugmasidan foydalaning:";

        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
        new []
        {
            // Use WithSwitchInlineQueryCurrentChat to provide an inline query when the button is clicked
            InlineKeyboardButton.WithSwitchInlineQueryCurrentChat("Search 🔎", " ")
        }
    });

        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: messageText,
            replyMarkup: inlineKeyboard
        );
    }

    private static async Task HandleInlineQuery(ITelegramBotClient botClient, InlineQuery inlineQuery)
    {
        string searchQuery = inlineQuery.Query;

        // You can process the search query and generate the appropriate results here
        // For simplicity, let's just echo the search query back to the user as a message
        await botClient.AnswerInlineQueryAsync(
            inlineQuery.Id,
            new InlineQueryResult[]
            {
            new InlineQueryResultArticle(
                id: "1",
                title: "Search Result",
                inputMessageContent: new InputTextMessageContent(searchQuery)
            )
            }
        );
    }
    private static async Task EditMessage(ITelegramBotClient botClient, ChatId chatId, int messageId, string newText)
    {
        await botClient.EditMessageTextAsync(
            chatId: chatId,
            messageId: messageId,
            text: newText,
            parseMode: ParseMode.Markdown
        );
    }

}
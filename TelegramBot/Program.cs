using System;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;
using HtmlAgilityPack;

namespace TelegramBot
{
    class Program
    {
        private static string token { get; set; } = "1962031709:AAFGse0Nr1C6SYFuX9hQgC0N9lCspqQbM5M";
        private static TelegramBotClient client;
        private static Dictionary<long, string> usersData = new Dictionary<long, string>();

        static void Main(string[] args)
        {
            client = new TelegramBotClient(token);
            client.StartReceiving();
            client.OnMessage += OnMessageHandler;

            Console.ReadLine();
            client.StopReceiving();
        }

        private static async void OnMessageHandler(object sender, MessageEventArgs e)
        {
            var msg = e.Message;
            var userId = e.Message.From.Id;

            if (msg.Text != null)
            {
                Console.WriteLine($"Пришло сообщение с текстом \"{ msg.Text }\" ");

                switch (msg.Text)
                {
                    case "Найти книгу":
                        if (usersData[userId].Length == 0)
                            await client.SendTextMessageAsync(msg.Chat.Id, "Введите имя автора или название книги");
                        else
                        {
                            await client.SendTextMessageAsync(msg.Chat.Id, FindBook(userId));
                            await client.SendTextMessageAsync(msg.Chat.Id, "Введите имя автора или название книги");
                        }
                        break;
                    case "Найти автора":
                        if (usersData[userId].Length == 0)
                        {
                            await client.SendTextMessageAsync(msg.Chat.Id, "Введите имя автора или название книги");
                            break;
                        }

                        var keyboard = FindAuthor(userId);
                        if (keyboard == null)
                            await client.SendTextMessageAsync(msg.Chat.Id, "Такого автора не существует");
                        else
                        {
                            await client.SendTextMessageAsync(msg.Chat.Id, "фыва", replyMarkup: keyboard);
                            await client.SendTextMessageAsync(msg.Chat.Id, "Введите имя автора или название книги");
                        }
                        break;
                    default:
                        if (usersData.ContainsKey(userId))
                            usersData[userId] = e.Message.Text;
                        else usersData.Add(userId, e.Message.Text);

                        await client.SendTextMessageAsync(msg.Chat.Id, "Выберите, что будете искать", replyMarkup: GetButtons());
                        break;
                }
                
            }
        }

        private static string FindBook(long userId)
        {
            if (usersData[userId].Length == 0)
                return "Вы не ввели название";
            var web = new HtmlWeb();
            var htmlDoc = web.Load("https://avidreaders.ru/s/" + usersData[userId].Replace(" ", "%20").Trim());
            var links = htmlDoc.DocumentNode.SelectNodes("//div[@class='clear books_list']//div[@class='book_name']/a");

            if (links == null)
                return "Книга не найдена";

            foreach (HtmlNode link in links)
            {
                Console.WriteLine(link.InnerText + "\n");
                if (link.InnerText.ToLower() == usersData[userId].ToLower())
                {
                    Console.WriteLine("Text found: " + link.InnerText + "\n");
                    
                    var href = link.GetAttributeValue("href", "");
                    if (href == "")
                        href = "Ссылка на книгу не найдена";
                    return href;
                }
            }
            return "Книга не найдена";
        }

        private static InlineKeyboardMarkup FindAuthor(long userId)
        {
            var web = new HtmlWeb();
            var htmlDoc = web.Load("https://avidreaders.ru/s/" + usersData[userId].Replace(" ", "%20").Trim());
            var links = htmlDoc.DocumentNode.SelectNodes("//div[@class='popular_slider']//a");

            if(links == null)
                return null;

            var buttons = new InlineKeyboardButton[links.Count][];
            int i = 0;
            foreach (var link in links)
            {
                var author = link.Element("div");
                buttons[i++] = new[] { InlineKeyboardButton.WithUrl(author.InnerText, link.GetAttributeValue("href", "")) };
            }

            return new InlineKeyboardMarkup(buttons);
        }



        private static IReplyMarkup GetButtons()
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new KeyboardButton[][]
                    {
                        new KeyboardButton[]
                        {
                            new KeyboardButton("Найти книгу"),
                            new KeyboardButton("Найти автора")
                        }
                    }
            };
        }
        

    }
}


/*
 
 
 */
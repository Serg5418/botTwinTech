using System;
using System.Configuration;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;
using System.Data;
using System.Data.SqlTypes;
using System.Data.Entity;
using System.Threading;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace bot4331
{
    class Program
    {
        /* private string ConnectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
         private SqlConnection sqlConnection = null; Выводить базы данных*/
     
        private static string token { get; set; } = "1559224676:AAFSNKPxmkCFfeXAzqUAvzOJ62vFdxKLVN0";//Telegram bot
        private static TelegramBotClient client;//Telegram bot
        static void Main (string[] args)
        {
            //Telegram bot

            client = new TelegramBotClient(token);
            client.StartReceiving();
            client.OnMessage += OnMessageHandler;
            Console.ReadLine();
            client.StopReceiving();
        }

        private static async void OnMessageHandler(object sender, MessageEventArgs e)
        {
            Ryzhkin_botEntities db = new Ryzhkin_botEntities();

            var msg = e.Message; // Ввод данных пользователя в чате
            long chatId = msg.Chat.Id; //Id  чата с пользователем
            string Names = msg.ForwardSenderName;
            if (msg.Text != null) //Если текст не пустой, он пойдет дальше
            {
                Console.WriteLine($"Пришло сообщение c текстом: {msg.Text} ");//Отпишет в консоль, что пришло от пользователя
                switch (msg.Text.ToLower())
                {
                    case "/start":
                        Users user = db.Users.FirstOrDefault(u => u.idChat == chatId);
                        if (user == null)
                        {
                            db.Users.Add(new Users()
                            {
                                idChat = msg.Chat.Id,
                                username = msg.Chat.Username

                            });
                            UsersDate userdate = db.UsersDate.FirstOrDefault();
                            UsersDate userdates = db.UsersDate.FirstOrDefault(info => info.Name == userdate.Name);
                            if (userdates !=null )
                            {
                                await client.SendTextMessageAsync(msg.Chat.Id, "Оставим");
                                

                            }

                            db.SaveChanges();
                            await client.SendTextMessageAsync(msg.Chat.Id, "Вы были занесены в базу данных под именнем " + msg.Chat.Username, replyMarkup: GetButtons());
                        }
                        await client.SendTextMessageAsync(msg.Chat.Id, "Привет " + msg.Chat.Username, replyMarkup: GetButtons());
                        break;
                    case "оставить заявку":
                        await client.SendTextMessageAsync(msg.Chat.Id, "Оставим", replyMarkup: GetButtons());
                        break;
                    //Если пишет "Оставить заявку", то бот ему отвечает "Оставим"
                    case "список услуг":
                        await client.SendTextMessageAsync(msg.Chat.Id, "Да, список", replyMarkup: GetButtons());
                        break;
                    //Если пишет "Список услуг", то бот ему отвечает "Да, список"
                    case "мои заявки":
                        await client.SendTextMessageAsync(msg.Chat.Id, "Твои", replyMarkup: GetButtons());
                        break;
                    //Если пишет "Мои заявки", то бот ему отвечает "Твои"

                    case "о компании":
                        //
                        InformationTalbes baseInformationTalbe = db.InformationTalbes.FirstOrDefault();
                        InformationTablesValues baseInformationTalbeValue = db.InformationTablesValues.FirstOrDefault(info=>info.idTable == baseInformationTalbe.id);

                        await client.SendTextMessageAsync(msg.Chat.Id, baseInformationTalbeValue.description
                        , replyMarkup: GetInformation());

                        /*List<InformationTablesValues> baseInformationTablesValue = db.InformationTablesValues.Where(info => info.name == info in db.UsersDate).ToList();
                        await client.SendTextMessageAsync(msg.Chat.Id, baseInformationTalbeValue.description);*/

                        InformationTablesValues InformationTablesValue = db.InformationTablesValues.FirstOrDefault(w => w.name == msg.Text);
                        if (InformationTablesValue )
                        {

                        }
                        break;

                    default: // Если пользователь ввел что-то другое, то он забивает в базу данных
                        Task<bool> isDialog = GetDialog(msg);
                        
                        if (isDialog.Result == false)
                        {
                            await client.SendTextMessageAsync(msg.Chat.Id, "Ошибка. Нет команды", replyMarkup: GetButtons());
                        }
                        break;
                        //Если пользователь написал что-то другое, то ему выскакивает панелька с выбором

                }


            }
        }
        private async static Task<bool>  GetDialog(Message message )//Панелька под именнем GetInformation, в которую можно вписывать данные
        {
            Ryzhkin_botEntities db = new Ryzhkin_botEntities();

            word_users wordUser = db.word_users.FirstOrDefault(w => w.text == message.Text);

            if (wordUser == null) return false;
          
                Dialog_Info dialog_ = db.Dialog_Info.FirstOrDefault(dialog => dialog.Id_word_users == wordUser.Id);

            if (dialog_ == null)
            {
                return false;
            }
            
            word_bot wordBot = db.word_bot.FirstOrDefault(w => w.Id == dialog_.Id_word_bot);
            string textBot = wordBot.text;
            List<KeyboardButton> keyboards = new List<KeyboardButton>();
          
            
            if (textBot.Contains("{Projects}")) //Проекты
            {
                string Projects = "\n\n";



                List<InformationTablesValues> informationTalbes = db.InformationTablesValues.Where(info => info.idTable == 2).ToList();
                foreach (InformationTablesValues InformationTalbe in informationTalbes)
                {
                    Projects += InformationTalbe.name + " \n\n";
                    keyboards.Add(new KeyboardButton { Text = InformationTalbe.name });
                }

                textBot = textBot.Replace("{Projects}", Projects);
                /*name InformationTablesValues = db.InformationTablesValues.First(w => w.name == message.Text);
                {

                }*/
            }




            await client.SendTextMessageAsync(message.Chat.Id, textBot, replyMarkup: GetCustomKeyboard(keyboards));
            return true;
        }

        private static IReplyMarkup GetCustomKeyboard(List<KeyboardButton> keyboards)//Панелька под именнем GetInformation, в которую можно вписывать данные
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                    // new List<KeyboardButton>{new KeyboardButton {Text= "Подробнее рассказать о компании" } }
                  keyboards

                }
            };
        }

        private static IReplyMarkup GetInformation()//Панелька под именнем GetInformation, в которую можно вписывать данные
        {
            Ryzhkin_botEntities db = new Ryzhkin_botEntities();
            List<KeyboardButton> keyboards = new List<KeyboardButton>();

            List<InformationTalbes> baseInformationTalbes = db.InformationTalbes.Where(info=>info.id != 1).ToList();
            
            foreach (InformationTalbes baseInformationTalbe in baseInformationTalbes)
            {
                keyboards.Add(new KeyboardButton { Text = baseInformationTalbe.name});
            }

            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                    // new List<KeyboardButton>{new KeyboardButton {Text= "Подробнее рассказать о компании" } }
                  keyboards

                }
            };
        }
        private static IReplyMarkup GetButtons()//Панелька под именнем GetButtons, в которую можно вписывать данные
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                    new List<KeyboardButton>{ new KeyboardButton { Text = "/start" } },
                    new List<KeyboardButton>{ new KeyboardButton { Text = "Оставить заявку"}, new KeyboardButton {Text= "Список услуг" } },
                    new List<KeyboardButton>{ new KeyboardButton { Text = "Мои заявки"}, new KeyboardButton {Text= "О компании" } }
                   
                }
            };
        }
    }
}

 


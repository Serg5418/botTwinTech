using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace bot4331
{
    class Program
    {
        private static string token { get; set; } = "1559224676:AAFSNKPxmkCFfeXAzqUAvzOJ62vFdxKLVN0";//Telegram bot
        private static TelegramBotClient client;//Telegram bot
        static void Main(string[] args)
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
                Console.WriteLine($"Пришло сообщение c текстом: {msg.Text} \nОт пользователя: {msg.Chat.Username}" );//Отпишет в консоль, что пришло от пользователя
                switch (msg.Text.ToLower())
                {
                    case "/start":
                        Users user = db.Users.FirstOrDefault(u => u.idChat == chatId);
                         if (msg.Chat.Username == null)
                         {
                             await client.SendTextMessageAsync(msg.Chat.Id, "Пожалуйста, заполните графу никнейма, иначе менеджер не сможет связаться с вами.", replyMarkup: GetButtons());
                         }
                         else if (user == null)
                         {
                             db.Users.Add(new Users()
                             {
                                 idChat = msg.Chat.Id,
                                 username = msg.Chat.Username,
                                 FirstName = msg.Chat.FirstName
                             });

                             db.SaveChanges();
                             await client.SendTextMessageAsync(msg.Chat.Id, "Вы были занесены в базу данных под именнем " + msg.Chat.Username );
                         }
                         if (msg.Chat.Username != null)
                         {
                             await client.SendTextMessageAsync(msg.Chat.Id, "Привет " + msg.Chat.Username, replyMarkup: GetButtons());
                         }
                        await client.SendTextMessageAsync(msg.Chat.Id, "Выберите интересующую кнопку.", replyMarkup: InformationClientbuttons());
                        break;
                    case "оставить заявку":
                        await client.SendTextMessageAsync(msg.Chat.Id, "Выберите интересующую вас услугу.", replyMarkup: GetInfoTables());
                        break;
                    case "список услуг":
                        word_bot wordBot = db.word_bot.FirstOrDefault(w => w.Id == 2);
                        string textBot = wordBot.text;
                        if (textBot.Contains("{Projects}")) //Проекты
                        {
                            string Projects = "\n\n";
                            List<Application> Applications = db.Application.Where(s => s.idTable == 2).ToList();
                            foreach (Application Application in Applications)
                            {
                                Projects += Application.applications + " \n";
                            }
                            Projects += "\n";
                            textBot = textBot.Replace("{Projects}", Projects);
                        }
                        await client.SendTextMessageAsync(msg.Chat.Id, textBot, replyMarkup: GetButtons());
                        break;
                    case "мои заявки":
                        usersApplications usera = db.usersApplications.FirstOrDefault(u => u.name == msg.Chat.Username);
                        usersApplications idUser = db.usersApplications.FirstOrDefault(u => u.idChat == msg.Chat.Id);
                        Users USERS = db.Users.FirstOrDefault(u => u.username == msg.Chat.Username);
                        Users USERS12 = db.Users.FirstOrDefault(u => u.idChat == msg.Chat.Id);
                        usersApplications usersApplication = db.usersApplications.FirstOrDefault(u => u.applications == msg.Chat.Username);
                        if (idUser !=null )
                        {
                            string projects = null;
                            List<usersApplications> informationUser = db.usersApplications.Where(info => info.idChat == msg.Chat.Id).ToList();
                            foreach (usersApplications InformationTalbe in informationUser)
                            {
                                projects += "Услуга: " + InformationTalbe.applications + "\nСтатус: " + InformationTalbe.condition + "\n\n";
                            }
                            await client.SendTextMessageAsync(msg.Chat.Id, projects, replyMarkup: GetButtons());
                        }
                        else if (USERS==null && USERS12 == null)
                        {
                            await client.SendTextMessageAsync(msg.Chat.Id, "Вы не зарегистрированы, нажмите /start", replyMarkup: GetButtons());
                        }
                        else if (USERS != null || USERS12!= null && usersApplication == null)
                        {
                            await client.SendTextMessageAsync(msg.Chat.Id, "Вы не оставляли заявки. Ознакомьтесь с нашими услугами и проектами, а затем оформите заявку.", replyMarkup: GetButtons());
                        }
                        break;
                    case "о компании":
                        InformationTalbes baseInformationTalbe = db.InformationTalbes.FirstOrDefault();
                        InformationTablesValues baseInformationTalbeValue = db.InformationTablesValues.FirstOrDefault(info => info.idTable == baseInformationTalbe.id);

                        await client.SendTextMessageAsync(msg.Chat.Id, "КТО МЫ?\n"+
                        "Наша IT компания оказывает различные услуги для бизнеса, начиная от разработки ПО, заканчивая обзвоном, администрированием и сопровождением создаваемых проектов.\n"+

                        "\nНАША МИССИЯ"+
                        "\nСодействовать бизнесу в развитии стабильных, дружелюбных и "+
                        "доверительных отношений с их клиентами."+
                        "\n\nНАШИ ЦЕННОСТИ"+
                        "\nПрофессионализм 🏅"+
                        "\nПартнерство 🤝"+
                        "\nОткрытость🙆‍♂"+
                        "\nСмелость 🚀"
                        , replyMarkup: GetInformation());  //Выводит кнопки проектов "ООО Мадеос"


                        break;
                    case "выход":
                        await client.SendTextMessageAsync(msg.Chat.Id, "Вы перешли на главное меню.", replyMarkup: GetButtons());
                        break;
                    default: // Если пользователь ввел что-то другое, то он забивает в базу данных
                        Task<bool> isDialog = GetApplication(msg);
                        Task<bool> isDialog_1 = GetDialog(msg);
                        Task<bool> isDialog_2 = GetInfoClient(msg);
                        if (isDialog_2.Result == false && isDialog_1.Result == false &&  isDialog.Result == false)
                        {
                            await client.SendTextMessageAsync(msg.Chat.Id, "Ошибка. Нет команды.", replyMarkup: GetButtons());
                        }
                            break;
                }
            }
        }


        private async static Task<bool> GetInfoClient(Message message)
        {
            Ryzhkin_botEntities db = new Ryzhkin_botEntities();
            InformationClient informationclient_1 = db.InformationClient.FirstOrDefault(w => w.text == message.Text && w.id == 1);
            string textBot1 = null;
            var msg = message;
            long chatId = msg.Chat.Id;
            Users users = db.Users.FirstOrDefault(w => w.idChat == msg.Chat.Id);
            if (informationclient_1 == null && (GetThreeProject(message) == false) && (GetSixProject(message) == false) ) return false;
            if (users==null)
            {
                textBot1 = "Вы ещё не зарегистрированы. Нажмите /start";
                await client.SendTextMessageAsync(message.Chat.Id, textBot1, replyMarkup: GetButtons());
            }
            else if(users !=null)
            {
                if (GetSixProject(message)==true)
                {
                    Users user = db.Users.FirstOrDefault(w => w.idChat == message.Chat.Id);
                    if (user.number.Contains("+7"))
                    {
                        await client.SendTextMessageAsync(message.Chat.Id, user.number);
                    }
                    else
                    {
                        await client.SendTextMessageAsync(message.Chat.Id, "Вы ещё не ввели номер телефона.");
                    }
                }
                if (informationclient_1 != null)
                {
                    textBot1 = "Введите ваш номер телефона. Начинайте с +7";
                    await client.SendTextMessageAsync(message.Chat.Id, textBot1);
                }

                if (GetThreeProject(message) == true)
                {

                    textBot1 = "Вы были занесены в базу данных под номером: " + message.Text;
                    await client.SendTextMessageAsync(message.Chat.Id, textBot1);
                    //List<KeyboardButton> keyboards_1 = new List<KeyboardButton>();
                    //string Projects = "";
                    //List<YesOrNo> yesOrNos = db.YesOrNo.Where(s => s.id != 0).ToList();
                    //foreach (YesOrNo Application in yesOrNos)
                    //{
                    //    Projects += Application.Text + " \n";
                    //    keyboards_1.Add(new KeyboardButton { Text = Application.Text });
                    //}
                    //textBot1 = textBot1.Replace("{Projects}", Projects);
                    Users user = db.Users.FirstOrDefault(u => u.idChat == msg.Chat.Id);//Сама ссылка на пользователя, но куда её добавить

                    user.number = msg.Text;
                    db.SaveChanges();

                    //await client.SendTextMessageAsync(message.Chat.Id, "\nВсе верно?", replyMarkup: GetCustomKeyboard(keyboards_1));
                }
            }
            return true;
        }


            private async static Task<bool> GetApplication(Message message)
        {
            Ryzhkin_botEntities db = new Ryzhkin_botEntities();
            word_users wordUser = db.word_users.FirstOrDefault(w => w.text == message.Text && w.Id == 2);
            if (wordUser == null) 
            {
                if (GetTwoProject(message) == true) 
                    wordUser = db.word_users.FirstOrDefault(w => w.text.Contains("{oneProject}")); 

                if (wordUser == null) return false;
            }
            Dialog_Info dialog_1 = db.Dialog_Info.FirstOrDefault(dialog => dialog.Id_word_users == wordUser.Id);

            if (dialog_1 == null)
            {
                return false;
            }

            word_bot wordBot1 = db.word_bot.FirstOrDefault(w => w.Id == dialog_1.Id_word_bot);
            string textBot1 = wordBot1.text;
            List<KeyboardButton> keyboards_1 = new List<KeyboardButton>();
            
            if (GetTwoProject(message) == true)
            {
                var msg= message;
                long chatId = msg.Chat.Id;
                string Projects12 = null;
                Users user = db.Users.FirstOrDefault(u => u.idChat == chatId);
                if (user != null)
                {
                    Users user1 = db.Users.FirstOrDefault(w => w.idChat == msg.Chat.Id);
                    //var hello = user1.number;
                    string hello = Convert.ToString(user1.number);
                    //usersApplications user2 = db.usersApplications.FirstOrDefault(w => w.number == user1.number);
                    db.usersApplications.Add(new usersApplications()
                    {
                        name = msg.Chat.Username,
                        applications = message.Text,
                        idChat = (int?)chatId,
                        condition = "Ожидает подтверждения.",
                        number = user1.number,
                        FirstName = message.Chat.FirstName
                    }) ;
                    db.SaveChanges();
                    Projects12 = "Вы записаны на услугу "+ message.Text.ToLower()+ ", наш менеджер свяжется с вами  в течение дня.";
                    textBot1 = textBot1.Replace("{descriptionProject}", Projects12);
                }
                else
                {
                    Projects12 = "Вы ещё не зарегистрировались.\nНажмите на /start";

                    textBot1 = textBot1.Replace("{descriptionProject}", Projects12);
                }
            }
            if (textBot1.Contains("{Projects}")) //Проекты
            {
                string Projects = "\n\n";
                List<Application> Applications = db.Application.Where(s => s.idTable == 2).ToList();
                InformationTalbes informationTalbesa = db.InformationTalbes.FirstOrDefault(info => info.id == 4);
                foreach (Application Application in Applications)
                {
                    Projects += Application.applications + " \n";
                    keyboards_1.Add(new KeyboardButton { Text = Application.applications }); 
                }

                keyboards_1.Add(new KeyboardButton { Text = informationTalbesa.name });
                textBot1 = textBot1.Replace("{Projects}", Projects);
            }
            await client.SendTextMessageAsync(message.Chat.Id, textBot1, replyMarkup: GetCustomKeyboard(keyboards_1));
            return true;
        }
        private async static Task<bool> GetDialog(Message message)
        {
            Ryzhkin_botEntities db = new Ryzhkin_botEntities();

            word_users WordUserIndex = db.word_users.FirstOrDefault(w => w.Id == 1);
            word_users wordUser = db.word_users.FirstOrDefault(w => w.text == message.Text && w.text == WordUserIndex.text); 

            if (wordUser == null)
            {
                if (GetOneProject(message) == true ) //если GetOneProject True идет далььше
                    wordUser = db.word_users.FirstOrDefault(w => w.text.Contains("{oneProject}")); 

                if (wordUser == null) return false;
            }

            Dialog_Info dialog_ = db.Dialog_Info.FirstOrDefault(dialog => dialog.Id_word_users == wordUser.Id);

            if (dialog_ == null)
            {
                return false;
            }

            word_bot wordBot = db.word_bot.FirstOrDefault(w => w.Id == dialog_.Id_word_bot);
            string textBot = wordBot.text;
            List<KeyboardButton> keyboards = new List<KeyboardButton>();
            if (textBot.Contains("{descriptionProject}")) 
            {
                string messageNameTables = message.Text;//Ссылается на message GetOneProject
                string nameProjects = null;
               InformationTablesValues informationTalbes = db.InformationTablesValues.FirstOrDefault(info => info.name == messageNameTables);
                nameProjects = informationTalbes.description;
                textBot = textBot.Replace("{descriptionProject}", nameProjects);
            }

            if (textBot.Contains("{Projects}")) //Проекты
            {
                string Projects = "\n\n";
                List<InformationTablesValues> informationTalbes = db.InformationTablesValues.Where(info => info.idTable == 2).ToList();
                InformationTalbes informationTalbesa = db.InformationTalbes.FirstOrDefault(info => info.id == 4);

                foreach (InformationTablesValues InformationTalbe in informationTalbes)
                {
                    Projects += InformationTalbe.name + " \n";
                    keyboards.Add(new KeyboardButton { Text = InformationTalbe.name}); //Добавляет в переменную Informatio
                }

                keyboards.Add(new KeyboardButton { Text = informationTalbesa.name });
                textBot = textBot.Replace("{Projects}", Projects);
            }
            await client.SendTextMessageAsync(message.Chat.Id, textBot, replyMarkup: GetCustomKeyboard(keyboards));
            return true;
        }
        private static bool GetOneProject(Message message)
        {
            Ryzhkin_botEntities db = new Ryzhkin_botEntities();
            var currentProject = db.InformationTablesValues.FirstOrDefault(info => info.idTable == 2 && info.name == message.Text);
           
            if (currentProject != null) return true;
            return false;
        }
        private static bool GetTwoProject(Message message)
        {
            Ryzhkin_botEntities db = new Ryzhkin_botEntities();
            var currentService = db.Application.FirstOrDefault(info =>info.applications == message.Text);
            if (currentService != null) return true;
            return false;
        }
        private static bool GetThreeProject(Message message)
        {
            if (message.Text.Contains("+7"))  return true;
            return false;
        }
        private static bool GetSixProject(Message message)
        {
            Ryzhkin_botEntities db = new Ryzhkin_botEntities();
            var info = db.InformationClient.FirstOrDefault(w => w.text == message.Text && w.id == 2);
            if (info != null) return true;
            return false;
        }
        private static IReplyMarkup GetCustomKeyboard(List<KeyboardButton> keyboards)
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                  keyboards
                }
            };
        }
        private static IReplyMarkup GetInfoTables()
        {
            Ryzhkin_botEntities db = new Ryzhkin_botEntities();
            List<KeyboardButton> keyboards_1 = new List<KeyboardButton>();

            List<InformationTalbes> baseInformationTalbes_1 = db.InformationTalbes.Where(info => info.id != 1 && info.id!=2).ToList();

            foreach (InformationTalbes baseInformationTalbe_1 in baseInformationTalbes_1)
            {
                keyboards_1.Add(new KeyboardButton { Text = baseInformationTalbe_1.name });
            }

            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                  keyboards_1
                }
            };
        }
        private static IReplyMarkup InformationClientbuttons()
        {
            Ryzhkin_botEntities db = new Ryzhkin_botEntities();
            List<KeyboardButton> keyboards_1 = new List<KeyboardButton>();

            List<InformationClient> InformationClient_1 = db.InformationClient.Where(info => info.id !=0).ToList();

            foreach (InformationClient baseInformationClient_1 in InformationClient_1)
            {
                keyboards_1.Add(new KeyboardButton { Text = baseInformationClient_1.text });
            }

            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                  keyboards_1
                }
            };
        }
        private static IReplyMarkup GetInformation()//Панелька под именнем GetInformation, в которую можно вписывать данные
        {
            Ryzhkin_botEntities db = new Ryzhkin_botEntities();
            List<KeyboardButton> keyboards = new List<KeyboardButton>();

            List<InformationTalbes> baseInformationTalbes = db.InformationTalbes.Where(info => info.id != 1 && info.id !=3).ToList();

            foreach (InformationTalbes baseInformationTalbe in baseInformationTalbes)
            {
                keyboards.Add(new KeyboardButton { Text = baseInformationTalbe.name });
            }

            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                {
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




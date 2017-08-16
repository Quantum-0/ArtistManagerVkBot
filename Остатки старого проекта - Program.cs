using Citrina;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace TestVKAPINuGet
{
    enum UserState : int
    {
        Hello,
        Default,
        DontReply,
        NotUnderstand,
        PriceList,
        Training
    }

    enum MessageDir
    {
        Received = 0,
        To = 0,
        Sended = 1,
        From = 1
    }

    class Program
    {
        // лог:
        // команда обработана
        // сообщение отправлено
        // сообщение пришло
        // ошибка отправки, полученя
        // варнинг - ошибка обработки команды

        // добавлять фразы регулярками
        // ответы - string.replace $msg$ и $name$
        // сделать гуй - в гуе выводить отдельно диалоги с пользователями, возможность включать/выключать, загрузить/сохранить базу данных, просмотрл неотвеченных сообщений, лог обучения и т.п.
        // сделать херню которая будет следить чтоб методы отправлялись не слишком часто

        const int SkidID = 87324758;
        static VkClient<UserState> client;
        static Random rnd;
        static bool OFF = false;
        static Dictionary<string, List<string>> CustomAnswers = new Dictionary<string, List<string>>();
        static Dictionary<int, int> AddedPhrases = new Dictionary<int, int>();
        static List<string> NotAnswered = new List<string>();

        static void SaveCustomAnswers(string fname)
        {
            using (StreamWriter sw = new StreamWriter(fname))
            {
                foreach (var ca in CustomAnswers)
                {
                    sw.WriteLine(ca.Key + '|' + string.Join("|", ca.Value));
                }
            }
        }

        static void LoadCustomAnswers(string fname)
        {
            if (!File.Exists(fname))
                return;

            using (StreamReader sr = new StreamReader(fname))
            {
                while(!sr.EndOfStream)
                {
                    var line = sr.ReadLine().Split('|');
                    CustomAnswers.Add(line.First(), line.Skip(1).ToList());
                }
            }
        }

        static void SaveUsersStates(string fname)
        {
            using (FileStream fs = new FileStream(fname, FileMode.Create))
            {
                client.SaveUserStates(fs);
            }
        }

        static void LoadUsersStates(string fname)
        {
            if (!File.Exists(fname))
                return;
            using (FileStream fs = new FileStream(fname, FileMode.Open))
            {
                client.LoadUserStates(fs);
            }
        }

        static void Main(string[] args)
        {
            FileStream logtext = new FileStream("text.txt", FileMode.Append, FileAccess.Write, FileShare.Read);
            LoadCustomAnswers("CustomAnswersDatabase.txt");
            rnd = new Random();
            client = new VkClient<UserState>(SkidAT);
            LoadUsersStates("UsersStates.txt");
            client.OffState = UserState.DontReply;
            client.MessageReceived += (s, e) => Log(e.Message, MessageDir.Received);
            client.MessageReceived += (s, e) => { logtext.Write(Encoding.Default.GetBytes(e.Message.Text + "\r\n"), 0, 2+Encoding.Default.GetByteCount(e.Message.Text)); logtext.Flush(); };
            client.MessageSended += (s, e) => Log(e.Message, MessageDir.Sended);
            client.UpdateMessagesError += Client_UpdateMessagesError;
            client.MessageReceived += Client_MessageReceived;
            client.StartReceiving();
            Console.ReadLine();
            SaveUsersStates("UsersStates.txt");
            SaveCustomAnswers("CustomAnswersDatabase.txt");
            logtext.Close();
            logtext.Dispose();
        }

        private static void Client_UpdateMessagesError(object sender, EventArgs e)
        {
            Console.WriteLine("Update messages error");
        }
        
        private static long GetObjectSize(object o)
        {
            long size = 0;
            using (Stream s = new MemoryStream())
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                formatter.Serialize(s, o);
                size = s.Length;
            }
            return size;
        }

        private static void Log(VkMessage<UserState> message, MessageDir direction)
        {
            var width = Console.BufferWidth - 22;
            Console.WriteLine("id{0,-9} | {1,4} | {2,-" + width + "}", message.User, (direction == MessageDir.Received) ? "FROM" : " TO ", message.Text.Replace("\n", ". ").Substring(0, Math.Min(message.Text.Length, width)));
        }

        private static void Client_MessageReceived(object sender, MessageEventArgs<UserState> e)
        {
            var msg = e.Message.Text.ToLower();

            if (OFF)
            {
                if (msg == "вкл")
                {
                    if (e.Message.User == SkidID)
                        OFF = false;
                    e.Message.Reply("Бот включён");
                }
                return;
            }

            if (e.Message.UserState == UserState.DontReply)
            {
                if (msg == "вкл")
                {
                    client.SetUserState(e.Message.User, UserState.Default);
                    e.Message.Reply("Бот включён");
                }
                return;
            }

            if (e.Message.UserState == UserState.Training)
            {
                if (msg == "отмена")
                {
                    e.Message.Reply("Режим обучения отключён");
                    client.SetUserState(e.Message.User, UserState.Default);
                    return;
                }

                if (msg == "размер")
                {
                    long memory, patterns, answers;
                    memory = GetObjectSize(CustomAnswers);
                    patterns = CustomAnswers.Keys.Count;
                    answers = CustomAnswers.Sum(p => p.Value.Count);
                    e.Message.Reply($"Текущий размер базы данных:\n\nЗанятая память: {memory / 1024f}Kb\nКоличество фраз: {patterns}\nКоличество ответов: {answers}");
                }

                if (e.Message.Text.Contains("|"))
                {
                    e.Message.Reply("Использование символа вертикальной черты запрещено");
                    return;
                }

                var strings = e.Message.Text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                if (strings.Length < 2)
                {
                    e.Message.Reply("Неверных формат входных данных для обучения.\nЗнаешь, у меня есть подозрения что один из нас тупенький. И это не я\n\nЕсли ты хочешь выйти из режима обучения, напиши ОТМЕНА");
                    return;
                }

                var pattern = strings.First().Trim();
                if (pattern.ToUpper() != pattern)
                {
                    e.Message.Reply("Шаблон должен быть в верхнем регистре");
                    return;
                }
                pattern = pattern.ToLower();

                List<string> existed = new List<string>();
                if (CustomAnswers.ContainsKey(pattern))
                {
                    foreach (var ss in strings.Skip(1).Select(s => s.Trim()).ToArray())
                    {
                        if (CustomAnswers[pattern].Select(s => s.ToLower()).Contains(ss.ToLower()))
                            existed.Add(ss);
                        else
                            CustomAnswers[pattern].Add(ss);
                    }

                    if (existed.Count > 0)
                        e.Message.Reply($"К ответам на фразу \"{pattern}\" добавлено {strings.Length - 1 - existed.Count} ответов.\nТекущее количество ответов на фразу = {CustomAnswers[pattern].Count}\n\nСледующие ответы не были добавлены, т.к. уже существуют:\n" + string.Join(", ", existed));
                    else
                        e.Message.Reply($"Спасибо, теперь я умею отвечать на фразу \"{pattern}\"\nНа данную фразу у меня уже {CustomAnswers[pattern].Count} ответов!");
                }
                else
                {
                    CustomAnswers[pattern] = strings.Skip(1).Select(s => s.Trim()).ToList();
                    e.Message.Reply($"Спасибо, теперь я умею отвечать на фразу \"{pattern}\"");
                }

                if (!AddedPhrases.ContainsKey(e.Message.User))
                    AddedPhrases[e.Message.User] = strings.Length - 1 - existed.Count;
                else
                    AddedPhrases[e.Message.User] += strings.Length - 1 - existed.Count;

                return;
            }

            if (e.Message.UserState == UserState.PriceList)
            {
                int index;
                var pricelist = client.PriceList.ToArray();
                if (!int.TryParse(msg, out index))
                {
                    var items = pricelist.Select(p => p.Name.ToLower()).ToList();
                    items.Add("весь прайс");
                    items.Add("дополнительная информация");
                    items.Add("отмена");
                    if (!items.Contains(msg))
                    {
                        client.SendMessage(e.Message.User, "Прости, я не такой умненький и не смог тебя понять.\nНапиши номер пункта или его название, чтоб получить информацию.\nЛибо напиши ОТМЕНА чтоб выйти из режима получения данных о прайсе о3о");
                        return;
                    }
                    index = items.IndexOf(msg) + 1;
                }

                index--;
                
                if (index < pricelist.Length)
                {
                    string answer;
                    answer = '[' + pricelist[index].Name + "]\n\nСтоимость: " + pricelist[index].Price +
                        "\nФормат: " + pricelist[index].Format +
                        "\nРазмер: " + pricelist[index].Size +
                        "\nЗа дополнительного персонажа: " + pricelist[index].AddChar +
                        "\n\nПримеры:";
                    client.SendMessage(e.Message.User, answer, pricelist[index].Examples);
                }
                else
                {
                    index -= pricelist.Length;
                    if (index == 0)
                    {
                        var answer = string.Join("\n", pricelist.Select(p => p.Name + " - " + p.Price).ToArray());
                        client.SendMessage(e.Message.User, answer);
                    }
                    if (index == 1)
                    {
                        client.SendMessage(e.Message.User, client.AdditionalInfoAboutCommissions);
                    }
                    if (index == 2)
                    {
                        client.SetUserState(e.Message.User, UserState.Default);
                        client.MarkAsRead(e.Message);
                    }
                }
                return;
            }

            if (e.Message.UserState == UserState.Default || e.Message.UserState == UserState.Hello || e.Message.UserState == UserState.NotUnderstand)
            {
                if (msg == "выкл")
                {
                    if (e.Message.User == SkidID)
                    {
                        e.Message.Reply("Бот отключен. Если хочешь чтоб бот снова отвечал пользователям, напиши ВКЛ");
                        OFF = true;
                        return;
                    }
                    client.SetUserState(e.Message.User, UserState.DontReply);
                    return;
                }
                
                if (Regex.IsMatch(msg, @"(\W|^)помощь(\W|$)"))
                {
                    client.SetUserState(e.Message.User, UserState.Default);
                    e.Message.Reply("ПРАЙС - прайс лист художника\n" + // сделяль
                                    //"АРТ-СТАТУС - текущий арт-статус\n" +
                                    "ПРАВИЛА - правила данной группы\n" + // сделяль
                                    "ПРИМЕРЫ - примеры артов\n" + // сделяль
                                    "РЕФ/РЕФКИ - персонаж(и) художника\n" + // сделяль
                                    "ФА - ссылка на FurAffinity\n" + // сделяль
                                    "ДА - ссылка на DeviantArt\n" + // сделаль
                                    "УЧИТЬ - режим обучения бота\n" + 
                                    "\nБОТ - информация о боте"); // сделяль
                    return;
                }

                if (msg == "бот")
                {
                    client.SetUserState(e.Message.User, UserState.Default);
                    e.Message.Reply("Твелв-бот\nБот разработан для группы https://vk.com/skid_gallery \nХудожник - https://vk.com/id" + SkidID + " (Skid)\nСоздатель бота - https://vk.com/id20108853 (Тш)\nЕсли хотите себе такого же бота в группу, можете написать мне :з (https://vk.com/id20108853)");
                    return;
                }

                if (msg == "учить")
                {
                    client.SetUserState(e.Message.User, UserState.Training);
                    e.Message.Reply("Добро пожаловать в режим обучения бота.\nВ данном режиме вы можете научить меня отвечать на различные сообщения.\nЧтобы добавить фразы, которыми я буду отвечать на определённое сообщение, пришлите мне сообщение в виде: ШАБЛОН *перенос строки* ОТВЕТ. Ответов можно добавлять несколько, разделяя их так же переносом строки.\nПример:\n\nЗДРАВСТВУЙ\nПриветик\nДобрый день\nХей! Как делишки?\n\nТаким образом бот будет отвечать на сообщение с текстом \"здравствуй\" одной из трёх фраз.\nБлижайшее время будет добавлена возможность вводить шаблоны в виде регулярных выражений.\nДля выхода из режима обучения пожалуйста напишите ОТМЕНА\n\nНадеюсь Вы научите меня правильно отвечать. Пожалуйста, не вставляйте различные ссылки/рекламу в ответы, или же сообщения не по теме. Злоупотребление данной функцией может привести к вашей блокировке. <3");
                    return;
                }

                if (Regex.IsMatch(msg, @"(\W|^)арт-?статус(\W|$)") || ((Regex.IsMatch(msg, @"(\W|^)открыты?(\W|$)") || Regex.IsMatch(msg, @"(\W|^)заказать(\W|$)")) &&
                    Regex.IsMatch(msg, @"(\W|^)(заказы?|комм?иш\w*|реквесты?|коллаб\w*)(\W|$)")))
                {
                    client.SetUserState(e.Message.User, UserState.Default);
                    e.Message.Reply("Извините, данная функция ещё не реализована");
                    return;
                }

                if (Regex.IsMatch(msg, @"(\W|^)прайс(\W|$)"))
                {
                    client.SetUserState(e.Message.User, UserState.PriceList);
                    //e.Message.Reply("Извините, данная функция ещё не реализована");
                    //e.Message.Reply("Нахуй иди, сам блять смотри прайс\nЯ тут не на тупые вопросы отвечать пришёл\nВ обсуждения группы самостоятельно зайти можешь, не маленький");
                    if (client.PriceList.Count == 0)
                        client.UpdatePrice();
                    var prices = client.PriceList.Select((p, i) => (i + 1).ToString() + ") " + p.Name).ToArray();
                    client.SendMessage(e.Message.User, "Выберите, что вас интересует:\n" + string.Join("\n", prices) + "\n" + (prices.Length + 1) + ") Весь прайс\n" + (prices.Length + 2) + ") Дополнительная информация\n" + (prices.Length + 3) + ") Отмена");
                    return;
                }

                if (Regex.IsMatch(msg, @"(\W|^)примеры?(\W|$)"))
                {
                    client.SetUserState(e.Message.User, UserState.Default);
                    var img = client.GetRandomPictureFromAlbum(239672159);
                    client.SendMessage(e.Message.User, "", new List<PhotosPhoto>(new [] { img }));
                    return;
                }

                if (Regex.IsMatch(msg, @"(\W|^)(fa|фа)(\W|$)"))
                {
                    client.SetUserState(e.Message.User, UserState.Default);
                    client.SetUserState(e.Message.User, UserState.Default);
                    e.Message.Reply(SkidFA);
                    return;
                }

                if (Regex.IsMatch(msg, @"(\W|^)(da|да)(\W|$)"))
                {
                    client.SetUserState(e.Message.User, UserState.Default);
                    client.SetUserState(e.Message.User, UserState.Default);
                    e.Message.Reply(SkidDA);
                    return;
                }

                if (Regex.IsMatch(msg, @"(\W|^)((тебя|тебе) нарисов(ать|ую)|нарисов(ать|ую) (тебя|тебе))(\W|$)"))
                {
                    client.SetUserState(e.Message.User, UserState.Default);
                    var photos = client.GetPhotosFromAlbum(214672056);
                    client.SendMessage(e.Message.User, "Нарисовать? Меня? Конечно. Вот мой реф:", photos.Take(1));
                    return;
                }

                if (Regex.IsMatch(msg, @"(\W|^)(рефы|рефки|персы|персонажи)(\W|$)"))
                {
                    client.SetUserState(e.Message.User, UserState.Default);
                    var photos = client.GetPhotosFromAlbum(214672056);
                    client.SendMessage(e.Message.User, "", photos);
                    return;
                }

                if (Regex.IsMatch(msg, @"(\W|^)(реф.?|перс|персонаж)(\W|$)"))
                {
                    client.SetUserState(e.Message.User, UserState.Default);
                    var photos = client.GetPhotosFromAlbum(214672056);
                    client.SendMessage(e.Message.User, "", photos.Take(1));
                    return;
                }

                if (Regex.IsMatch(msg, @"(\W|^)(с?пасиб(о|ки)?|благодар(ю|ствую))(\W|$)"))
                {
                    client.SetUserState(e.Message.User, UserState.Default);
                    var r = rnd.NextDouble();
                    if (r < 0.3)
                        e.Message.Reply("Пжалста :з");
                    else if (r < 0.6)
                        e.Message.Reply("Не за что");
                    else if(r < 0.9)
                        e.Message.Reply("Обращайся~");
                    else
                        e.Message.Reply("Не стоит благодарности ^-^");
                    return;
                }

                /*if (Regex.IsMatch(msg, @"(\W|^)(пидо?р|мразь|тварь)"))
                {
                    client.SetUserState(e.Message.User, UserState.Default);
                    e.Message.Reply("Сам такой :р");
                    return;
                }*/

                if (Regex.IsMatch(msg, @"(\W|^)правила?(\W|$)"))
                {
                    client.SetUserState(e.Message.User, UserState.Default);
                    var rules = client.GetRules();
                    if (string.IsNullOrWhiteSpace(rules))
                        e.Message.Reply("Не удалось получить правила группы");
                    else
                        e.Message.Reply(rules);

                    return;
                }

                /*if (Regex.IsMatch(msg, @"(\W|^)привет(\W|$)"))
                {
                    client.SetUserState(e.Message.User, UserState.Default);
                    e.Message.Reply("Ну здравствуй, здравствуй");
                    return;
                }*/

                if (e.Message.UserState == UserState.Hello)
                {
                    if (e.Message.User == SkidID)
                    {
                        e.Message.Reply("Ну дратуй, скидка хЪ Короч ВЫКЛ - выключить бота (для всех), ПОМОЩЬ - список команд");
                        client.SetUserState(e.Message.User, UserState.Default);
                        return;
                    }
                    e.Message.Reply("Здравствуйте. В данном сообществе на сообщения отвечает бот. Если не хотите получать сообщения от бота, напишите \"ВЫКЛ\". Чтобы получить список возможностей бота, напишите \"ПОМОЩЬ\"\nЕсли у вас есть предложения о том, какие функции необходимо добавить боту, пишите пожалуйста мне сюда - https://vk.com/id20108853");
                    client.SetUserState(e.Message.User, UserState.Default);
                }
                else if (e.Message.UserState == UserState.Default || e.Message.UserState == UserState.NotUnderstand)
                {
                    if (CustomAnswers.ContainsKey(msg) || CustomAnswers.ContainsKey(msg + '?') || CustomAnswers.ContainsKey(msg.Trim(new[] { ' ', '.', '?', '!' })))
                    {
                        var answers = CustomAnswers.ContainsKey(msg) ? CustomAnswers[msg] :
                            (CustomAnswers.ContainsKey(msg + '?') ? CustomAnswers[msg+'?'] : CustomAnswers[msg.Trim(new[] { ' ', '.', '?', '!' })]);
                        var i = rnd.Next(answers.Count);
                        e.Message.Reply(answers[i]);
                    }
                    else
                    {
                        // поменять фразу, добавить "так же вы можете просто поболтать со мной"
                        if (e.Message.UserState != UserState.NotUnderstand)
                        {
                            e.Message.Reply("Хей, погодь!\n Смотри, список моих возможностей ты можешь узнать, написав ПОМОЩЬ.Если тебе от меня больше ничего не будет нужно, просто напиши ВЫКЛ и мы прекратим нашу \"увлекательную\" беседу.");
                            client.SetUserState(e.Message.User, UserState.NotUnderstand);
                        }
                        NotAnswered.Add(e.Message.Text);
                    }
                    //e.Message.Reply("Извините, я вас не поняль. Список возможностей бота вы можете узнать, написав ПОМОЩЬ. Если же вы не хотите получать ответы от бота, просто напишите ВЫКЛ");
                }
            }
        }
    }
}




/*var vk = new VkNet.VkApi();
           var authparams = new VkNet.ApiAuthParams() { AccessToken = AT };
           vk.Authorize(authparams);

           var group = vk.Utils.ResolveScreenName("habr");

           var test = vk.Messages.GetDialogs(new VkNet.Model.RequestParams.MessagesDialogsGetParams() { Count = 100 });

           var test2 = vk.Messages.GetDialogs(new VkNet.Model.RequestParams.MessagesDialogsGetParams() { Count = 100, Unread = true });*/

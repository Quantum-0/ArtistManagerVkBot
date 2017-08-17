using BaseForBotExtension;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Learning
{
    public class Learning : BotExtension
    {
        private Random rnd = new Random();

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

        public override string Name => "Learning";

        public override string Description => "Возможность обучать бота";

        public override Priority Priority => Priority.Lowest;

        public override bool StopAfterProcessed => false;

        public override Version Version => new Version(1, 1);

        public override void Load()
        {
            Load(ConfigFile);
            Tab.listBox1.Items.AddRange(CustomAnswers.Keys.Cast<object>().ToArray());
            Tab.listBoxNotAnswered.Items.AddRange(NotAnswered.Cast<object>().ToArray());
        }

        public Learning()
        {
            Tab = new LearningForm(this);
        }

        public override ProcessResult ProcessMessage(int userid, string text)
        {
            // Обработка сообщений с обучением
            if (Trainers.Contains(userid))
                return ProcessMessageFromTrainer(userid, text);
            // Обработка команды начала обучения
            else if (text.ToLower() == "учить")
            {
                Trainers.Add(userid);
                CallUsersGridUpdate(new UsersGridUpdateEventArgs("LearningNow", userid, "Yes"));
                vk.SendMessage(userid, "Добро пожаловать в режим обучения бота.\n" +
                    "В данном режиме вы можете научить меня отвечать на различные сообщения.\n" +
                    "Чтобы добавить фразы, которыми я буду отвечать на определённое сообщение, " +
                    "пришлите мне сообщение в виде: ШАБЛОН *перенос строки* ОТВЕТ. " +
                    "Ответов можно добавлять несколько, разделяя их так же переносом строки.\n" +
                    "Пример:\n\nЗДРАВСТВУЙ\nПриветик\nДобрый день\nХей! Как делишки?\n\n" + 
                    "Таким образом бот будет отвечать на сообщение с текстом \"здравствуй\" одной из трёх фраз.\n" +
                    "Так же вы можете дать ответ на одну из фраз, которую писали боту, на которую он не смог ответить сам," +
                    "тем самым так же пополнив его базу данных. Для получения случайной фразы, на которую нужно дать ответ, напишите ОТВЕТ" +
                    "Ближайшее время будет добавлена возможность вводить шаблоны в виде регулярных выражений.\n" +
                    "Для выхода из режима обучения пожалуйста напишите ОТМЕНА\n\n" +
                    "Надеюсь Вы научите меня правильно отвечать. Пожалуйста, не вставляйте различные ссылки/рекламу в ответы," +
                    "или же сообщения не по теме. Злоупотребление данной функцией может привести к вашей блокировке. <3");
                return ProcessResult.Processed;
            }
            // Ответ на фразу, ответ на которую существует
            else if (CustomAnswers.ContainsKey(text.ToLower()) || CustomAnswers.ContainsKey(text.ToLower() + '?') || CustomAnswers.ContainsKey(text.ToLower().Trim(new[] { ' ', '.', '?', '!' })))
            {
                text = text.ToLower();
                var answers = CustomAnswers.ContainsKey(text) ? CustomAnswers[text] :
                            (CustomAnswers.ContainsKey(text + '?') ? CustomAnswers[text + '?'] : CustomAnswers[text.Trim(new[] { ' ', '.', '?', '!' })]);
                var i = rnd.Next(answers.Count);
                vk.SendMessage(userid, answers[i]);
                return ProcessResult.Processed;
            }
            // Добавление фразы в неотвеченные
            NotAnswered.Add(text);
            Tab.Invoke((Action)(() => Tab.listBoxNotAnswered.Items.Add(text)));
            return ProcessResult.Skipped;
        }

        private ProcessResult ProcessMessageFromTrainer(int userid, string text)
        {
            // Обработка команд отмены и размера
            if (text == "отмена")
            {
                vk.SendMessage(userid, "Режим обучения отключён");
                CallUsersGridUpdate(new UsersGridUpdateEventArgs("LearningNow", userid, "No"));
                Trainers.Remove(userid);
                WaitAnswersFor.Remove(userid);
                return ProcessResult.Processed;
            }
            if (text == "размер")
            {
                long memory, patterns, answers;
                memory = GetObjectSize(CustomAnswers);
                patterns = CustomAnswers.Keys.Count;
                answers = CustomAnswers.Sum(p => p.Value.Count);
                vk.SendMessage(userid, $"Текущий размер базы данных:\n\nЗанятая память: {memory / 1024f}Kb\nКоличество фраз: {patterns}\nКоличество ответов: {answers}");
                return ProcessResult.Processed;
            }
            if (text.StartsWith("неотвечен") || text.StartsWith("не отвечен"))
            {
                if (NotAnswered.Count < 10)
                    vk.SendMessage(userid, string.Join("\r\n", NotAnswered));
                else
                    vk.SendMessage(userid, string.Join("\r\n", NotAnswered.Take(10)) + "\r\nИ ещё " + (NotAnswered.Count - 10));
            }
            if (text == "ответ")
            {
                if (NotAnswered.Count == 0)
                {
                    vk.SendMessage(userid, "К сожалению, на данный момент нет неотвеченных фраз. Но вы можете добавить свою ;)");
                    return ProcessResult.Processed;
                }

                var phrase = NotAnswered.First(); // FIXME
                WaitAnswersFor[userid] = phrase;
                vk.SendMessage(userid, phrase);
                return ProcessResult.Processed;
            }

            // Разделение сообщения на строки
            var strings = text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            
            // Добавление ответов на существующую фразу
            if (WaitAnswersFor.ContainsKey(userid))
            {
                var pattern = WaitAnswersFor[userid];
                NotAnswered.Remove(pattern);
                Tab.Invoke((Action)(() => Tab.listBoxNotAnswered.Items.Remove(pattern)));
                AddAnswer(pattern, strings, userid);
                return ProcessResult.Processed;
            }

            // Проверка на ошибки ввода
            if (!CheckCorrect(strings, userid))
                return ProcessResult.Processed;

            // Добавление фразы
            return AddAnswer(strings.First(), strings.Skip(1), userid);
        }

        private bool CheckCorrect(string[] strings, int userid)
        {
            if (strings.Any(s => s.Contains("|")))
            {
                vk.SendMessage(userid, "Использование символа вертикальной черты запрещено");
                return false;
            }

            if (strings.Length < 2)
            {
                vk.SendMessage(userid, "Неверных формат входных данных для обучения.\nЗнаешь, у меня есть подозрения что один из нас тупенький. И это не я\n\nЕсли ты хочешь выйти из режима обучения, напиши ОТМЕНА");
                return false;
            }

            var pattern = strings.First().Trim();
            if (pattern.ToUpper() != pattern)
            {
                vk.SendMessage(userid, "Шаблон должен быть в верхнем регистре");
                return false;
            }

            return true;
        }

        private ProcessResult AddAnswer(string pattern, IEnumerable<string> answers, int userid)
        {
            pattern = pattern.ToLower();

            List<string> existed = new List<string>();
            if (CustomAnswers.ContainsKey(pattern))
            {
                foreach (var ss in answers.Select(s => s.Trim()).ToArray())
                {
                    if (CustomAnswers[pattern].Select(s => s.ToLower()).Contains(ss.ToLower()))
                        existed.Add(ss);
                    else
                        CustomAnswers[pattern].Add(ss);
                }

                if (existed.Count > 0)
                    vk.SendMessage(userid, $"К ответам на фразу \"{pattern}\" добавлено {answers.Count() - existed.Count} ответов.\nТекущее количество ответов на фразу = {CustomAnswers[pattern].Count}\n\nСледующие ответы не были добавлены, т.к. уже существуют:\n" + string.Join(", ", existed));
                else
                    vk.SendMessage(userid, $"Спасибо, теперь я умею отвечать на фразу \"{pattern}\"\nНа данную фразу у меня уже {CustomAnswers[pattern].Count} ответов!");                

                return ProcessResult.Processed;
            }
            else
            {
                Tab.Invoke((Action)(() => Tab.listBox1.Items.Add(pattern)));
                CustomAnswers[pattern] = answers.Select(s => s.Trim()).ToList();
                vk.SendMessage(userid, $"Спасибо, теперь я умею отвечать на фразу \"{pattern}\"");
                return ProcessResult.Processed;
            }
        }

        public override void Save()
        {
            Save(ConfigFile);
        }

        private List<int> Trainers = new List<int>();
        private Dictionary<int, string> WaitAnswersFor = new Dictionary<int, string>();
        internal Dictionary<string, List<string>> CustomAnswers = new Dictionary<string, List<string>>();
        internal List<string> NotAnswered = new List<string>();

        public override IEnumerable<CustomColumn> GetUsersGridColumns() => new[] { LearningNowCol };
        CustomColumn LearningNowCol = new CustomColumn() { Name = "LearningNow", Header = "Learning now", DefaultValue = "No" };

        public override IEnumerable<Form> GetCustomTabs() => new[] { Tab };

        private LearningForm Tab;

        private void Save(string fname)
        {
            using (StreamWriter sw = new StreamWriter(fname))
            {
                foreach (var ca in CustomAnswers)
                {
                    sw.WriteLine(ca.Key + '|' + string.Join("|", ca.Value));
                }

                sw.WriteLine("==========");

                foreach (var na in NotAnswered)
                {
                    sw.WriteLine(na.Replace("\n", ". "));
                }
            }
        }

        private void Load(string fname)
        {
            if (!File.Exists(fname))
                return;

            using (StreamReader sr = new StreamReader(fname))
            {
                string line;

                while(true)
                {
                    line = sr.ReadLine();
                    if (line == "==========")
                        break;
                    var parts = line.Split('|');
                    CustomAnswers.Add(parts.First(), parts.Skip(1).ToList());
                }
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    NotAnswered.Add(line);
                }
            }
        }
    }
}

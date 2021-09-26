using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using XlsToJsonConverter;
using System.IO;
using Moq;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Data;

namespace XlsToJsonConverter.Tests
{

    /// <summary>
    /// Тесты преобразования DataTable в коллекцию json сообщений с помощью BarsXlsToJsonConverter
    /// </summary>
    public class BarsTableToJsonConverterTests
    {
        // Студенты, которые должны находиться в бд
        private List<StudentInfo> studentList = new List<StudentInfo> {
                new StudentInfo { Id = 1, Name = "Student1", Group = "А-07-17", Institute ="Inst1", Email ="test1@test.com",
                Vk= "221756370", CreatedAt = new DateTime(2008, 5, 1, 8, 30, 52) },

                new StudentInfo { Id = 2, Name = "Student2", Group = "А-07-17", Institute ="Inst1", Email ="test2@test.com",
                Telegram = "4567865656", CreatedAt = new DateTime(2011, 5, 4, 3, 31, 54)} };

        /// <summary>
        /// Сообщения из корректной таблицы создадут для студентов, которые есть в базе данных корректные сообщения
        /// </summary>
        [Fact]
        public void Converting_of_valid_table_is_correct()
        {
            // Подготовка 
            Mock<IRepository> mock = CreateMockIRepository();
            var sut = new BarsTableToJsonConverter(mock.Object);

            DataTable table = new TestBarsTableBuilder("2020/2021, Осенний семестр, Базы данных, экзамен", "А-07-17")
               .AddBarsUnit("КМ-1", "4", "5")
               .AddBarsUnit("КМ-2", "8", "30")
               .AddBarsUnit("КМ-3", "12", "15")
               .AddBarsUnit("КМ-4", "13", "30")
               .AddBarsUnit("КМ-5", "16", "20")
               .FormStructure()
               .AddStudent(name: "Student1", current: "4,2", semestr: "4,0", exam: "", total: "", cmMarks: new []{ "4", "3", "5", "4", "4" })
               .AddStudent(name: "Student2", current: "3,5", semestr: "", exam: "", total: "", cmMarks: new[] { "3", "4" })
               .AddStudent(name: "Student3", current: "4,5", semestr: "", exam: "", total: "", cmMarks: new[] { "4", "", "", "", "3" })
               .AddStudent(name: "Student4", current: "4,7", semestr: "", exam: "", total: "", cmMarks: new[] { "3", "5", "", "4" })
               .Build();

            // Действие
            IEnumerable<string> result = sut.Convert(table);

            // Проверка
            var resultArray = ResultToJObjectArray(result);

            Assert.Equal(2, result.Count());

            mock.Verify(x => x.AddMessageToStudent(It.IsAny<DbMessage>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
            mock.Verify(x => x.Save(), Times.Once);

            IsMessageCorrect(resultArray[0], studentList[0]);
            IsMessageCorrect(resultArray[1], studentList[1]);
        }

        /// <summary>
        /// Сообщения из таблицы без студентов не будут созданы
        /// </summary>
        [Fact]
        public void Converting_table_without_students_does_not_create_messages()
        {
            // Подготовка
            Mock<IRepository> mock = CreateMockIRepository();
            var sut = new BarsTableToJsonConverter(mock.Object);
            DataTable table = new TestBarsTableBuilder("2020/2021, Осенний семестр, Базы данных, экзамен", "А-07-17")
               .AddBarsUnit("КМ-1", "4", "5")
               .AddBarsUnit("КМ-2", "8", "30")
               .AddBarsUnit("КМ-3", "12", "15")
               .AddBarsUnit("КМ-4", "13", "30")
               .AddBarsUnit("КМ-5", "16", "20")
               .FormStructure()
               .Build();

            // Действие
            IEnumerable<string> result = sut.Convert(table);

            // Проверка
            Assert.Empty(result);
        }

        /// <summary>
        /// Сообщения для студентов, которых нет в базе данных не будут созданы
        /// </summary>
        [Fact]
        public void Converting_table_with_students_that_not_in_db_does_not_create_messages()
        {
            // Подготовка
            Mock<IRepository> mock = CreateMockIRepository();

            var sut = new BarsTableToJsonConverter(mock.Object);

            DataTable table = new TestBarsTableBuilder("2020/2021, Осенний семестр, Базы данных, экзамен", "А-09-17")
               .AddBarsUnit("КМ-1", "4", "5")
               .AddBarsUnit("КМ-2", "8", "30")
               .AddBarsUnit("КМ-3", "12", "15")
               .AddBarsUnit("КМ-4", "13", "30")
               .AddBarsUnit("КМ-5", "16", "20")
               .FormStructure()
               .AddStudent("Student1", "4,2", "4,0", "", "", "4", "3", "5", "4", "4")
               .AddStudent("Student2", "3,5", "", "", "", "3", "4")
               .AddStudent("Student3", "4,5", "", "", "", "4", "", "", "", "3")
               .AddStudent("Student4", "4,7", "", "", "", "3", "5", "", "4")
               .Build();

            // Действие
            IEnumerable<string> result = sut.Convert(table);

            // Проверка
            Assert.Empty(result);
        }

        /// <summary>
        /// Сообщения из пустой таблицы не будут созданы
        /// </summary>
        [Fact]
        public void Converting_empty_table_does_not_create_messages()
        {
            // Подготовка
            Mock<IRepository> mock = CreateMockIRepository();
            var sut = new BarsTableToJsonConverter(mock.Object);
            DataTable table = new DataTable();

            // Действие
            IEnumerable<string> result = sut.Convert(table);

            // Проверка
            Assert.Empty(result);
        }

        /// <summary>
        /// Создает и настраивает мок репозитория со студентами из studentList 
        /// </summary>
        /// <returns>Мок репозитория</returns>
        private Mock<IRepository> CreateMockIRepository()
        {
            // Экземпляр мока
            var mock = new Mock<IRepository>();

            // При вызове метода GetStudentsByGroup с аргуметом "А-07-17" должен вернутся список студентов studentList
            mock.Setup(x => x.GetStudentsByGroup("А-07-17")).Returns(studentList);

            // Вызвовы методов AddMessageToStudent и Save нужно проверить
            mock.Setup(x => x.AddMessageToStudent(It.IsAny<DbMessage>(), It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            mock.Setup(x => x.Save()).Verifiable();

            return mock;
        }

        /// <summary>
        /// Приводитколлекцию строк json сообщений к массиву JObject
        /// </summary>
        /// <param name="result">IEnumerable json строк</param>
        /// <returns>Массив JObject, полученный из коллекции json строк</returns>
        private JObject[] ResultToJObjectArray(IEnumerable<string> result)
        {
            JObject[] jObjResult = new JObject[result.Count()];

            var resultArr = result.ToArray();

            // Парсим каждую json строку
            for (int i = 0; i < jObjResult.Length; i++)
            {
                jObjResult[i] = JObject.Parse(Regex.Unescape(resultArr[i]));
            }

            return jObjResult;
        }

        /// <summary>
        /// Проверяет корректность сообщения
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="student">Студент</param>
        private void IsMessageCorrect(JObject message, StudentInfo student)
        {

            Assert.Equal("BARS MPEI", message["From"]);

            // Проверка, что сообщение предназначено одному человеку
            Assert.Single(message["To"]);

            // Проверка корректности имени и контактов
            Assert.Equal(student.Name, message["To"][0]["Name"]);
            Assert.Equal(student.Email ?? "", message["To"][0]["Email"].ToString());
            Assert.Equal(student.Phone ?? "", message["To"][0]["Phone"].ToString());
            Assert.Equal(student.Vk ?? "", message["To"][0]["Vk"].ToString());
            Assert.Equal(student.Telegram ?? "", message["To"][0]["Telegram"].ToString());

            // Проверка корректности типа сообщения и его темы
            Assert.Equal(0, message["Type"]);
            Assert.Equal("2020/2021, Осенний семестр, Базы данных, экзамен", message["Subject"]);

            // Проверка, что вложений нет
            Assert.Empty(message["Attachments"]);
        }

    }
}

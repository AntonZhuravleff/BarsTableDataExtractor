using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using Npgsql;
using NpgsqlTypes;
using Newtonsoft.Json;

namespace XlsToJsonConverter.Tests
{
    /// <summary>
    /// Проверяет корректную обработку файлов, взаимодействие с бд и Google Drive с помощью XlsNodeManager
    /// </summary>
    public class XlsNodeManagerTests : IntegrationTests
    {
        //Список тестовых студентов, которые будут добавлены в бд
        protected override List<StudentInfo> DbStudents { get; } = new List<StudentInfo>()
        {
                new StudentInfo
                {
                    Name = "Student1",
                    Group = "А-07-17",
                    Institute = "ИВТИ",
                    CreatedAt = DateTime.Now,
                    Vk = "2345546546",
                    Email = "Student1_07@mail.com",
                    Phone = "804-222-1111",
                    Telegram = "354657854",
                },

                new StudentInfo
                {
                    Name = "Student4",
                    Group = "А-07-17",
                    Institute = "ИВТИ",
                    CreatedAt = new DateTime(2020, 3, 1, 7, 0, 0),
                    Vk = "2345546546",
                    Telegram = "465767868"
                },

                new StudentInfo
                {
                    Name = "Student4",
                    Group = "А-08-17",
                    Institute = "ИВТИ",
                    CreatedAt = new DateTime(2020, 6, 30, 3, 0, 0),
                    Email = "Student4_08@mail.com"
                },

                new StudentInfo
                {
                    Name = "Student6",
                    Group = "А-08-17",
                    Institute = "ИВТИ",
                    CreatedAt = new DateTime(2007, 2, 11, 4, 0, 0),
                },

                new StudentInfo
                {
                    Name = "Student7",
                    Group = "Э-05-17",
                    Institute = "ИЭЭ",
                    CreatedAt = new DateTime(2017, 11, 11, 4, 0, 0),
                    Email = "Student7_05@mail.com"
                }
        };

        /// <summary>
        /// Обработка корректных файлов происходит корректно
        /// </summary>
        [Fact]
        public void Processing_files_in_normal_state_is_correct()
        {
            // Подготовка
            PrepareTestFiles("correct1.xlsx", "correct2.xlsx");

            Mock<IDriveService> drive = new Mock<IDriveService>();
            drive.Setup(x => x.UploadJsonFileAsync(It.IsAny<string>())).Verifiable();

            var sut = new XlsNodeManager<BarsTableToJsonConverter, EFCoreRepository>(
                drive.Object,
                new XlsFilesService(inputPath, outputPath));

            // Действие
            sut.ProcessFiles();

            // Проверка
            Assert.Empty(new DirectoryInfo(inputPath).GetFileSystemInfos());
            Assert.Equal(2, new DirectoryInfo(outputPath).GetFileSystemInfos().Length);

            drive.Verify(x => x.UploadJsonFileAsync(It.IsAny<string>()), Times.Exactly(3));

            AssertIsMessageCorrect("Student1", "А-07-17",
                "Студент: Student1:\r\nКМ-1: балл: 4, срок: 4, вес: 5\r\n" +
                "КМ-2: балл: 3, срок: 8, вес: 30\r\nКМ-3: балл: 5, срок: 12, вес: 15\r\nКМ-4: балл: 4, срок: 13, вес: 30\r\n" +
                "КМ-5: балл: 4, срок: 16, вес: 20\r\nТекущий балл: 4,2\r\nСеместрвоая составляющая: 4\r\n" +
                "Экзаминационная составляющая: не указано\r\nОценка за освоение дисциплины: не указано\r\n");

            AssertIsMessageCorrect("Student4", "А-07-17",
                "Студент: Student4:\r\nКМ-1: балл: 3, срок: 4, вес: 5\r\n" +
                "КМ-2: балл: 5, срок: 8, вес: 30\r\nКМ-3: балл: не указано, срок: 12, вес: 15\r\nКМ-4: балл: 4, срок: 13, вес: 30\r\n" +
                "КМ-5: балл: не указано, срок: 16, вес: 20\r\nТекущий балл: 4,7\r\nСеместрвоая составляющая: не указано\r\n" +
                "Экзаминационная составляющая: не указано\r\nОценка за освоение дисциплины: не указано\r\n");

            AssertIsMessageCorrect("Student4", "А-08-17",
                "Студент: Student4:\r\nКМ-1: балл: 3, срок: 4, вес: 5\r\n" +
                "КМ-2: балл: 4, срок: 8, вес: 30\r\nКМ-3: балл: не указано, срок: 12, вес: 15\r\nКМ-4: балл: не указано, срок: 13, вес: 30\r\n" +
                "КМ-5: балл: не указано, срок: 16, вес: 20\r\nТекущий балл: 3,5\r\nСеместрвоая составляющая: не указано\r\n" +
                "Экзаминационная составляющая: не указано\r\nОценка за освоение дисциплины: не указано\r\n");
        }

        /// <summary>
        /// Сообщения не добавляются в базу данных и не загружаются на Google Drive если входная папка пуста
        /// </summary>
        [Fact]
        public void Empty_input_folder_do_not_affect_on_db_and_drive()
        {
            // Подготовка
            ClearInputFolder();

            Mock<IDriveService> drive = new Mock<IDriveService>();
            drive.Setup(x => x.UploadJsonFileAsync(It.IsAny<string>())).Verifiable();

            var sut = new XlsNodeManager<BarsTableToJsonConverter, EFCoreRepository>(
                drive.Object,
                new XlsFilesService(inputPath, outputPath));

            int expectedMessageCount = context.MsgsMessage.Count();

            // Действие
            sut.ProcessFiles();

            // Проверка
            drive.Verify(x => x.UploadJsonFileAsync(It.IsAny<string>()), Times.Never);
            Assert.Equal(expectedMessageCount, context.MsgsMessage.Count());
        }

        /// <summary>
        /// Файлы с расширением отличным от "xls" и "xlsx" не обрабатываются
        /// </summary>
        [Fact]
        public void Files_with_an_invalid_extension_are_not_processed()
        {
            // Подготовка
            PrepareTestFiles("bug.txt", "bug.docx");

            Mock<IDriveService> drive = new Mock<IDriveService>();
            drive.Setup(x => x.UploadJsonFileAsync(It.IsAny<string>())).Verifiable();

            var sut = new XlsNodeManager<BarsTableToJsonConverter, EFCoreRepository>(
                drive.Object,
                new XlsFilesService(inputPath, outputPath));

            int expectedMessageCount = context.MsgsMessage.Count();

            // Действие
            sut.ProcessFiles();

            // Проверка
            drive.Verify(x => x.UploadJsonFileAsync(It.IsAny<string>()), Times.Never);
            Assert.Equal(expectedMessageCount, context.MsgsMessage.Count());
        }

        /// <summary>
        /// При наличии входной папки и отсутствии выходной, создается выходная папка
        /// </summary>
        [Fact]
        public void Output_folder_is_created_when_input_folder_is_valid()
        {
            // Подготовка
            PrepareTestFiles("correct1.xlsx", "correct2.xlsx");

            Directory.Delete(outputPath);

            string expectedPath = Directory.GetParent(inputPath).FullName + "/Output";

            Mock<IDriveService> drive = new Mock<IDriveService>();
            var sut = new XlsNodeManager<BarsTableToJsonConverter, EFCoreRepository>(
                drive.Object,
                new XlsFilesService(inputPath, "C:/bars/bug.txt"));

            // Действие
            sut.ProcessFiles();

            // Проверка
            Assert.True(Directory.Exists(expectedPath));
            Assert.Equal(2, Directory.EnumerateFiles(expectedPath).Count());
        }

        /// <summary>
        ///  Проверяет сообщение в базе данных на корректность
        /// </summary>
        /// <param name="studentName">Имя студента</param>
        /// <param name="group">Учебная группа</param>
        /// <param name="expectedMessage">Ожидаемый текст сообщения</param>
        private void AssertIsMessageCorrect(string studentName, string group, string expectedMessage)
        {
            // Берем студента из бд
            var student = context.MsgsStudentinfo.Where(x => x.Name == studentName && x.Group == group).First();

            // Проверям, что в бд у него только одно сообщение
            Assert.Single(context.MsgsMessageTo.Where(x => x.StudentinfoId == student.Id));

            // Берем из бд сообщение
            var message = context.MsgsMessage.Where(x => x.Id == context.MsgsMessageTo.Where(x => x.StudentinfoId == student.Id).First().MessageId).First();

            // Проверяем, что тип сообщения - 0
            Assert.Equal(0, (int)message.Type);

            // Проверяем валидность тела сообщения
            Assert.Equal(expectedMessage, message.Body);
        }
    }
}

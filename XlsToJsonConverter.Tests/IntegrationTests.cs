using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XlsToJsonConverter.Tests
{
    /// <summary>
    /// Базовый класс для все интеграционных тестов в проекте
    /// </summary>
    public abstract class IntegrationTests
    {
        // Экземпляр контекста
        protected MpeiMessagesContext context = new MpeiMessagesContext();

        // Путь к входной папке
        protected string inputPath { get; set; } = "C:\\bars\\Tests\\Input";

        // Путь к выходной папке
        protected string outputPath = "C:\\bars\\Tests\\Output";

        // Путь к папке с тестовыми файлами
        protected string testFilesPath = "C:\\bars\\Tests\\TestFiles";

        /// <summary>
        /// Список тестовых данных студентов для базы данных
        /// </summary>
        protected abstract List<StudentInfo> DbStudents { get; }

        public IntegrationTests()
        {
            // Удаляем тестовые данны из бд
            DeleteDbTestData();
            // Загружаем тестовые данные
            LoadDbWithTestData();
        }

        /// <summary>
        /// Загружает в базу данных тестовые данные
        /// </summary>
        protected virtual void LoadDbWithTestData()
        {
            // Создаем экземпляр контекста
            MpeiMessagesContext context1 = new MpeiMessagesContext();

            // Загружаем данные в бд
            using (var connection = (NpgsqlConnection)context1.Database.GetDbConnection())
            {
                connection.Open();

                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = connection;

                // Запрос для добавления тестового пользователя в таблицу auth_user
                string insertQuery = "INSERT INTO auth_user " +
                        "(password, is_superuser, username, first_name, last_name, email, is_staff,is_active, date_joined) " +
                        "VALUES";

                // Добавляем тестовых пользователей к запросу
                for (int i = 0; i < DbStudents.Count; i++)
                {
                    insertQuery += $"('sfrgghb3frgdretgd',FALSE,'testuser{i}','testuser{i}','testuser{i}','testuser{i}@test.com', FALSE, FALSE, NOW())";

                    if (i != DbStudents.Count - 1)
                        insertQuery += ',';
                    else
                        insertQuery += ';';
                }

                // Выполняем запрос
                command.CommandText = insertQuery;
                command.ExecuteNonQuery();

                // Запрос для получения id тестовых пользователей
                command.CommandText = "SELECT id FROM auth_user WHERE username LIKE 'testuser%'";

                // Массив для хранения id
                var ids = new int[DbStudents.Count];

                // Считываем id из бд и добавляем их в массив 
                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    int i = 0;
                    while (reader.Read())
                    {
                        ids[i] = reader.GetInt32(0);
                        i++;
                    }
                }

                // Запрос для вставки тестовых данных студентов в таблицу msgs_studentinfo
                insertQuery = "INSERT INTO msgs_studentinfo" +
                 "(name, institute, \"group\", email, phone, vk, telegram, created_at, updated_at, user_id)" +
                 "VALUES";

                // Добавляем тестовых данные студентов к запросу
                for (int i = 0; i < DbStudents.Count; i++)
                {
                    insertQuery += $"('{DbStudents[i].Name}', '{DbStudents[i].Institute}', '{DbStudents[i].Group}', '{DbStudents[i]?.Email ?? ""}', " +
                          $"'{DbStudents[i]?.Phone ?? ""}', '{DbStudents[i]?.Vk ?? ""}', '{DbStudents[i]?.Telegram ?? ""}', '{NpgsqlDateTime.Now}'," +
                          $"'{NpgsqlDateTime.Now}', {ids[i]})";

                    if (i != DbStudents.Count - 1)
                        insertQuery += ',';
                    else
                        insertQuery += ';';
                }

                // Выполняем запрос
                command.CommandText = insertQuery;
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Удаляет из базы данных тестовые данные
        /// </summary>
        protected virtual void DeleteDbTestData()
        {
            // Создаем экземпляр контекста
            MpeiMessagesContext context1 = new MpeiMessagesContext();

            // Удаляем данные из бд
            using (var connection = (NpgsqlConnection)context1.Database.GetDbConnection())
            {
                // Открываем соединение
                connection.Open();

                // Создаем команду
                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = connection;

                // Запрос для удаления тестовых данных из таблиц msgs_message, msgs_message_to, msgs_studentinfo и auth_user
                command.CommandText = "DELETE FROM msgs_message WHERE id IN" +
                    "(SELECT message_id FROM msgs_message_to WHERE studentinfo_id IN" +
                    "(SELECT id FROM msgs_studentinfo WHERE name LIKE 'Student%'));" +
                    "DELETE FROM msgs_message_to WHERE studentinfo_id IN (SELECT id FROM msgs_studentinfo WHERE name LIKE 'Student%');" +
                    "DELETE FROM msgs_studentinfo WHERE name LIKE 'Student%';" +
                    "DELETE FROM auth_user WHERE username LIKE 'testuser%';";

                // Выполняем запрос
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Перемещает тестируемые файлы во входную папку и перемещает файлы из выходной папки в папку со всеми тестовыми файлами
        /// </summary>
        /// <param name="filenames">Имена файлов для перемещения во входную папку</param>
        protected void PrepareTestFiles(params string[] filenames)
        {
            //Перемещаем файлы из выходной папки в папку с тестовыми файлами
            DirectoryInfo testDirInfo = new DirectoryInfo(testFilesPath);

            // Берем файлы из выходной папки
            IEnumerable<string> outputFiles = Directory.EnumerateFiles(outputPath);

            // Перемещаем файлы из outputFiles в папку с тестовыми файлами
            foreach (var file in outputFiles)
            {
                FileInfo fileInf = new FileInfo(file);

                // Если файла нет в тестовой папке, то перемещаем его
                if (new FileInfo(testDirInfo + "\\" + fileInf.Name).Exists == false)
                {
                    fileInf.MoveTo(testDirInfo + "\\" + fileInf.Name);
                }
            }

            DirectoryInfo inputDirInfo = new DirectoryInfo(inputPath);

            // Берем файлы указанные в filenames из тестовой папки 
            IEnumerable<string> testFiles = Directory.EnumerateFiles(testFilesPath).Where(x => filenames.Contains(Path.GetFileName(x)));

            // Перемещаем файлы из testFiles в входную папку
            foreach (var file in testFiles)
            {
                FileInfo fileInf = new FileInfo(file);

                if (new FileInfo(inputDirInfo + "\\" + fileInf.Name).Exists == false)
                {
                    fileInf.MoveTo(inputDirInfo + "\\" + fileInf.Name);
                }
            }
        }

        /// <summary>
        /// Перемещает все файлы из входной папки в папку со всеми тестовыми файлами
        /// </summary>
        protected void ClearInputFolder()
        {
            //Перемещаем файлы из входной папки в папку с тестовыми файлами
            DirectoryInfo testDirInfo = new DirectoryInfo(testFilesPath);

            // Берем файлы с расширением из выходной папки
            IEnumerable<string> outputFiles = Directory.EnumerateFiles(inputPath);

            // Перемещаем файлы из outputFiles в тестовую папку
            foreach (var file in outputFiles)
            {
                FileInfo fileInf = new FileInfo(file);

                if (new FileInfo(testDirInfo + "\\" + fileInf.Name).Exists == false)
                {
                    fileInf.MoveTo(testDirInfo + "\\" + fileInf.Name);
                }
            }
        }
    }
}

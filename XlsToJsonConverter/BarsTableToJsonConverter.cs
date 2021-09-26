using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using Npgsql;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;

namespace XlsToJsonConverter
{
    /// <summary>
    /// Преобразует таблицу выгрузки БАРСа в коллекцию json сообщений
    /// </summary>
    public class BarsTableToJsonConverter : IDataTableToJsonConverter
    {
        // Парсер
        private readonly BarsTableParser parser = new BarsTableParser();

        // Репозиторий
        private readonly IRepository repository;

        // Логгер
        public ILogger Logger { get; set; } = Log.Logger;

        public BarsTableToJsonConverter(IRepository repo)
        {
            repository = repo;
        }

        // Преобразуем xls/xlsx таблицу в коллекцию JSON сообщений
        public IEnumerable<string> Convert(DataTable table)
        {
            // Логируем исполнение метода
            Logger.LogMethodExecution(className: this.GetType().Name);

            BarsDocument document = null;

            try
            {
                // Парсим таблицу
                document = parser.Parse(table);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while parsing DataTable. Message: {e.Message}");
                return Enumerable.Empty<string>();
            }

            // Создаем из документа json сообщения
            var messages = CreateJsonMessages(document);

            Logger.Information($"{messages.Count()} json message(s) from file {table.TableName} created");

            return messages;
        }

        /// <summary>
        /// Создает json сообщения из данных BarsDocument
        /// </summary>
        /// <param name="document">Документ, из данных которого требуется создать сообщения</param>
        /// <returns>Коллекция json сообщений</returns>
        private IEnumerable<string> CreateJsonMessages(BarsDocument document)
        {
            Logger.LogMethodExecution(className: this.GetType().Name);

            var messages = new List<string>();
            // Получаем из бд студентов группы
            var stundentInfos = repository.GetStudentsByGroup(document.Group);

            // Перебираем студентов из выгрузки
            foreach (var student in document.Students)
            {
                StudentInfo recipient = null;

                // Берем студента, у которого имя совпадает с текущим
                try
                {
                    recipient = stundentInfos.FirstOrDefault(s => s.Name == student.Name);
                }
                catch (System.InvalidOperationException e)
                {
                    Logger.Error($"Error occured while taking student {student.Name}. Message: {e.Message}");
                    continue;
                }
                catch (Exception e)
                {
                    Logger.Error($"Error occured while taking student {student.Name}. Message: {e.Message}");
                    continue;
                }

                bool hasContacts = !String.IsNullOrEmpty(recipient?.Email) || !String.IsNullOrEmpty(recipient?.Telegram) ||
                    !String.IsNullOrEmpty(recipient?.Phone) || !String.IsNullOrEmpty(recipient?.Vk);

                // Если такого студента нет или не указаны его контакты, то переходим к следующему
                if (recipient == null || !hasContacts)
                    continue;

                // Создаем тело сообщения
                string body = $"Студент: {student.Name}:\r\n";

                // Добавляем информацию о КМах к сообщению
                foreach (var cm in student.CmMarks)
                {
                    body += $"{cm.Name}: балл: {HandleMark(cm.Mark.ToString())}, срок: {cm.Term}, вес: {cm.Weight}\r\n";
                }

                // Добавляем оценки к сообщению
                body += $"Текущий балл: {HandleMark(student.CurrentMark.ToString())}\r\n";

                body += $"Семестрвоая составляющая: {HandleMark(student.SemesterMark.ToString())}\r\n";

                body += $"Экзаминационная составляющая: {HandleMark(student.ExamMark.ToString())}\r\n";

                body += $"Оценка за освоение дисциплины: {HandleMark(student.TotalMark.ToString())}\r\n";


                // Создаем сообщение
                var message = new MessageToSend
                {
                    Sender = "BARS MPEI",
                    Type = MessageType.Bars,
                    Subject = document.Name,
                    Body = body,
                    CreatedAt = DateTime.Now,
                    Attachments = new List<Attachment>()
                };

                // Добавляем получателя
                message.To.Add(new Recipient
                {
                    Name = recipient.Name,
                    Email = recipient.Email,
                    Phone = recipient.Phone,
                    Vk = recipient.Vk,
                    Telegram = recipient.Telegram
                });

                // Сериализуем сообщение в JSON и добавляем его в список
                var json = JsonConverter.SerializeObject(message);

                if (json != null)
                    messages.Add(json);

                // Заполняем, для добавления сообщения в базу данных
                var dbMsg = new DbMessage(message);

                // Добавляем в базу данных
                repository.AddMessageToStudent(dbMsg, student.Name, document.Group);
            }
            // Принимаем изменения в базе данных
            try
            {
                repository.Save();
            }
            catch (NpgsqlException e)
            {
                Logger.Error($"An error occurred while inserting messages in database. Message: {e.Message}");
            }
            catch (DbException e)
            {
                Logger.Error($"An error occurred while inserting messages in database. Message: {e.Message}");
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while inserting messages in database. Message: {e.Message}");
            }

            return messages;
        }

        /// <summary>
        /// Возвращает строку 'не указано' в случае, если строка пустая или null, иначе возвращает изначальную строку
        /// </summary>
        /// <param name="mark">Оценка</param>
        /// <returns>Оценку или "не указано" если оценка пустая строка или null</returns>
        private string HandleMark(string mark)
        {
            if (string.IsNullOrEmpty(mark))
                return "не указано";
            else
                return mark;
        }

    }
}

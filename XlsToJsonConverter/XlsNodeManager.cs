using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XlsToJsonConverter
{
    /// <summary>
    /// Создает сообщения из xls/xlsx файлов и загружает их на диск
    /// </summary>
    public class XlsNodeManager<T1, T2> where T1 : IDataTableToJsonConverter where T2 : IRepository
    {
        // Диск
        private IDriveService Drive { get; set; }

        // Сервис дял работы с файловой системой
        private IXlsFilesService FileService { get; set; }

        // Логгер
        public ILogger Logger { get; set; } = Log.Logger;

        public XlsNodeManager(IDriveService drive, IXlsFilesService fileService)
        {
            this.Drive = drive;
            this.FileService = fileService;
        }

        /// <summary>
        /// Обрабатывает файлы из входной папки
        /// </summary>
        public void ProcessFiles()
        {
            // Логируем исполнение метода
            Logger.LogMethodExecution(className: this.GetType().Name);

            // Берем файлы
            IEnumerable<FileInfo> files = FileService.GetXlsFiles();

            // Если файлов нет, то выходим из функции
            if (files.Count() == 0)
            {
                Logger.Information("Input folder dosn't contain any .xls/.xlsx files");
                return;
            }

            var tasks = new List<Task>();

            // Обрабатывам каждый файл
            foreach (var file in files)
            {
                Logger.Information($"Start processing file: {file.Name}");

                // Создаем экземпляры репозитория и конвертера
                var repository = (T2)Activator.CreateInstance(typeof(T2));
                var converter = (T1)Activator.CreateInstance(typeof(T1), repository);

                tasks.Add(Task.Run(                 
                    async () =>
                    {
                        // Получаем спискок JSON сообщений из таблицы
                        var messages = converter.Convert(FileService.GetDataTable(file));

                        // Загружаем файлы на Google Drive
                        await UploadMessagesAsync(messages);

                        // Перемещаем файл в outputFolder
                        FileService.MoveFile(file);
                    }));                  
            }

            // Ждем завершения всех тасков
            Task.WaitAll(tasks.ToArray());
        }

        /// <summary>
        /// Загружает сообщения на диск
        /// </summary>
        /// <param name="messages">Коллекция JSON сообщений</param>
        /// <returns>Task</returns>
        private async Task UploadMessagesAsync(IEnumerable<string> messages)
        {
            List<Task> taskList = new List<Task>();

            foreach (var msg in messages)
            {
                taskList.Add(Drive.UploadJsonFileAsync(msg));
            }

            await Task.WhenAll(taskList);
        }
    }
}

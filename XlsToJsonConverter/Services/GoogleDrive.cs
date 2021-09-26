using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using System.IO;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Google.Apis.Upload;

namespace XlsToJsonConverter
{
    /// <summary>
    /// Класс работы с Google Drive
    /// </summary>
    public class GoogleDrive : IDriveService
    {
        // Права доступа
        private static string[] Scopes = { DriveService.Scope.Drive };
        // Имя приложения
        private static string ApplicationName = "XlsToJsonNode";

        // Сервис Google диска
        private DriveService service;

        //Логгер
        private ILogger Logger { get; set; } = Log.Logger;
        // Путь к учетным данным
        private string credentialsPath;
        // Путь к токену
        private string tokenPath;
        public GoogleDrive(string credsPath, string tokenPath)
        {
            credentialsPath = credsPath;
            this.tokenPath = tokenPath;
            Authorize();
        }

        // Авторизация
        private void Authorize()
        {
            // Логируем исполнение метода
            Logger.LogMethodExecution(className: this.GetType().Name);

            // Учетные данные пользоваетля
            UserCredential credential;

            try
            {
                using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
                {
                    // Файл token.json хранит данные юзера и токены. Создается при первой авторизации
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(tokenPath, true)).Result;

                }

                // Создание API сервиса
                service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });
            }
            // При неудаче логируем и выходим из приложения
            catch (Exception e)
            {
                Logger.Fatal($"An error occurred while authorizating in Google Drive. Message: {e.Message}");
                Log.CloseAndFlush();
                Environment.Exit(1);
            }



        }

        /// <summary>
        /// Загрузить JSON файл на диск
        /// </summary>
        /// <param name="json">JSON строка</param>
        /// <returns>Task</returns>
        public async Task UploadJsonFileAsync(string json)
        {
            Logger.LogMethodExecution(className: this.GetType().Name);

            // Имя файла на диске будет текущей датой
            string name = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK") + ".json";

            // Метаданные файла
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = name
            };

            // Запрос
            FilesResource.CreateMediaUpload request;

            try
            {
                using (var stream = new MemoryStream())
                {
                    // Записываем JSON в поток
                    var writer = new StreamWriter(stream);
                    await writer.WriteAsync(json);
                    writer.Flush();
                    stream.Position = 0;

                    // Загружаем на диск
                    request = service.Files.Create(fileMetadata, stream, "application/json");
                    request.Fields = "id";
                    var result = await request.UploadAsync();

                    // Экспоненциальный откат в случае неудачи
                    if (result.Status != UploadStatus.Completed)
                    {
                        var rdn = new Random();
                        var waitTime = 0;
                        var count = 0;
                        do
                        {
                            Logger.Information($"Failed to upload file {fileMetadata.Name}. Trying again {count + 1}  time");

                            // Время = 2^n + (случайное время до 1 сек).
                            waitTime = (Convert.ToInt32(Math.Pow(2, count)) * 1000) + rdn.Next(0, 1000);

                            // Ждем 
                            await Task.Delay(waitTime);

                            // Пытаемся загрузить 
                            result = await request.UploadAsync();
                            count++;

                        } while (count < 5 && (result.Status != UploadStatus.Completed));
                    }
                }

                // Получаем ответ
                var file = request.ResponseBody;
                Logger.Information($"Uploaded. File id: {file.Id}");

            }
            catch (Exception e)
            {
                dynamic data = JObject.Parse(json);
                Logger.Error($"Unable to upload json with subject: {data.Subject}. Message: {e.Message}");
            }
        }
    }
}

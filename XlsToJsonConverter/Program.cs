using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Newtonsoft.Json;
using Serilog;
using System;
using System.IO;
using System.Linq;

namespace XlsToJsonConverter
{
    class Program
    {
        // Путь к входной папке
        private static string inputFolderPath;

        // Путь к выходной папке
        private static string outputFolderPath;

        // Путь к credentials
        private static string credsPath;

        // Путь к токену
        private static string tokenPath;

        static void Main(string[] args)
        {
            try
            {
                // Настраиванм приложение
                ConfigureApp();
            }
            catch (Exception e)
            {
                Log.Logger.Fatal($"An error occurred while working with the configuration file. Message: {e.Message}");
                Log.CloseAndFlush();
                System.Environment.Exit(1);
            }

            Log.Logger.Information("BARS export handler node start running");

            try
            {
                // Берем пути
                Path.GetFullPath(inputFolderPath);
                Path.GetFullPath(outputFolderPath);
            }
            catch(ArgumentException e)
            {
                Log.Logger.Error($"Invalid input or output path. Message {e.Message}");
                return;
            }
            catch(Exception e)
            {
                Log.Logger.Error($"Invalid input or output path. Message {e.Message}");
                return;
            }

            if (!new DirectoryInfo(inputFolderPath).Exists||!new DirectoryInfo(outputFolderPath).Exists)
            {
                Log.Error("Input or output directory doesn't exist");
                return;
            }

            // Создаем экземпляр менеджера
            var manager = new XlsNodeManager<BarsTableToJsonConverter, EFCoreRepository>(
                new GoogleDrive(credsPath, tokenPath),
                new XlsFilesService(inputFolderPath, outputFolderPath));

            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                // Обрабатываем файлы
                manager.ProcessFiles();
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine("Execution time: " + elapsedMs);
            }
            catch (Exception e)
            {
                Log.Logger.Fatal($"Error occured while node was processing files. Message{e.Message}");
            }

            Log.Logger.Information($"BARS export handler node finished work");
            Log.CloseAndFlush();
        }

        /// <summary>
        /// Настраивает приложение
        /// </summary>
        private static void ConfigureApp()
        {
            // Настраиваем логгер
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.Http("http://127.0.0.1:9605/")
                .Enrich.WithProperty("Node", "BARS export handler")
                .Enrich.WithProperty("Application", "XlsToJsonConverter")
                .CreateLogger();

            // Подключаем файл конфигурации
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appconfig.json")
                .Build();

            // Берем пути из файла конфигурации
            inputFolderPath = configuration["ExcelInputFolder"];
            outputFolderPath = configuration["ExcelProcessedFolder"];
            credsPath = configuration["GoogleDriveCredsPath"];
            tokenPath = configuration["GoogleDriveTokenPath"];
        }

    }
}

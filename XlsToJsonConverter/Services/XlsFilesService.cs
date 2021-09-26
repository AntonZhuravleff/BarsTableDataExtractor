using ExcelDataReader;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace XlsToJsonConverter
{
    /// <summary>
    /// Класс для работы с файловой системой
    /// </summary>
    public class XlsFilesService : IXlsFilesService
    {
        // Ограничения на расширения файлов
        private readonly string[] extensions = new string[] { ".xls", ".xlsx" };

        // Входные и выходные папки
        private DirectoryInfo inputDir;
        private DirectoryInfo outputDir;

        // Логгер
        private ILogger logger = Log.Logger;

        public XlsFilesService(string inputFolderPath, string outputFolderPath)
        {
            // Создаем экземпляры DirectoryInfo из путей к входной и выходной папке
            inputDir = new DirectoryInfo(inputFolderPath);
            outputDir = new DirectoryInfo(outputFolderPath);

            // Если входная папка существует, а выходная нет - создаем выходную папку
            if (inputDir.Exists && !outputDir.Exists)
            {
                outputDir = Directory.CreateDirectory(Directory.GetParent(inputFolderPath).FullName + "/Output");
                logger.Information($"Output path doesn't exist. Folder {outputDir.Name} is created");
            }

        }

        /// <summary>
        /// Создает DataTable из xls/xlsx файла
        /// </summary>
        /// <param name="xlsFile">FileInfo xls/xlsx файла</param>
        /// <returns>DataTable, сформированную из xls/xlsx файла</returns>
        public DataTable GetDataTable(FileInfo xlsFile)
        {
            Log.Logger.LogMethodExecution(className: this.GetType().Name);

            // Если неверное расширение - выбрасываем исключние
            if (!extensions.Contains(xlsFile.Extension.ToLower()))
                throw new ArgumentException($"File must be xls or xlsx."); 

            // Читаем файл
            using (var stream = xlsFile.OpenRead())
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    // Преобразуем считанное в DataSet и берем единственную таблицу
                    var result = reader.AsDataSet();
                    return result.Tables[0];
                }
            }
        }

        /// <summary>
        /// Получает из входной папки(inputDir) ".xls", ".xlsx" файлы
        /// </summary>
        /// <returns>Коллекция содержащая FileInfo файлов с расширением ".xls", ".xlsx"</returns>
        public IEnumerable<FileInfo> GetXlsFiles()
        {
            Log.Logger.LogMethodExecution(className: this.GetType().Name);

            // Возвращаем коллекцию файлов с расширением ".xls", ".xlsx"
            return inputDir.EnumerateFiles().Where(f => extensions.Contains(f.Extension.ToLower()));
        }

        /// <summary>
        /// Перемещает файл в выходную папку
        /// </summary>
        /// <param name="file">FileInfo перемещаемого файла</param>
        public void MoveFile(FileInfo file)
        {
            Log.Logger.LogMethodExecution(className: this.GetType().Name);
            // Перемещаем файл
            File.Move(file.FullName, Path.Combine(outputDir.FullName, file.Name));
        }
    }
}

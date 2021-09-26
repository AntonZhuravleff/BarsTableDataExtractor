using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace XlsToJsonConverter
{
    /// <summary>
    /// Интерфейс работы с файловой системой
    /// </summary>
    public interface IXlsFilesService
    {
        /// <summary>
        /// Получает файлы с расширением ".xls", ".xlsx"
        /// </summary>
        /// <returns>Коллекция содержащая FileInfo файлов с расширением ".xls", ".xlsx"</returns>
        public IEnumerable<FileInfo> GetXlsFiles();

        /// <summary>
        /// Создает DataTable из xls/xlsx файла
        /// </summary>
        /// <param name="xlsFile">FileInfo xls/xlsx файла</param>
        /// <returns>DataTable, сформированную из xls/xlsx файла</returns>
        public DataTable GetDataTable(FileInfo xlsFile);

        /// <summary>
        /// Перемещает файл
        /// </summary>
        /// <param name="file">FileInfo файла</param>
        public void MoveFile(FileInfo file);
    }
}

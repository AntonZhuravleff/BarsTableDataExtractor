using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XlsToJsonConverter
{
    /// <summary>
    /// Интерфейс работы с диском
    /// </summary>
    public interface IDriveService
    {
        /// <summary>
        /// Загружает JSON файл на диск
        /// </summary>
        /// <param name="json">JSON строка</param>
        public Task UploadJsonFileAsync(string json);
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace XlsToJsonConverter
{
    /// <summary>
    /// Преобразует таблицу в коллекцию json сообщений
    /// </summary>
    public interface IDataTableToJsonConverter
    {
        /// <summary>
        /// Преобразует DataTable в коллекцию JSON сообщений
        /// </summary>
        /// <param name="table">Таблица из которой будут созданы JSON сообщения</param>
        /// <returns>Коллекция JSON сообщений</returns>
        public IEnumerable<string> Convert(DataTable table);
    }
}

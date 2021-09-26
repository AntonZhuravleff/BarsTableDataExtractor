using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace XlsToJsonConverter
{
    /// <summary>
    /// Json преобразователь
    /// </summary>
    public static class JsonConverter
    {
        /// <summary>
        /// Сериализует объект типа Т в JSON
        /// </summary>
        /// <typeparam name="T">Тип</typeparam>
        /// <param name="value">Объект</param>
        /// <returns></returns>
        public static string SerializeObject<T>(T value)
        {
            // Логируем исполнение метода
            Log.Logger.LogMethodExecution(className: typeof(JsonConverter).Name);

            // Создаем экземпляры StringBuilder и StringWriter
            StringBuilder sb = new StringBuilder(256);
            StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);

            // Создаем стандартный сериализатор
            var jsonSerializer = JsonSerializer.CreateDefault();
            using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
            {
                // Настраиваем формат для дальнейшей сериализации
                jsonWriter.StringEscapeHandling = StringEscapeHandling.EscapeNonAscii;
                jsonWriter.Formatting = Formatting.Indented;
                jsonWriter.IndentChar = ' ';
                jsonWriter.Indentation = 4;

                try
                {
                    // Сериализуем
                    jsonSerializer.Serialize(jsonWriter, value, typeof(T));
                }
                // Если не удалось сериализвать - логируем и возвращаем null
                catch(System.Runtime.Serialization.SerializationException e)
                {
                    Log.Logger.Error($"Unable to serialize object {value.GetType().Name}. Message: {e.Message}");
                    return null;
                }
                catch(Exception e)
                {
                    Log.Logger.Error($"Unable to serialize object {value.GetType().Name}. Message: {e.Message}");
                    return null;
                }
                
            }
            // Возвращаем JSON строку
            return sw.ToString();
        }
    }
}

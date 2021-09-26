using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace XlsToJsonConverter
{
    /// <summary>
    /// Класс, содержащий методы расширения для ILogger
    /// </summary>
    public static class LoggerExtensionMethods
    {
        /// <summary>
        /// Логирует имя выполняемого метода и имя класса
        /// </summary>
        /// <param name="logger">Логер</param>
        /// <param name="methodName">Имя метода</param>
        /// <param name="className">Имя класса</param>
        public static void LogMethodExecution(this ILogger logger, [CallerMemberName] string methodName = "", string className = "")
        {
            // Строка лога
            string messageString;

            //Если не указано имя класса, то лоигруем только имя метода, иначе имя класса и имя метода
            if (string.IsNullOrEmpty(className))
                messageString = $"Executing {methodName}";
            else
                messageString = $"Executing {className}.{methodName}";

            // Логируем
            logger.Information(messageString);
        }
    }
}

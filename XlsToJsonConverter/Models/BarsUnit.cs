using System;
using System.Collections.Generic;
using System.Text;

namespace XlsToJsonConverter
{
    /// <summary>
    /// Модель контрольного меропириятия
    /// </summary>
    public class BarsUnit
    {
        // Название 
        public string Name { get; set; }
        // Срок
        public int Term { get; set; }
        // Вес
        public int Weight { get; set; }
        // Оценка
        public int? Mark { get; set; }
    }
}

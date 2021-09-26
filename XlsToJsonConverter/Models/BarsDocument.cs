using System;
using System.Collections.Generic;
using System.Text;

namespace XlsToJsonConverter
{
    /// <summary>
    /// Модель данных из выгрузки из БАРСА
    /// </summary>
    public class BarsDocument
    {
        // Название
        public string Name {get;set;} 
        // Группа
        public string Group { get; set; }
        // Список данных студентов
        public List<BarsStudent> Students { get; set; } = new List<BarsStudent>();
    }

}

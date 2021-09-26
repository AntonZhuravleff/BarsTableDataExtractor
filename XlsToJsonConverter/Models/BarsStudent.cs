using System.Collections.Generic;
using System.Diagnostics;

namespace XlsToJsonConverter
{
    /// <summary>
    ///  Данные студентов из выгрузки БАРСа
    /// </summary>
    public class BarsStudent
    {
        // ФИО студента
        public string Name { get; }
        // Оценки за контрольные меропрития
        public IList<BarsUnit> CmMarks { get; } = new List<BarsUnit>();
        // Текущий балл
        public float? CurrentMark { get; set; }
        // Семестрвоая составляющая
        public float? SemesterMark { get; set; }
        // Экзаменационная составляющая
        public float? ExamMark { get; set; }
        // Оценка за освоение дисциплины
        public int? TotalMark { get; set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="name">Имя студента</param>
        public BarsStudent(string name)
        {
            Name = name;
        }
    }
}
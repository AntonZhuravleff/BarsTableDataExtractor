using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace XlsToJsonConverter
{
    /// <summary>
    /// Предоставляет информацию о выгрузке из БАРСа
    /// </summary>
    public class BarsTableInfo
    {
        // Индексы рядов и колонн ячеек таблицы
        // Название таблицы
        protected readonly int NameRow = 0;
        protected readonly int NameCol = 0;
        // Группа
        protected readonly int GroupRow = 5;
        protected readonly int GroupCol = 0;
        // Надпись "Контрольные мероприятия"
        protected readonly int CMRow = 1;
        protected readonly int CMCol = 2;
        // Надписи "Текущий балл", "Семетровая составляющая" и т.д.
        protected readonly int MarkRow = 1;
        // Начало списка студентов
        protected readonly int StudentRow = 6;
        protected readonly int StudentCol = 1;
        // Название контрольного мероприятия
        protected readonly int CMNameRow = 2;
        // Срок контрольного мероприятия
        protected readonly int CMTermRow = 3;
        // Вес контрольного мероприятия
        protected readonly int CMWeightRow = 4;

        //  Индексы, идущие относительно кол-ва контрольных мероприятий (нужно складывать с кол-вом контрольных мероприятий)
        protected readonly int CurrentMarkRelCol = 2;
        protected readonly int SemesterMarkRelCol = 3;
        protected readonly int ExamMarkRelCol = 4;
        protected readonly int TotalMarkRelCol = 5;

        /// <summary>
        /// Возвращает число контрольных мероприятий из таблицы выгрузки БАРСа
        /// </summary>
        /// <param name="table">DataTable выгрузки из БАРСа</param>
        /// <returns>Число контрольных мероприятий</returns>
        protected int GetCMCount(DataTable table)
        {
            // Логируем исполнение метода
            Log.Logger.LogMethodExecution(className: this.GetType().Name);
        
            // Номер колонны с надписью "Контрольные мероприятия"
            int i = CMCol;
            // Сюда будем записывать кол-во контрольных мероприятий
            int num = 0;

            // Пока не дойдем до колонны "Текущий балл", увеличиваем счетчик контрольных мероприятий и индекс колонны.
            while (!String.Equals(table.Rows[CMRow][i].ToString(), "Текущий балл", StringComparison.CurrentCultureIgnoreCase))
            {
                num++;
                i++;
            }
            return num;
        }

    }
}

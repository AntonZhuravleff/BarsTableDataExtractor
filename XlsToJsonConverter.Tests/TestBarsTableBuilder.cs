using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace XlsToJsonConverter.Tests
{
    /// <summary>
    /// Строитель тестовых таблиц DataTable выгрузок БАРСа
    /// </summary>
    public class TestBarsTableBuilder : BarsTableInfo
    {
        // Таблица выгрузки из БАРСа
        private DataTable barsTable;

        // Текущий индекс столбца для добавления КМов
        private int currentCMCol;

        // Текущий индекс строки дял добавления студентов
        private int currentStudentRow;

        // Кол-во контрольных мероприятий
        private int cmCount;

        // Сформирована ли структура таблицы?
        private bool isFormed = false;

        // Начальное кол-во строк
        private readonly int seedRows = 6;

        // Начальное кол-во столбцов
        private readonly int seedColumns = 11;

        public TestBarsTableBuilder()
        {
            // Инициализируем поля
            currentCMCol = CMCol;
            currentStudentRow = StudentRow;
            barsTable = new DataTable();

            // Добавляем сторки и столбцы в таблицу
            AddRows(seedRows);
            AddColumns(seedColumns);
        }

        public TestBarsTableBuilder(string name, string group)
        {
            // Инициализируем поля
            currentCMCol = CMCol;
            currentStudentRow = StudentRow;
            barsTable = new DataTable();

            // Добавляем сторки и столбцы в таблицу
            AddRows(seedRows);
            AddColumns(seedColumns);

            // Записываем в таблицу назавние и учебную группы
            SetName(name);
            SetGroup(group);

            // Добавляем надпись "Контрольные мероприятия"
            barsTable.Rows[CMRow][CMCol] = "Контрольные мероприятия";
        }

        /// <summary>
        /// Записывает в таблицу ее название
        /// </summary>
        /// <param name="name">Название таблицы</param>
        /// <returns>TestBarsTableBuilder, позволяющий настраивать таблицу дальше</returns>
        public TestBarsTableBuilder SetName(string name)
        {
            barsTable.Rows[NameRow][NameCol] = name;
            return this;
        }

        /// <summary>
        /// Записывает в таблицу учебную группу
        /// </summary>
        /// <param name="group">Учебная группа</param>
        /// <returns>TestBarsTableBuilder, позволяющий настраивать таблицу дальше</returns>
        public TestBarsTableBuilder SetGroup(string group)
        {
            barsTable.Rows[GroupRow][GroupCol] = group;
            return this;
        }

        /// <summary>
        ///  Добавляет в таблицу контрольное мероприятие
        /// </summary>
        /// <param name="name">Название КМа</param>
        /// <param name="term">Срок</param>
        /// <param name="weight">Вес</param>
        /// <returns>TestBarsTableBuilder, позволяющий настраивать таблицу дальше</returns>
        public TestBarsTableBuilder AddBarsUnit(string name, string term, string weight)
        {
            // Если структура таблицы сформирована - выбрасываем исключение
            if (isFormed)
                throw new Exception("Cannot add BarsUnit after the table structure is formed");

            // Если в таблице не хватает столбцов для КМа - добавляем столбец
            if (barsTable.Columns.Count <= currentCMCol)
                barsTable.Columns.Add();

            // Записываем данные КМа
            barsTable.Rows[CMNameRow][currentCMCol] = name;
            barsTable.Rows[CMTermRow][currentCMCol] = term;
            barsTable.Rows[CMWeightRow][currentCMCol] = weight;

            // Инкрементируем индекс столбца КМов
            currentCMCol++;

            return this;
        }

        /// <summary>
        /// Заканчивает формирование структуры таблицы
        /// </summary>
        /// <returns>TestBarsTableBuilder, позволяющий настраивать таблицу дальше</returns>
        public TestBarsTableBuilder FormStructure()
        {
            // Если структура таблицы уже сформированна - выбрасываем исключение
            if (isFormed)
                throw new Exception("Table structure is already formed");

            // Сколько столбцов будет добавлено
            int addedCols = 4;

            // Если столбцов не хватает - добавляем
            if (currentCMCol + addedCols >= barsTable.Columns.Count)
                AddColumns(addedCols);

            // Вычисляем кол-во КМов
            cmCount = currentCMCol - CMCol;

            // Записываем название столбцов для оценок
            barsTable.Rows[MarkRow][cmCount + CurrentMarkRelCol] = "Текущий балл";
            barsTable.Rows[MarkRow][cmCount + ExamMarkRelCol] = "Экзаменационная составляющая";
            barsTable.Rows[MarkRow][cmCount + SemesterMarkRelCol] = "Семестровая составляющая";
            barsTable.Rows[MarkRow][cmCount + TotalMarkRelCol] = "Оценка за освоение дисциплины";

            // Теперь таблица считается сформированной
            isFormed = true;

            return this;
        }

        /// <summary>
        /// Добавляет студента с его баллами в таблицу
        /// </summary>
        /// <param name="name">Имя студента</param>
        /// <param name="current">Текущий балл</param>
        /// <param name="semestr">Семестрвоая составляющая</param>
        /// <param name="exam">Экзаменационная составляющая</param>
        /// <param name="total">Оценка за осваение дисциплины</param>
        /// <param name="cmMarks">Оценки за КМы</param>
        /// <returns>TestBarsTableBuilder, позволяющий настраивать таблицу дальше</returns>
        public TestBarsTableBuilder AddStudent(string name, string current, string semestr, string exam, string total, params string[] cmMarks)
        {
            // Если структура таблицы уже сформированна - выбрасываем исключение
            if (!isFormed)
                throw new Exception("Cannot add students until table structure is formed");

            // Если кол-во оценок за КМы больше кол-ва Кмов - выбрасываем исключение
            if (cmMarks.Length > cmCount)
                throw new ArgumentException("cmMarks count is more than BarsUnit count");

            // Если не хватает рядов для добавления студента - добавляем строку
            if (barsTable.Rows.Count <= currentStudentRow)
                barsTable.Rows.Add();

            // Записывем имя студента
            barsTable.Rows[currentStudentRow][StudentCol] = name;

            // Записываем оценки за КМы
            for (int i = 0; i < cmMarks.Length; i++)
                barsTable.Rows[currentStudentRow][i + CMCol] = cmMarks[i];

            // Записываем оценки студента
            barsTable.Rows[currentStudentRow][cmCount + CurrentMarkRelCol] = current;
            barsTable.Rows[currentStudentRow][cmCount + SemesterMarkRelCol] = semestr;
            barsTable.Rows[currentStudentRow][cmCount + ExamMarkRelCol] = exam;
            barsTable.Rows[currentStudentRow][cmCount + TotalMarkRelCol] = total;

            // Инкрементируем индекс строки студентов
            currentStudentRow++;

            return this;
        }

        /// <summary>
        /// Строит DataTable выгрузки БАРСа
        /// </summary>
        /// <returns>DataTable выгрузки БАРСа</returns>
        public DataTable Build() => barsTable;

        /// <summary>
        /// Добавляет указанное кол-во строк к таблице
        /// </summary>
        /// <param name="amount">Кол-во строк, которые нужно добавить</param>
        private void AddRows(int amount)
        {
            for (int i = 0; i < amount; i++)
                barsTable.Rows.Add();
        }

        /// <summary>
        /// Добавляет указанное кол-во столбцов к таблице
        /// </summary>
        /// <param name="amount">Кол-во столбцов, которые нужно добавить</param>
        private void AddColumns(int amount)
        {
            for (int i = 0; i < amount; i++)
                barsTable.Columns.Add();
        }
    }


}

using ExcelDataReader;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace XlsToJsonConverter
{
    /// <summary>
    /// Парсер для DataTable выгрузки БАРСа
    /// </summary>
    public class BarsTableParser : BarsTableInfo
    {
        // Логгер
        public ILogger Logger { get; set; } = Log.Logger;

        /// <summary>
        /// Извлекает данные из выгрузки БАРСа
        /// </summary>
        /// <param name="table">Выгрузка БАРСа</param>
        /// <returns>BarsDocument с данными из выгрузки БАРСа</returns>
        public BarsDocument Parse(DataTable table)
        {
            // Логируем исполнение метода
            Logger.LogMethodExecution(className: this.GetType().Name);

            // Создаем документ
            BarsDocument doc = new BarsDocument();

            // Извлекаем имя и учебную группу
            doc.Name = table.Rows[NameRow][NameCol].ToString();
            doc.Group = table.Rows[GroupRow][GroupCol].ToString();

            // Вычисляем кол-во контрольных мероприятий
            int cmCount = GetCMCount(table);

            // Перебираем студентов
            for (int i = StudentRow; i < table.Rows.Count; i++)
            {
                // Берем ФИО студента
                var name = table.Rows[i][StudentCol].ToString();
                // Удаляем '(В)' в ФИО 
                name = Regex.Replace(name, @"( *\(В\))", "");

                float? currentMark;
                float? semesterMark;
                float? examMark;
                int? totalMark;

                // Извлекаем баллы
                TryParseMark(table.Rows[i][cmCount + CurrentMarkRelCol].ToString(), out currentMark);
                TryParseMark(table.Rows[i][cmCount + SemesterMarkRelCol].ToString(), out semesterMark);
                TryParseMark(table.Rows[i][cmCount + ExamMarkRelCol].ToString(), out examMark);
                TryParseMark(table.Rows[i][cmCount + TotalMarkRelCol].ToString(), out totalMark);

                // Создаем студента
                var student = new BarsStudent(name)
                {
                    CurrentMark = currentMark,
                    SemesterMark = semesterMark,
                    ExamMark = examMark,
                    TotalMark = totalMark
                };

                // Перебирем контрольные мероприятия и оценки по ним 
                for (int j = CMCol; j < cmCount + CMCol; j++)
                {
                    // Берем данные о контрольном мероприятии
                    BarsUnit unit = new BarsUnit();
                    unit.Name = table.Rows[CMNameRow][j].ToString();

                    try
                    {
                        // Извлекаем срок и вес КМа
                        unit.Term = Convert.ToInt32(table.Rows[CMTermRow][j].ToString());
                        unit.Weight = Convert.ToInt32(table.Rows[CMWeightRow][j].ToString());
                    }
                    catch (FormatException e)
                    {
                        Logger.Error($"Format exeption. Cannot convert unit's term or weight to Int32. Message: {e.Message}");
                    }
                    catch(Exception e)
                    {
                        Logger.Error($"Cannot convert unit's term or weight to Int32. Message: {e.Message}");
                    }
                    
                    // Извлекаем оценку за КМ
                    int? mark;
                    TryParseMark(table.Rows[i][j].ToString(), out mark);

                    unit.Mark = mark;

                    // Добавляем КМ в список КМов студента
                    student.CmMarks.Add(unit);
                }

                // Добавляем студента в документ
                doc.Students.Add(student);

            }
            return doc;
        }

        /// <summary>
        /// Конвертирует строку в nullable float. Может конвертировать числа с точкой или запятой в качестве разделителя
        /// </summary>
        /// <param name="num">Число в строковом представлении</param>
        /// <param name="result">Результат преобразования</param>
        /// <returns>true если пребразование успешно. false если результат null</returns>
        private bool TryParseMark(string num, out float? result)
        {
            // Культура
            var cultureInfo = CultureInfo.InvariantCulture;

            // В зависимости от совпадения с шаблонами присваиваем культуру. Так можно будет конвертировать числа с точкой и запятой.
            if (Regex.IsMatch(num, @"^(:?[\d,]+\.)*\d+$"))
                cultureInfo = new CultureInfo("en-US");
            else if (Regex.IsMatch(num, @"^(:?[\d.]+,)*\d+$"))
                cultureInfo = new CultureInfo("ru-RU");

            NumberStyles styles = NumberStyles.Number;

            // Конвертируем
            bool isParseble = float.TryParse(num, styles, cultureInfo, out float parseResult);

            // Если можно конвертировать и результат не будет равен бесконечности то записывем результат и возвращаем true
            // иначе присваиваем резульату null и возвращаем false
            if (isParseble && !Single.IsInfinity(parseResult))
            {
                result = parseResult;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }



        /// <summary>
        /// Конвертирует строку в nullable int.
        /// </summary>
        /// <param name="num">Число в строковом представлении</param>
        /// <param name="result">Результат преобразования</param>
        /// <returns>true если пребразование успешно. false если результат null</returns>
        private bool TryParseMark(string num, out int? result)
        {
            // Конвертируем
            bool isParseble = int.TryParse(num, out int parseResult);

            // Если можно конвертировать записывем результат и возвращаем true
            // иначе присваиваем резульату null и возвращаем false
            if (isParseble)
            {
                result = parseResult;
                return true;
            }
            else
            {
                result = null;
                return false;
            }

        }


    }
}

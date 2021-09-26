using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace XlsToJsonConverter.Tests
{
    /// <summary>
    /// Тесты преобразования DataTable в BarsXlsDocment
    /// </summary>
    public class BarsTableParserTests
    {
        /// <summary>
        /// Преобразования корректной таблицы корректно
        /// </summary>
        [Fact]
        public void Parsing_valid_bars_table_is_correct()
        {
            // Подготовка
            DataTable table = new TestBarsTableBuilder("2020/2021, Осенний семестр, Базы данных, экзамен", "А-07-17")
                 .AddBarsUnit("КМ-1", "4", "5")
                 .AddBarsUnit("КМ-2", "8", "30")
                 .AddBarsUnit("КМ-3", "12", "15")
                 .AddBarsUnit("КМ-4", "13", "30")
                 .AddBarsUnit("КМ-5", "16", "20")
                 .FormStructure()
                 .AddStudent("Student1", "4,2", "4,0", "", "", "4", "3", "5", "4", "4")
                 .AddStudent("Student2", "3,5", "", "", "", "3", "4")
                 .AddStudent("Student3", "4,5", "", "", "", "4", "", "", "", "3")
                 .AddStudent("Student4", "4,7", "", "", "", "3", "5", "", "4")
                 .Build();

            BarsTableParser sut = new BarsTableParser();

            // Действие
            BarsDocument result = sut.Parse(table);

            // Проверка
            Assert.Equal("2020/2021, Осенний семестр, Базы данных, экзамен", result.Name);
            Assert.Equal("А-07-17", result.Group);
            Assert.Equal(4, result.Students.Count);

            result.Students.ForEach(s => Assert.Equal(5, s.CmMarks.Count));

            CheckStudent("Student1", 4.2f, 4, null, null, result.Students[0]);
            CheckCm("КМ-1", 4, 5, 4, result.Students[0].CmMarks[0]);
            CheckCm("КМ-2", 8, 30, 3, result.Students[0].CmMarks[1]);
            CheckCm("КМ-3", 12, 15, 5, result.Students[0].CmMarks[2]);
            CheckCm("КМ-4", 13, 30, 4, result.Students[0].CmMarks[3]);
            CheckCm("КМ-5", 16, 20, 4, result.Students[0].CmMarks[4]);

            CheckStudent("Student2", 3.5f, null, null, null, result.Students[1]);
            CheckCm("КМ-1", 4, 5, 3, result.Students[1].CmMarks[0]);
            CheckCm("КМ-2", 8, 30, 4, result.Students[1].CmMarks[1]);
            CheckCm("КМ-3", 12, 15, null, result.Students[1].CmMarks[2]);
            CheckCm("КМ-4", 13, 30, null, result.Students[1].CmMarks[3]);
            CheckCm("КМ-5", 16, 20, null, result.Students[1].CmMarks[4]);

            CheckStudent("Student3", 4.5f, null, null, null, result.Students[2]);
            CheckCm("КМ-1", 4, 5, 4, result.Students[2].CmMarks[0]);
            CheckCm("КМ-2", 8, 30, null, result.Students[2].CmMarks[1]);
            CheckCm("КМ-3", 12, 15, null, result.Students[2].CmMarks[2]);
            CheckCm("КМ-4", 13, 30, null, result.Students[2].CmMarks[3]);
            CheckCm("КМ-5", 16, 20, 3, result.Students[2].CmMarks[4]);

            CheckStudent("Student4", 4.7f, null, null, null, result.Students[3]);
            CheckCm("КМ-1", 4, 5, 3, result.Students[3].CmMarks[0]);
            CheckCm("КМ-2", 8, 30, 5, result.Students[3].CmMarks[1]);
            CheckCm("КМ-3", 12, 15, null, result.Students[3].CmMarks[2]);
            CheckCm("КМ-4", 13, 30, 4, result.Students[3].CmMarks[3]);
            CheckCm("КМ-5", 16, 20, null, result.Students[3].CmMarks[4]);
        }

        /// <summary>
        /// "(В)" удаляется из имени студента
        /// </summary>
        /// <param name="studentName">Имя студента</param>
        /// <param name="expected">Ожидаемый результат</param>
        [Theory]
        [InlineData("Студент(В)", "Студент")]
        [InlineData("Студент (В)", "Студент")]
        [InlineData("Студент(В) Студ", "Студент Студ")]
        [InlineData("КомароВ", "КомароВ")]
        [InlineData("Комаров В.", "Комаров В.")]
        public void Status_char_deleted_from_student_name(string studentName, string expected)
        {
            // Подготовка
            DataTable table = new TestBarsTableBuilder("2020/2021, Осенний семестр, Базы данных, экзамен", "А-07-17")
                .AddBarsUnit("КМ-1", "4", "5")
                .AddBarsUnit("КМ-2", "8", "30")
                .FormStructure()
                .AddStudent(studentName, "4,2", "4,0", "", "", "3", "5")
                .Build();

            BarsTableParser sut = new BarsTableParser();

            // Действие
            BarsDocument result = sut.Parse(table);

            // Проверка
            Assert.Equal(expected, result.Students.First().Name);
        }

        /// <summary>
        /// Некорректные целые числа принимают значение null после обработки
        /// </summary>
        /// <param name="mark">Число</param>
        [Theory]
        [InlineData("9999999999999999999999999999999999999999999999999")]
        [InlineData(null)]
        [InlineData("NOT A NUMBER")]
        [InlineData("2,35467")]
        [InlineData("2.35467")]
        [InlineData("")]
        public void Incorrect_integer_marks_are_null(string mark)
        {
            // Подготовка
            DataTable table = new TestBarsTableBuilder("2020/2021, Осенний семестр, Базы данных, экзамен", "А-07-17")
                .AddBarsUnit("КМ-1", mark, "5")
                .FormStructure()
                .AddStudent("Student1", "4,2", "4,0", "", "", mark)
                .Build();

            BarsTableParser sut = new BarsTableParser();

            // Действие
            BarsDocument result = sut.Parse(table);

            // Проверка
            Assert.Null(result.Students.First().CmMarks.First().Mark);
        }

        /// <summary>
        /// Некорректные числа с плавающей точкой принимают значение null после обработки
        /// </summary>
        /// <param name="mark">Число</param>
        [Theory]
        //[InlineData("9999999999999999999999999999999999999999999999999")]
        [InlineData(null)]
        [InlineData("NOT A NUMBER")]
        [InlineData("")]
        [InlineData("-3.40282347E+38")]
        public void Incorrect_float_marks_are_null(string mark)
        {
            // Подготовка
            DataTable table = new TestBarsTableBuilder("2020/2021, Осенний семестр, Базы данных, экзамен", "А-07-17")
                .AddBarsUnit("КМ-1", "5", "4")
                .AddBarsUnit("КМ-2", "12", "15")
                .FormStructure()
                .AddStudent("Student1", mark, mark, mark, mark, "3")
                .Build();

            BarsTableParser sut = new BarsTableParser();

            // Действие
            BarsDocument result = sut.Parse(table);

            // Проверка
            Assert.Null(result.Students.First().CurrentMark);
            Assert.Null(result.Students.First().SemesterMark);
            Assert.Null(result.Students.First().ExamMark);
            Assert.Null(result.Students.First().TotalMark);
        }

        /// <summary>
        /// Успешно обрабатываются числа с точкой и запятой в качестве десятичного разделителя
        /// </summary>
        [Fact]
        public void Parsing_floats_with_dot_or_comma_is_valid()
        {
            // Подготовка
            DataTable table = new TestBarsTableBuilder("2020/2021, Осенний семестр, Базы данных, экзамен", "А-07-17")
                .AddBarsUnit("КМ-1", "5", "4")
                .AddBarsUnit("КМ-2", "12", "15")
                .FormStructure()
                .AddStudent("Student1", "2,35", "2.35", "3", null)
                .Build();

            BarsTableParser sut = new BarsTableParser();

            // Действие
            BarsDocument result = sut.Parse(table);

            // Проверка
            Assert.Equal(2.35F, result.Students.First().CurrentMark);
            Assert.Equal(2.35F, result.Students.First().SemesterMark);
        }

        /// <summary>
        /// Парсинг таблиц с различным числом контрольных мероприятий проходит успешно
        /// </summary>
        /// <param name="unitCount">Число контрольных мероприятий</param>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        public void Parsing_tables_with_different_BarsUnit_amount_is_valid(int unitCount)
        {
            // Подготовка
            var cmList = new[] {
                new { Name = "КМ-1", Term = "3", Weight = "10" },
                new { Name = "КМ-2", Term = "4", Weight = "15" },
                new { Name = "КМ-3", Term = "10", Weight = "15" },
                new { Name = "КМ-4", Term = "14", Weight = "8" },
                new { Name = "КМ-5", Term = "18", Weight = "2" },
                new { Name = "КМ-6", Term = "23", Weight = "33" },
                new { Name = "КМ-7", Term = "25", Weight = "17" }
            };

            string[] cmMarks = new string[] { "3", "5", "0", "1", "4", "4", "5" };

            var builder = new TestBarsTableBuilder("2020/2021, Осенний семестр, Базы данных, экзамен", "А-07-17");

            for (int i = 0; i < unitCount; i++)
                builder.AddBarsUnit(cmList[i].Name, cmList[i].Term, cmList[i].Weight);

            DataTable table = builder.FormStructure().AddStudent("Student1", "2,56", "3", "4", "4", 
                cmMarks.Take(unitCount).ToArray()).Build();
            BarsTableParser sut = new BarsTableParser();

            // Действие
            BarsDocument result = sut.Parse(table);

            // Проверка
            Assert.Equal(unitCount, result.Students.First().CmMarks.Count);

            for (int i = 0; i < unitCount; i++)
            {
                Assert.Equal(cmList[i].Name, result.Students.First().CmMarks[i].Name);
                Assert.Equal(Int32.Parse(cmList[i].Term), result.Students.First().CmMarks[i].Term);
                Assert.Equal(Int32.Parse(cmList[i].Weight), result.Students.First().CmMarks[i].Weight);
                Assert.Equal(Int32.Parse(cmMarks[i]), result.Students.First().CmMarks[i].Mark);
            }

            Assert.Equal(2.56F, result.Students.First().CurrentMark);
            Assert.Equal(3, result.Students.First().SemesterMark);
            Assert.Equal(4, result.Students.First().ExamMark);
            Assert.Equal(4, result.Students.First().TotalMark);
        }

        /// <summary>
        /// Проверяет контрольной мероприятие на валидность
        /// </summary>
        /// <param name="name">Ожидаемое имя</param>
        /// <param name="term">Ожидаемый срок</param>
        /// <param name="weight">Ожидаемый вес</param>
        /// <param name="mark">Ожидаемая оценка</param>
        /// <param name="result">Результат</param>
        private void CheckCm(string name, int term, int weight, int? mark, BarsUnit result)
        {
            Assert.Equal(name, result.Name);
            Assert.Equal(term, result.Term);
            Assert.Equal(weight, result.Weight);
            Assert.Equal(mark, result.Mark);
        }

        /// <summary>
        /// Проверяет студента и его оценки на валидность
        /// </summary>
        /// <param name="name">Ожидаемое имя студента</param>
        /// <param name="currentMark">Ожидаемый текущий балл</param>
        /// <param name="semestrMark">Ожидаемая семестроая составляющая</param>
        /// <param name="examMark">Ожидаемая экзаменационная составляющая</param>
        /// <param name="total">Ожидаемая оценка за освоение дисциплины</param>
        /// <param name="result">Результат</param>
        private void CheckStudent(string name, float? currentMark, float? semestrMark, float? examMark, int? total, BarsStudent result)
        {
            Assert.Equal(name, result.Name);
            Assert.Equal(currentMark, result.CurrentMark);
            Assert.Equal(semestrMark, result.SemesterMark);
            Assert.Equal(examMark, result.ExamMark);
            Assert.Equal(total, result.TotalMark);
        }


    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace XlsToJsonConverter
{
    /// <summary>
    /// Репозиторий
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// Возвращает коллекцию студентов по указанной учебной группе
        /// </summary>
        /// <param name="group">Учебная группа</param>
        /// <returns>Коллекция студентов принадлежащих группе</returns>
        public IEnumerable<StudentInfo> GetStudentsByGroup(string group);

        /// <summary>
        /// Прикрипляет сообщение к студенту
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="studentName">Имя студента</param>
        /// <param name="group">Группа студента</param>
        public void AddMessageToStudent(DbMessage message, string studentName, string group);

        /// <summary>
        /// Применить изменения
        /// </summary>
        public void Save();
    }
}

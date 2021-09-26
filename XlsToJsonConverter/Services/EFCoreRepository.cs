using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XlsToJsonConverter
{
    /// <summary>
    /// Репозиторий EntityFramework core
    /// </summary>
    public class EFCoreRepository : IRepository
    {
        // Контекст
        private MpeiMessagesContext context = new MpeiMessagesContext();

        /// <summary>
        /// Возвращает коллекцию студентов по указанной учебной группе
        /// </summary>
        /// <param name="group">Учебная группа</param>
        /// <returns>Коллекция студентов принадлежащих группе</returns>
        public IEnumerable<StudentInfo> GetStudentsByGroup(string group) => context.MsgsStudentinfo.Where(g => g.Group == group);

        /// <summary>
        /// Прикрепляет сообщение к студенту
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="studentName">Имя студента</param>
        /// <param name="group">Учебная группа студента</param>
        public void AddMessageToStudent(DbMessage message, string studentName, string group)
        {
            context.MsgsMessage.Add(message);
            context.MsgsStudentinfo.First(s => s.Name == studentName && s.Group == group).MsgsMessageTo.Add(new MsgsMessageTo { Message = message });
        }

        /// <summary>
        /// Применяет изменения
        /// </summary>
        public void Save() => context.SaveChanges();

    }
}

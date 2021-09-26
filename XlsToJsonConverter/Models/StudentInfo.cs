using System;
using System.Collections.Generic;

namespace XlsToJsonConverter
{
    /// <summary>
    /// Информация о студенте (студент в бд)
    /// </summary>
    public partial class StudentInfo : Recipient
    {
        public StudentInfo()
        {
            MsgsMessageTo = new HashSet<MsgsMessageTo>();
        }

        // Идентификатор
        public int Id { get; set; }
        // Институт
        public string Institute { get; set; }
        // Группа
        public string Group { get; set; }

        // Когда создан
        public DateTime CreatedAt { get; set; }
        // Когда обновлен
        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<MsgsMessageTo> MsgsMessageTo { get; set; }
    }
}

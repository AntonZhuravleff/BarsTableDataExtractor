using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace XlsToJsonConverter
{
    /// <summary>
    /// Сообщение для хранения в базе данных
    /// </summary>
    public partial class DbMessage : Message
    {
        public DbMessage()
        {
            MsgsMessageTo = new HashSet<MsgsMessageTo>();
        }

        public DbMessage(MessageToSend message)
        {
            Sender = message.Sender;
            Type = (short)message.Type;
            CreatedAt = message.CreatedAt;
            Subject = message.Subject;
            Body = message.Body;
        }

        // Идентификатор
        public int Id { get; set; }
        // Тип сообщения
        public short Type { get; set; }
        // Дата и время обновления
        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<MsgsMessageTo> MsgsMessageTo { get; set; }
    }
}

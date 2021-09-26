using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace XlsToJsonConverter
{
    /// <summary>
    /// Модель сообщения
    /// </summary>
    public class MessageToSend : Message
    {

        //Тип сообщения
        [JsonProperty(Order = 3)]
        public MessageType Type { get; set; }

        // Список получателей
        [JsonProperty(Order = 2)]
        public IList<Recipient> To { get; set; } = new List<Recipient>();

        // Список вложений
        [JsonProperty(Order = 7)]
        public IList<Attachment> Attachments { get; set; }
    }

    /// <summary>
    /// Тип сообщения
    /// </summary>
    public enum MessageType
    {
        Bars, PrivateMsg, Group, Institute, University
    }


}

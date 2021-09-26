using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace XlsToJsonConverter
{
    public class Message
    {
        // От кого
        [JsonProperty(Order = 1, PropertyName = "From")]
        public string Sender { get; set; }

        // Дата и время создания
        [JsonProperty(Order = 4)]
        public DateTime CreatedAt { get; set; }

        // Тема сообщения
        [JsonProperty(Order = 5)]
        public string Subject { get; set; }

        // Тело сообщения
        [JsonProperty(Order = 6)]
        public string Body { get; set; }
    }
}

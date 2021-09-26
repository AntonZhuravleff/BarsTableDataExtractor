using System;
using System.Collections.Generic;
using System.Text;

namespace XlsToJsonConverter
{
    /// <summary>
    /// Получатель сообщения
    /// </summary>
    public class Recipient
    {
        // Имя
        public string Name { get; set; }
        // Адрес электронной почты
        public string Email { get; set; }
        // Номер телефона
        public string Phone { get; set; }
        // id ВКонтакте
        public string Vk { get; set; }
        // id Телеграм
        public string Telegram { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace XlsToJsonConverter
{
    public partial class MsgsMessageTo
    {
        // Id
        public int Id { get; set; }

        // Id сообщения
        public int MessageId { get; set; }

        // Id студента
        public int StudentinfoId { get; set; }

        public virtual DbMessage Message { get; set; }
        public virtual StudentInfo Studentinfo { get; set; }
    }
}

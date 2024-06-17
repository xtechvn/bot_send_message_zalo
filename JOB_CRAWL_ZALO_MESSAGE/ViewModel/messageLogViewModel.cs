using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JOB_CRAWL_ZALO_MESSAGE.ViewModel
{
  public  class messageLogViewModel
    {
        public int user_id { get; set; }
        public string chat_id { get; set; }
        public string content { get; set; }
        public string content_raw { get; set; }
        public string send_date { get; set; } // ngay gui
        public string group_chat { get; set; }
        public string sender_name  { get; set; } // người gửi
        public string send_time { get; set; } // thời gian gửi
        public DateTime create_date { get; set; } // ngày craw tin nhắn

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HAIAPI.Models
{
    public class IdentityCheckIn
    {

    }

    public class CheckCalendarResult : ResultInfo
    {
        public List<CalendarType> status { get; set; }

        public List<string> month { get; set; }
    }

}
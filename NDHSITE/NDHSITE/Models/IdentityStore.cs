using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NDHSITE.Models
{

    using System;
    using System.Collections.Generic;
    public class IdentityStore
    {
    }

    public partial  class checkin_report_plan_result
    {
        public string AgencyCode { get; set; }

        public string CType { get; set; }

        public int? D1 { get; set; }
        public int? D2 { get; set; }
        public int? D3 { get; set; }
        public int? D4 { get; set; }
        public int? D5 { get; set; }
        public int? D6 { get; set; }
        public int? D7 { get; set; }

        public int? D8 { get; set; }
        public int? D9 { get; set; }
        public int? D10{ get; set; }
        public int? D11 { get; set; }
        public int? D12 { get; set; }
        public int? D13 { get; set; }
        public int? D14 { get; set; }
        public int? D15 { get; set; }
        public int? D16 { get; set; }
        public int? D17 { get; set; }
        public int? D18 { get; set; }
        public int? D19 { get; set; }
        public int? D20 { get; set; }
        public int? D21 { get; set; }
        public int? D22 { get; set; }
        public int? D23 { get; set; }
        public int? D24 { get; set; }
        public int? D25 { get; set; }
        public int? D26 { get; set; }
        public int? D27 { get; set; }
        public int? D28 { get; set; }
        public int? D29 { get; set; }
        public int? D30 { get; set; }
        public int? D31 { get; set; }


        public int? getValue(int day)
        {
            try
            {
                return this.GetType().GetProperty("D" + day).GetValue(this, null) as int?;
            }
            catch
            {
                return 0;
            }
        }
    }
}
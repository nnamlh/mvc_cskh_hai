using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NDHSITE.Util
{
    public static class HaiUtil
    {
        public static string ConvertProductQuantityText(int? box, int? quantity, string unit)
        {
            int? countCan = quantity / box;
            int? countBox = quantity - countCan * box;

            if (countCan == 0)
            {
                return countBox + " " + unit;
            }

            if (countBox == 0)
            {
                return countCan + " thùng";
            }

            return countCan + " thùng " + countBox + " " + unit;

        }

    }
}
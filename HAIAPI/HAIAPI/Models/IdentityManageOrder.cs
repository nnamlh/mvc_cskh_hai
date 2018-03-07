using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HAIAPI.Models
{
    public class StaffOrderRequest : RequestInfo
    {
        //
        public string c1Code { get; set; }

        public string status { get; set; }

        public int? page { get; set; }

        public string fdate { get; set; }

        public string tdate { get; set; }

        public string place { get; set; }

        public string processId { get; set; }
    }

    public class C2OrderRequest : RequestInfo
    {
        public int? page { get; set; }
    }

    public class YourOrderResult : ResultInfo
    {
        public List<YourOrder> orders { get; set; }
    }


    public class UpdateDeliveryCompleteRequest : RequestInfo
    {
        public string orderId { get; set; }

    
    }

    public class UpdateDeliveryCompleteResult : ResultInfo
    {
        public List<ProductOrderInfo> products { get; set; }

        public string deliveryStatus { get; set; }

        public string deliveryStatusCode { get; set; }


    }

    public class UpdateDeliveryResult : ResultInfo
    {
        public string deliveryStatus { get; set; }

        public string deliveryStatusCode { get; set; }

        public int finish { get; set; }

    }

}
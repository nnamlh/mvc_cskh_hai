using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HAIAPI.Models
{

    public class C1OrderRequest : RequestInfo
    {
        
        public string c2Code { get; set; }

        public string status { get; set; }

        public int? page { get; set; }
    }

    public class C1OrderResult : ResultInfo
    {
        public List<YourOrder> orders { get; set; }
    }

   public class UpdateOrderRequest: RequestInfo
    {
        public string orderId { get; set; }

        public string productId { get; set; }

        public int? quantity { get; set; }
    }
   
}
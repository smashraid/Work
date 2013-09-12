using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class SubscriptionReceipt 
    {
      public int Id { get; set; }
      public int MemberId { get; set; }
      public int Status { get; set; }
      public string Receipt { get; set; }
      public string LatestReceipt { get; set; }
      public string LatestReceiptInfo { get; set; }
      public string LatestExpiredReceiptInfo { get; set; }
      public DateTime CreatedDate { get; set; }
    }


//"receipt": {
//        "original_purchase_date_pst": "2013-08-05 04:00:38 America/Los_Angeles",
//        "unique_identifier": "0000b01969f8",
//        "original_transaction_id": "1000000082964147",
//        "expires_date": "1376460989047",
//        "transaction_id": "1000000084020731",
//        "quantity": "1",
//        "product_id": "inapp.metafitness.weekly",
//        "original_purchase_date_ms": "1375700438000",
//        "bid": "com.rd.iphone",
//        "web_order_line_item_id": "1000000027238061",
//        "bvrs": "1.29",
//        "expires_date_formatted": "2013-08-14 06:16:29 Etc/GMT",
//        "purchase_date": "2013-08-14 06:13:29 Etc/GMT",
//        "purchase_date_ms": "1376460809047",
//        "expires_date_formatted_pst": "2013-08-13 23:16:29 America/Los_Angeles",
//        "purchase_date_pst": "2013-08-13 23:13:29 America/Los_Angeles",
//        "original_purchase_date": "2013-08-05 11:00:38 Etc/GMT",
//        "item_id": "684155875"
//    },
//    "latest_expired_receipt_info": {
//        "original_purchase_date_pst": "2013-08-05 04:00:38 America/Los_Angeles",
//        "unique_identifier": "0000b01969f8",
//        "original_transaction_id": "1000000082964147",
//        "expires_date": "1376461889000",
//        "transaction_id": "1000000084024238",
//        "quantity": "1",
//        "product_id": "inapp.metafitness.weekly",
//        "original_purchase_date_ms": "1375700438000",
//        "bid": "com.rd.iphone",
//        "web_order_line_item_id": "1000000027254453",
//        "bvrs": "1.25",
//        "expires_date_formatted": "2013-08-14 06:31:29 Etc/GMT",
//        "purchase_date": "2013-08-14 06:28:29 Etc/GMT",
//        "purchase_date_ms": "1376461709000",
//        "expires_date_formatted_pst": "2013-08-13 23:31:29 America/Los_Angeles",
//        "purchase_date_pst": "2013-08-13 23:28:29 America/Los_Angeles",
//        "original_purchase_date": "2013-08-05 11:00:38 Etc/GMT",
//        "item_id": "684155875"
//    },
//    "status": 21006
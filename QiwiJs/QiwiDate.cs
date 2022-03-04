using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServiceApi.QiwiJs
{
    class QiwiDate
    {
        public data[] data;
        public string nextTxnId { get; set; }
        public string nextTxnDate { get; set; }
    }
    class data
    {
        public string txnId { get; set; }
        public string personId { get; set; }
        public string account { get; set; }
        public string date { get; set; }
        public string trmTxnId { get; set; }
        public sum sum { get; set; }
        public sum commission { get; set; }
        public sum total { get; set; }
        public provider provider { get; set; }
    }
}

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;
using ServiceApi.QiwiJs;

namespace ServiceApi
{
    class QiwiApi
    {
        public static QiwiDate OperationHistory(string token, string phone, string nextTxnId=null, string nextTxnDate=null)
        {
            string url;
            if (nextTxnDate is null || nextTxnId is null)
                url =$"https://edge.qiwi.com/payment-history/v2/persons/{phone}/payments?rows=50";
            else
                url = $"https://edge.qiwi.com/payment-history/v2/persons/{phone}/payments?rows=50&nextTxnId={nextTxnId}&nextTxnDate={nextTxnDate}";
            string resultPage = "";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";

            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Authorization", $"Bearer {token}");
            request.Headers.Add("Content-type", "aplication/json");
            request.Headers.Add("Host", "edge.qiwi.com");

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8, true))
                {
                    resultPage = sr.ReadToEnd();
                    sr.Close();
                }
            }
            catch (WebException e)
            {
                Console.WriteLine(e.Message);
                return new QiwiDate();
            }
            return JsonConvert.DeserializeObject<QiwiDate>(resultPage);
        }
    }
}

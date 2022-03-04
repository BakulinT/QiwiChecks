using System;
using System.IO;
using System.Drawing;
using Newtonsoft.Json.Linq;

using ServiceApi.QiwiJs;

namespace ServiceApi
{
    class DateTimeT
    {
        public string date { get; set; }
    }
    class JpgConvert
    {
        private static string path = @"../../../Sample/";
        public static Stream CreateImageQiwi(data qiwi)
        {
            string date = "{\u0022date\u0022:\u0022" + qiwi.date + "\u0022}";
            var tempDate = JObject.Parse(date).ToObject<DateTimeT>();

            if (qiwi.provider.shortName.IndexOf("Xsolla") == -1 && qiwi.provider.shortName.IndexOf("STEAM") == -1 && qiwi.provider.shortName.IndexOf("steam") == -1)
                return new MemoryStream();

            Image img = new Bitmap(path + "qiwi.png");
            int heihgt = img.Height,
                width = img.Width,
                x = 330;

            Graphics g = Graphics.FromImage(img);

            g.DrawString(qiwi.txnId.ToString(), new Font("Verdana", (float)14),
                new SolidBrush(Color.Black), x, 153);
            g.DrawString(tempDate.date, new Font("Verdana", (float)14),
                new SolidBrush(Color.Black), x, 200);
            g.DrawString(qiwi.provider.shortName.ToString(), new Font("Verdana", (float)14),
                new SolidBrush(Color.Black), x, 248);
            g.DrawString(qiwi.account.ToString(), new Font("Verdana", (float)14),
                new SolidBrush(Color.Black), x, 300);
            g.DrawString(qiwi.trmTxnId.ToString(), new Font("Verdana", (float)14),
                new SolidBrush(Color.Black), x, 353);
            g.DrawString(qiwi.personId.ToString(), new Font("Verdana", (float)14),
                new SolidBrush(Color.Black), x, 427);

            g.DrawString(qiwi.sum.amount + " Р", new Font("Verdana", (float)16, FontStyle.Bold),
                new SolidBrush(Color.Black), 222, 512);
            g.DrawString(qiwi.commission.amount + " Р", new Font("Verdana", (float)14),
                new SolidBrush(Color.Black), x, 577);
            g.DrawString(qiwi.total.amount + " Р", new Font("Verdana", (float)14),
                new SolidBrush(Color.Black), x, 623);

            using (var ms = new MemoryStream())
            {
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return new MemoryStream(ms.ToArray());
            }
        }
    }
}

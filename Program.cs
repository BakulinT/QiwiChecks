using System;
using System.IO;
using System.Threading.Tasks;
using ServiceApi.TelegramBot;
using Newtonsoft.Json;

namespace ServiceApi
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string path = @"../../../config.json", sRead = "{}";

            using (StreamReader sr = new StreamReader(path))
            {
                sRead = await sr.ReadToEndAsync();
            }

            Config js = JsonConvert.DeserializeObject<Config>(sRead);
            await TelegramBotApi.StartBot(js);
        }
    }
}

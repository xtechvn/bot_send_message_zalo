using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JOB_LISTENT_TELEGRAM
{
    struct BotUpdate
    {
        public string content_data;
        public long chat_id;
        public string? customer_name;
        public long user_id;
        public string send_date;
        public string group_name;
        public string token_group_name;
    }

    class Program
    {
        public static string telegram_token = ConfigurationManager.AppSettings["telegram_token"];
        public static int total_msg_history = Convert.ToInt32(ConfigurationManager.AppSettings["total_msg_history"]);
        public static string api_listent_message = (ConfigurationManager.AppSettings["api_listent_message"]).ToString();
        static TelegramBotClient Bot = new TelegramBotClient(telegram_token);

        static string fileName = "updates.json";
        static List<BotUpdate> botUpdates = new List<BotUpdate>();

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            //TelegramBotClient bot = new TelegramBotClient(telegram_token);
            //var tele = new Tele(telegram_token, telegram_group);
            //tele.TelegramBotReply();

            //Read all saved updates
            try
            {
                var botUpdatesString = System.IO.File.ReadAllText(fileName);

                botUpdates = JsonConvert.DeserializeObject<List<BotUpdate>>(botUpdatesString) ?? botUpdates;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading or deserializing {ex}");
            }

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[]
                {
                    UpdateType.Message,
                    UpdateType.EditedMessage,
                }
            };

            Bot.StartReceiving(UpdateHandler, ErrorHandler, receiverOptions);

            Console.ReadLine();


        }

        private static Task ErrorHandler(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            throw new NotImplementedException();
        }

        private static async Task UpdateHandler(ITelegramBotClient bot, Update update, CancellationToken arg3)
        {
            if (update.Type == UpdateType.Message)
            {
                if (update.Message.Type == MessageType.Text)
                {
                    //write an update
                    var _botUpdate = new BotUpdate
                    {
                        user_id = 1,
                        send_date = DateTime.Now.ToString("dd/MM/yyyy"),
                        content_data = update.Message.Text,
                        chat_id = update.Message.MessageId,
                        customer_name = update.Message.From.Username.ToString(),
                        group_name = string.IsNullOrEmpty(update.Message.Chat.Title) ? update.Message.From.Username.ToString() : update.Message.Chat.Title,
                        token_group_name = telegram_token
                    };

                    botUpdates.Add(_botUpdate);

                    // Push api nhận Notify
                    pushApiNotify(_botUpdate);

                    // Tin hiện tại
                    Console.WriteLine("[" + _botUpdate.group_name + "] " + _botUpdate.customer_name + " say: " + _botUpdate.content_data);

                    // lưu history 100 tin mới nhất
                    var msg_history = botUpdates.OrderBy(x => x.chat_id).Take(total_msg_history).ToList();

                    var botUpdatesString = JsonConvert.SerializeObject(msg_history);
                    System.IO.File.WriteAllText(fileName, botUpdatesString);
                }
            }
        }

        private static async void pushApiNotify(BotUpdate message_info)
        {
            try
            {

                string URI = api_listent_message;
                string myParameters = "?typePut=2&userid=" + message_info.user_id + "&customerName=" + message_info.customer_name + "&contentData=" + Encode(message_info.content_data) + "&DateData=" + message_info.send_date + "&nameGroup=" + message_info.group_name + "";

                var webRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(URI + myParameters);
                var action = "http://sap.com/xi/WebService/soap1.1";

                webRequest.Headers.Add("SOAPAction", action);
                webRequest.ContentType = "text/xml;charset=\"utf-8\"";
                webRequest.Accept = "text/xml";
                webRequest.Method = "POST";
                System.Net.NetworkCredential credential = new System.Net.NetworkCredential("", "");
                webRequest.Credentials = credential;
                webRequest.KeepAlive = true;
                webRequest.UseDefaultCredentials = true;

                string soapResult;
                using (WebResponse webResponse = webRequest.GetResponse())
                {
                    using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
                    {
                        soapResult = rd.ReadToEnd();
                    }
                    Console.WriteLine("Response api from " + message_info.customer_name + ": " + soapResult);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }
        }





        public static string Encode(string strString)
        {
            try
            {
                strString = KeyED(strString, "key");
                Byte[] byt = System.Text.Encoding.UTF8.GetBytes(strString);
                strString = Convert.ToBase64String(byt);
                return strString;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }

        }
        private static string KeyED(string strString, string strKeyphrase)
        {
            int strStringLength = strString.Length;
            int strKeyPhraseLength = strKeyphrase.Length;

            System.Text.StringBuilder builder = new System.Text.StringBuilder(strString);

            for (int i = 0; i < strStringLength; i++)
            {
                int pos = i % strKeyPhraseLength;
                int xorCurrPos = (int)(strString[i]) ^ (int)(strKeyphrase[pos]);
                builder[i] = Convert.ToChar(xorCurrPos);
            }

            return builder.ToString();
        }
    }
}

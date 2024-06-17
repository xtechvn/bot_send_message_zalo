using HtmlAgilityPack;
using JOB_CRAWL_MESSAGE_ZALO.common;
using JOB_CRAWL_ZALO_MESSAGE.ViewModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using Quartz;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using SeleniumExtras.WaitHelpers;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace JOB_CRAWL_MESSAGE_ZALO
{
    /// <summary>
    /// create by: cuonglv
    /// note: Job sẽ thực hiện chạy định kỳ để gửi thông tin tới nhóm zalo thông qua api trả về
    /// 
    /// </summary>
    public class ZaloJobCrawler : IJob
    {

        private static string link_zalo = ConfigurationManager.AppSettings["DOMAIN_WEBSITE_CRAWLER"];
        private static int second_waiting_login = Convert.ToInt16(ConfigurationManager.AppSettings["second_waiting_login"]);
        private static string group_zalo = ConfigurationManager.AppSettings["group_zalo"];
        public static string api_listent_message = (ConfigurationManager.AppSettings["api_listent_message"]).ToString();
        private static string user_data_dir = ConfigurationManager.AppSettings["user_data_dir"];
        private static int time_delay_read_message = Convert.ToInt16(ConfigurationManager.AppSettings["time_delay_read_message"]);
        private static string private_key_api = ConfigurationManager.AppSettings["private_key_api"];
        
        static List<messageLogViewModel> botUpdates = new List<messageLogViewModel>();
        void IJob.Execute(IJobExecutionContext context)
        {
            try
            {
                string startupPath = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\bin\Debug\", @"\");
                //// setting
                var chrome_option = new ChromeOptions();
                chrome_option.AddArgument("--start-maximized"); // set full man hinh
                chrome_option.AddArgument(@"user-data-dir=" + user_data_dir);
                chrome_option.AddArgument("--remote-debugging-port=9222");
                string x_path_text_message = "//*[@id=\"richInput\"]";
                string x_path_send = "//div[contains(@icon, 'Sent-msg_24_Line') and contains(@class, 'chat-box-input-button') and contains(@class, 'icon-only')]";
                string x_path_filter_zalo = "//*[@id='contact-search-input']";
                string x_path_select_sender = "(//*[contains(@id,'group-item-')])[1] | (//*[contains(@id,'friend-item-')])[1]";// "(//*[contains(@id,'group-item-')])[1]";                
                string x_path_fill_friend = "//div[contains(@id,'friend-item-')]";
                string x_path_name_my_account = "//*[contains(@class,'nav__tabs__zalo web')]"; // group chat cua loai friend chat
                string my_account_name = string.Empty;

                ChromeDriverService service = ChromeDriverService.CreateDefaultService();

                using (var browers = new ChromeDriver(startupPath, chrome_option))
                {
                    try
                    {
                        browers.Navigate().GoToUrl(link_zalo);

                        // Chờ login 
                        // Thread.Sleep(second_waiting_login);
                        WebDriverWait wait = new WebDriverWait(browers, TimeSpan.FromSeconds(10));
                        wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(x_path_filter_zalo)));// chờ cho tới khi tìm thấy xpath kế tiếp

                        var group_name = group_zalo.Split(',');
                        for (int i = 0; i <= group_name.Length - 1; i++)
                        {


                            // Lọc nhóm Zalo để gửi tin nhắn
                            if (!checkXpathExist(x_path_filter_zalo, browers))
                            {
                                continue;
                            }
                            browers.FindElement(By.XPath(x_path_filter_zalo)).Clear();
                            browers.FindElement(By.XPath(x_path_filter_zalo)).SendKeys(group_name[i]);
                            Thread.Sleep(300);

                            Console.WriteLine("------[" + DateTime.Now.ToString("dd-MM-yyyy HH:mm") + "] Quét tin trong nhóm: " + group_name[i] + "--------");

                            if (checkXpathExist(x_path_select_sender, browers))
                            {
                                // Chọn nhóm
                                Thread.Sleep(3000);
                                browers.FindElement(By.XPath(x_path_select_sender)).Click();

                                // Detect group or friend
                                if (checkXpathExist(x_path_fill_friend, browers))
                                {

                                    my_account_name = browers.FindElement(By.XPath(x_path_name_my_account)).GetAttribute("title").ToString();
                                }
                            }
                            else
                            {
                                continue;
                            }

                            Thread.Sleep(600);

                            // Get data message for API
                            var obj_report =  getDataSending();
                            
                            // Send Message
                            if (obj_report != null)
                            {
                                string msg_send = obj_report.Result.data;

                                browers.FindElement(By.XPath(x_path_text_message)).SendKeys("..."); // fill content    
                                IWebElement textElement = browers.FindElement(By.XPath("//*[@id=\"input_line_0\"]"));
                                IJavaScriptExecutor js = (IJavaScriptExecutor)browers;                              
                                
                                // Refresh phần tử để cập nhật giá trị mới
                                textElement = browers.FindElement(By.Id("input_line_0"));

                                js.ExecuteScript("arguments[0].textContent  = arguments[1];" +
                                      "var event = new Event('input', { bubbles: true, cancelable: true });" +
                                      "arguments[0].dispatchEvent(event);",
                                      textElement, msg_send);                                

                                Thread.Sleep(3000);
                                browers.FindElement(By.XPath(x_path_send)).Click(); //send zalo
                                var actions = new Actions(browers);
                                actions.KeyDown(Keys.Control).SendKeys(Keys.Enter).KeyUp(Keys.Control).Perform();


                                Console.WriteLine("Send msg success !!!");
                                Thread.Sleep(5000);
                            }

                        }
                        browers.Dispose();

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("IJob.Execute= " + ex.ToString());
                        ErrorWriter.WriteLog(startupPath, "IJob.Execute()", ex.ToString());

                    }
                }
                Environment.Exit(0); // dd
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        // call api
        //var sb = new StringBuilder();
        //sb.AppendLine("======= Báo cáo cuối ngày 15/06/2024 =========");
        //sb.AppendLine("\nDoanh thu tổng: 13,000,000,000");                
        //sb.AppendLine("\nLợi nhuận thuần: 13,661,656,519");
        //sb.AppendLine("\nTổng khách đã thanh toán trong ngày: 500,000");
        //sb.AppendLine("\n======= End 15/06/2024 =========");
        //var msg_send = sb.ToString();
        //return msg_send;
        private static readonly HttpClient client = new HttpClient();
        public async Task<apiReportRevenueResponseViewModel> getDataSending()
        {
            try
            {                
                // Gửi yêu cầu GET đến API
                //var j_param = new Dictionary<string, string>
                //{
                //    {"fromdate", "2024/06/16"},
                //    {"todate", "2024/06/16"}
                //};
                //var token = JsonConvert.SerializeObject(j_param);
                string j_param = "{'fromdate':'2024/06/16','todate':'2024/06/16'}";
                var token = Encode(j_param, private_key_api);

                string apiUrl = api_listent_message + "/api/get-msg-zalo.json";

                var result = await CreateHttpRequest(token, apiUrl);

                var data_msg = JsonConvert.DeserializeObject<apiReportRevenueResponseViewModel>(result.ToString());
                return data_msg;
            }
            catch (Exception ex)
            {
                // logtele
                Console.WriteLine(ex.ToString());
                return null;
            }
        }
        public async Task<string> CreateHttpRequest(string token, string url_api)
        {
            try
            {
                string responseFromServer = string.Empty;
                string status = string.Empty;
                var httpClientHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
                };

                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("token", token),
                    });

                    var response_api = await httpClient.PostAsync(url_api, content);

                    // Nhan ket qua tra ve                            
                    responseFromServer = response_api.Content.ReadAsStringAsync().Result;

                }

                return responseFromServer;
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram(token_tele, group_id, "[API NOT CONNECT] CreateHttpRequest error: " + ex.ToString() + " token =" + token + " url_api = " + url_api);
                return string.Empty;
            }
        }
        public string getContentByXpath(string html_source, string x_path)
        {
            try
            {
                var document = new HtmlDocument();
                document.LoadHtml(html_source);
                var nodes = document.DocumentNode.SelectNodes(x_path);

                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        var result = ((node.InnerText).Replace("&nbsp;", " "));

                        return result;
                    }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return string.Empty;

            }
        }
        public static bool checkXpathExist(string s_x_path, ChromeDriver browers)
        {
            try
            {
                List<IWebElement> list_input_link_elements = browers.FindElements(By.XPath(s_x_path)).ToList();
                if (list_input_link_elements.Count() == 0)
                {
                    return false;
                }
                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }





        public static string Encode(string strString,string strKeyphrase)
        {
            try
            {
                strString = KeyED(strString, strKeyphrase);
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
        public static string GetContentObject(string sContentEncode, string sKey)
        {
            try
            {
                sContentEncode = sContentEncode.Replace(" ", "+");
                string data = Decode(sContentEncode, sKey); // Lay ra content 
                return data;
            }
            catch (Exception ex)
            {

                ErrorWriter.WriteLog(System.Web.HttpContext.Current.Server.MapPath("~"), "GiaiMa()", ex.ToString());
                return string.Empty;
            }

        }
        public static string Decode(string strString, string strKeyPhrase)
        {
            Byte[] byt = Convert.FromBase64String(strString);
            strString = System.Text.Encoding.UTF8.GetString(byt);
            strString = KeyED(strString, strKeyPhrase);
            return strString;
        }

    }
}

using JOB_CRAWL_MESSAGE_ZALO.common;
using OpenQA.Selenium.Chrome;
using Quartz;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace JOB_CRAWL_MESSAGE_ZALO
{
    public class ZaloJobCrawler : IJob
    {
        private static string link_zalo = ConfigurationManager.AppSettings["DOMAIN_WEBSITE_CRAWLER"];
        void IJob.Execute(IJobExecutionContext context)
        {
            try
            {
                string startupPath = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\bin\Debug\", @"\");
                //// setting
                var chrome_option = new ChromeOptions();
                chrome_option.AddArgument("--start-maximized"); // set full man hinh
                // PC
                chrome_option.AddArgument("--user-agent=Mozilla/5.0 (iPad; CPU OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5355d Safari/8536.25");
               

                using (var browers = new ChromeDriver(startupPath, chrome_option))
                {
                    try
                    {
                        browers.Navigate().GoToUrl(link_zalo);


                    }
                    catch (Exception ex)
                    {
                        ErrorWriter.WriteLog(startupPath, "IJob.Execute()", ex.ToString());

                    }
                }



                Environment.Exit(0);
            }
            catch (Exception ex)
            {

                Console.ReadLine();

            }
        }
    }
}

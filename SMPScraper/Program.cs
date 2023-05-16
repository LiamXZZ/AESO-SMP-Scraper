using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngleSharp;
using PuppeteerSharp;
using System.Net.Mail;
using System.Net;

namespace SMPScraper
{
    internal class Program
    {

        static Uri AESOSMPURL = new Uri("http://ets.aeso.ca/ets_web/ip/Market/Reports/CSMPriceReportServlet");

        static async Task Main(string[] args)
        {

            Console.WriteLine("Choose the scraping method: ");
            Console.WriteLine("1. HTTPClient Scraper");
            Console.WriteLine("2. AngleSharp Scraper");
            Console.WriteLine("3. Puppeteer Scraper");

            var ScrapeMethod = Console.ReadLine();

            Console.WriteLine("Would you like to receive email updates on new prices? (Y/N)");

            var EmailNotification = Console.ReadLine();

            string EmailToAddress = "";

            if(string.Equals(EmailNotification,"Y",StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Please enter the email address you would like to receive price update on: ");
                EmailToAddress = Console.ReadLine();

            }

            var SMPTable = new DataTable();

            while(true)
            {
                try
                {
                    var NewSMPTable = new DataTable();

                    switch (ScrapeMethod)
                    {
                        case "1":
                            {
                                NewSMPTable = await HTTPClientScraper.ScrapeData(AESOSMPURL);
                                break;
                            }

                        case "2":
                            {
                                NewSMPTable = await AngleSharpScraper.ScrapeData(AESOSMPURL);
                                break;
                            }

                        case "3":
                            {
                                NewSMPTable = await PuppeteerScraper.ScrapeData(AESOSMPURL);
                                break;
                            }

                        default:
                            {
                                NewSMPTable = await HTTPClientScraper.ScrapeData(AESOSMPURL);
                                break;
                            }
                    }


                    if (string.Equals(EmailNotification, "Y", StringComparison.OrdinalIgnoreCase))
                    {
                        ComparePriceTable(SMPTable, NewSMPTable, true, EmailToAddress);
                    }
                    else
                    {
                        ComparePriceTable(SMPTable, NewSMPTable, false);
                    }

                    SMPTable = NewSMPTable;

                    WriteToCSVFile("AESO SMP Table", SMPTable);

                    //Poll the website every 10 seconds
                    Thread.Sleep(10000);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }

        }

        private static void ComparePriceTable(DataTable CurrentSMPTable, DataTable NewSMPTable, bool EmailNotification, string EmailToAddress = null)
        {
            try
            {
                if (CurrentSMPTable.Rows.Count > 0)
                {
                    var DRC = DataRowComparer<DataRow>.Default;

                    //Compare the top row of both tables to see if values have been updated

                    if (!DRC.Equals(CurrentSMPTable.Rows[0], NewSMPTable.Rows[0]))
                    {
                        Console.WriteLine("New Data updated!");

                        //Play one of the system sounds to alert user when there is a price update
                        System.Media.SystemSounds.Asterisk.Play();

                        //Show message box alert in Windows, however it will cause the thread to hang if it is not closed/interacted with
                        //System.Windows.Forms.MessageBox.Show("New Data updated!");

                        SendEmailAlert(NewSMPTable,EmailToAddress);

                        var row = NewSMPTable.Rows[0];

                        foreach (DataColumn column in NewSMPTable.Columns)
                        {
                            Console.Write(String.Format("{0,15}", column.ColumnName));

                        }
                        Console.WriteLine();


                        for (int cellindex = 0; cellindex < row.ItemArray.Length; cellindex++)
                        {
                            Console.Write(String.Format("{0,15}", row[cellindex]));
                        }

                        Console.WriteLine();

                    }

                }

                else
                {

                    foreach (DataColumn column in NewSMPTable.Columns)
                    {

                        Console.Write(String.Format("{0,15}", column.ColumnName));

                    }

                    Console.WriteLine();

                    foreach (DataRow row in NewSMPTable.Rows)
                    {
                        for (int cellindex = 0; cellindex < row.ItemArray.Length; cellindex++)
                        {
                            Console.Write(String.Format("{0,15}", row[cellindex]));
                        }

                        Console.WriteLine();

                    }

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void SendEmailAlert(DataTable dtPrice, string EmailToAddress)
        {
            try
            {
                var EmailAddress = new MailAddress("aesosystemmarginalprice@gmail.com","AESO SMP");

                var Password = "rzhmqncxiizrkalr";

                var Subject = "AESO System Marginal Price Update";

                var smpt = new SmtpClient
                {

                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(EmailAddress.Address, Password)

                };

                string EmailMessage = "";

                for(int i = 0; i < dtPrice.Columns.Count; i++)
                {
                    //Construct Email Message, using the column names and the first row of the Price table
                    EmailMessage += dtPrice.Columns[i].ColumnName + ": " + dtPrice.Rows[0][i] + Environment.NewLine;

                }

                using (var message = new MailMessage(EmailAddress.Address,EmailToAddress,Subject,EmailMessage))
                {
                    smpt.Send(message);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private static void WriteToCSVFile(string FileName, DataTable dataTable)
        {        
            string CSVFileName = FileName + ".csv";

            try
            {
                using (StreamWriter writer = new StreamWriter(new FileStream(CSVFileName, FileMode.Create, FileAccess.Write)))
                {
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        writer.Write(column.ColumnName + ",");
                    }
                    writer.WriteLine();

                    foreach (DataRow row in dataTable.Rows)
                    {
                        foreach (var cell in row.ItemArray)
                        {
                            writer.Write(cell + ",");
                        }
                        writer.WriteLine();
                    }

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


        }
    }
}

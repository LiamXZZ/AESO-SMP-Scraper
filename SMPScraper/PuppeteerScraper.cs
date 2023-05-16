using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPScraper
{
    internal class PuppeteerScraper
    {

        //Puppeteer Scraper uses the Puppeteer API to simulate a real chromium browser via the Chrome DevTools Protocol, often used for automated browser testing similar to Selenium
        //Advantages: Can be run in either headless or headful mode, compatible with all websites by simulating real user activity, can connect to existing browser and use existing user profiles in all Chromium based browsers
        //Disadvantages: Heavy weight, resource intensive

        public static async Task<DataTable> ScrapeData(Uri AESOSMPURL)
        {
            var SMPTable = new DataTable();

            try
            {
                //Download chromium browser if not available
                using (var browserFetcher = new BrowserFetcher(Product.Chrome))
                {
                    await browserFetcher.DownloadAsync();
                }

                
                using (var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Product = Product.Chrome, Headless = false }))
                {
                    var page = await browser.NewPageAsync();

                    await page.GoToAsync(AESOSMPURL.AbsoluteUri);

                    //wait for all table elements to load
                    await page.WaitForSelectorAsync("table");

                    var table = await page.QuerySelectorAsync("table[border=\"1\"]");

                    //get all header columns
                    var headers = await table.QuerySelectorAllAsync("th");

                    foreach (var column in headers)
                    {
                        var innerText = (await (await column.GetPropertyAsync("innerText")).JsonValueAsync()).ToString();

                        SMPTable.Columns.Add(new DataColumn(innerText));
                    }

                    var rows = await table.QuerySelectorAllAsync("tr:has(td)");

                    foreach(var row in rows)
                    {
                        var cells = await row.QuerySelectorAllAsync("td");

                        var datarow = SMPTable.NewRow();

                        for(int cellindex = 0; cellindex < cells.Length; cellindex++)
                        {
                            var innerText = (await (await cells[cellindex].GetPropertyAsync("innerText")).JsonValueAsync()).ToString();

                            datarow[cellindex] = innerText;
                        }

                        SMPTable.Rows.Add(datarow);

                    }

                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return SMPTable;
        }



    }
}

using AngleSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPScraper
{
    internal class AngleSharpScraper
    {


        //AngleSharp Scraper uses the popular HTML parser AngleSharp, there are other similar tools such as HTML Agility Pack etc.
        //Advantages: Good Library for parsing HTML documents, less code, easier to read
        //Disadvantages: May not work with websites that require JavaScript or implements anti-bot detection
        public static async Task<DataTable> ScrapeData(Uri AESOSMPURL)
        {

            var SMPTable = new DataTable();

            //Get the DOM
            var Document = await BrowsingContext.New(Configuration.Default.WithDefaultLoader()).OpenAsync(AESOSMPURL.AbsoluteUri);

            //Select the table with the attribute of border = 1 which is our desired table and the only table that has such attribute
            var table = Document.QuerySelector("table[border = \"1\"]");

            //Get headers
            var headers = table.GetElementsByTagName("th");

            foreach (var column in headers)
            {
                //Get the innerText content of each header column
                SMPTable.Columns.Add(new DataColumn(column.TextContent));
            }

            //Select all rows in the table that has td as child elements so that we do not select the headers again
            var rows = table.QuerySelectorAll("tr:has(td)");

            foreach (var row in rows)
            {
                var cells = row.GetElementsByTagName("td");

                var Newrow = SMPTable.NewRow();

                for (int cellindex = 0; cellindex < cells.Length; cellindex++)
                {
                    Newrow[cellindex] = cells[cellindex].TextContent;
                }

                SMPTable.Rows.Add(Newrow);
            }

            return SMPTable;
        }



    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SMPScraper
{
    internal class HTTPClientScraper
    {
        //Simplest method without the need to add any extra nuget packages, Native HTTPClient
        //Advantages: Small size, easy to run, lightweight, does not require any extra packages
        //Disadvantages: Will not work with most modern websites that require JavaScript enabled browser or ones that implements anti-bot detection techniques
        public static async Task<DataTable> ScrapeData(Uri AESOSMPURL)
        {
            DataTable SMPTable = new DataTable();

            using (var Httpclient = new HttpClient())
            {
                // Send a HTTP GET request to the URL to request the HTML document
                var HTTPGetResponse = await Httpclient.GetAsync(AESOSMPURL);

                if (HTTPGetResponse.IsSuccessStatusCode)
                {
                    //Get the entire HTML document in string format
                    var HTMLContent = await HTTPGetResponse.Content.ReadAsStringAsync();

                    DataTable Datatable = new DataTable();

                    //Get all content inside the HTML tag, which in this case is table
                    var Tables = GetContentByHTMLTag(HTMLContent, "table");

                    //There are 4 tables on the website in total, the one we are interested in is the 3rd one
                    var PriceTable = Tables[2];

                    var Rows = GetContentByHTMLTag(PriceTable, "tr");

                    //Get headers and column names which is the first row of the table
                    var Headers = GetContentByHTMLTag(Rows.First(), "th");

                    foreach (var Column in Headers)
                    {
                        var ColumnName = GetInnerTextByElement(Column);

                        Datatable.Columns.Add(new DataColumn(ColumnName));
                    }


                    //Get row values
                    for (int Rowindex = 1; Rowindex < Rows.Count; Rowindex++)
                    {
                        var Row = Rows[Rowindex];

                        var Datarow = Datatable.NewRow();

                        var Cells = GetContentByHTMLTag(Row, "td");

                        for (int Cellindex = 0; Cellindex < Cells.Count; Cellindex++)
                        {
                            var CellValue = GetInnerTextByElement(Cells[Cellindex]);

                            Datarow[Cellindex] = CellValue;

                        }

                        Datatable.Rows.Add(Datarow);

                    }

                    SMPTable = Datatable;
                }

            }

            return SMPTable;

        }


        //Method 1 Helper method
        private static string GetInnerTextByElement(string HTMLElement)
        {

            string InnerText = string.Empty;

            try
            {
                var StartTagIndex = HTMLElement.IndexOf("<");

                var EndTagIndex = HTMLElement.IndexOf(">");

                while (StartTagIndex != -1 && EndTagIndex != -1)
                {
                    HTMLElement = HTMLElement.Remove(StartTagIndex, EndTagIndex - StartTagIndex + 1);

                    StartTagIndex = HTMLElement.IndexOf("<");

                    EndTagIndex = HTMLElement.IndexOf(">");
                }


                InnerText = HTMLElement;
            }
            catch (Exception ex)
            {

            }


            return InnerText;
        }


        //Method 1 Helper method
        //Works for AESO's SMP website and most others but could run into issues if the website has <table> or </table> in any of the element's innerText or innerHTML
        private static List<string> GetContentByHTMLTag(string HTMLDocument, string HTMLTag)
        {

            List<string> ContentList = new List<string>();

            try
            {
                var StartTag = "<" + HTMLTag;

                var EndTag = "</" + HTMLTag + ">";

                var StartingIndex = HTMLDocument.IndexOf(StartTag, StringComparison.OrdinalIgnoreCase);

                while (StartingIndex != -1)
                {
                    var ClosingIndex = HTMLDocument.IndexOf(EndTag, StringComparison.OrdinalIgnoreCase);

                    var Content = HTMLDocument.Substring(StartingIndex, ClosingIndex - StartingIndex + EndTag.Length);

                    ContentList.Add(Content);

                    HTMLDocument = HTMLDocument.Remove(StartingIndex, ClosingIndex - StartingIndex + EndTag.Length);

                    StartingIndex = HTMLDocument.IndexOf(StartTag, StringComparison.OrdinalIgnoreCase);
                }

            }
            catch (Exception ex)
            {

            }

            return ContentList;

        }


    }
}

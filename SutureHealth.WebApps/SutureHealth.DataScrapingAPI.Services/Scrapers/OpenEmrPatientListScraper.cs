
using AngleSharp.Html.Dom;

namespace SutureHealth.DataScraping.Scrapers
{
    internal class OpenEmrPatientListScraper
    {
        public IHtmlDocument HtmlDoc { get; set; }
        public ScrapedPatientList ScrapedPatients { get; set; }

        public OpenEmrPatientListScraper(IHtmlDocument htmlDoc)
        {
            HtmlDoc = htmlDoc;
            ScrapedPatients = new ScrapedPatientList();
        }

        public ScrapedPatientList Scrape()
        {           
            var tableDoc = HtmlDoc.GetElementById("pt_table")?.GetElementsByTagName("tbody");

            if (!tableDoc.IsNullOrEmpty())
            {
                var patientParameters = tableDoc[0].GetElementsByTagName("td");

                for (int i = 0; i < patientParameters.Length; i++)
                {
                    ScrapedPatientHistory scrapedPatient = new ScrapedPatientHistory(patientParameters[i].TextContent,
                                                                                 patientParameters[i + 1].TextContent,
                                                                                 patientParameters[i + 2].TextContent,
                                                                                 DateTime.Parse(patientParameters[i + 3].TextContent),
                                                                                 patientParameters[i + 4].TextContent,
                                                                                 ""); 

                    ScrapedPatients.ScrapedPatients.Add(scrapedPatient);

                    i += 4;
                }
                return ScrapedPatients;
            }
            else
            {
                return ScrapedPatients;
            }
        }       
    }
}


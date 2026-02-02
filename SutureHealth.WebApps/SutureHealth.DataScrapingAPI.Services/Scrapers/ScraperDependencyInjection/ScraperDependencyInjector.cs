
using AngleSharp.Html.Dom;

namespace SutureHealth.DataScraping.Scrapers.ScraperDependecyInjector
{
    internal class ScraperDependencyInjector
    {
        private readonly string Url;
        private readonly IHtmlDocument HtmlDocument;

        public ScraperDependencyInjector(string url, IHtmlDocument htmlDocument)
        {
            Url = url;
            HtmlDocument = htmlDocument;
        }

        public IScraper GetScraper()
        {
            if(Url=="")
            {
                return new OpenEmrPatientDetailScraper(HtmlDocument);
            }
            else
            {
                return new OpenEmrPatientDetailScraper(HtmlDocument);
            }
        }
    }
}

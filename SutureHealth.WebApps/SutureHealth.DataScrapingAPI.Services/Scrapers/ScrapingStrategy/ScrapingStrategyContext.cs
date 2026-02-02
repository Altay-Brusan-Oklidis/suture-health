namespace SutureHealth.DataScraping.Scrapers.ScrapingStrategy
{
    internal class ScrapingStrategyContext
    {
        private IScraper ScrapeStrategy { get; set; }

        public ScrapingStrategyContext()
        {
        }

        public void SetScrapingStrategy(IScraper scrapingStrategy)
        {
            ScrapeStrategy = scrapingStrategy;
        }

        //public ScrapedPatientDetail Scrape()
        //{
        //    return ScrapeStrategy.Scrape();
        //}
    }
}

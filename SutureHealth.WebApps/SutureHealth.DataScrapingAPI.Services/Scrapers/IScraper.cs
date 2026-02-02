using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.DataScraping.Scrapers
{
    public interface IScraper
    {
        ScrapedPatientDetailHistory Scrape();
    }
}

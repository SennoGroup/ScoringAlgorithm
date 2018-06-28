using System;
using System.Collections.Generic;

namespace ScoringAlgorithm
{
    public class DataSourceArticlesPerDay
    {
        /// <summary>
        /// The day when the articles were published
        /// </summary>
        public DateTime PublishingDate { get; set; }

        /// <summary>
        /// Key is a data source id and the Value is a couple of the useful and total articles
        /// </summary>
        public Dictionary<int, DataSourceArticles> DataSourceArticles { get; set; }
    }
}

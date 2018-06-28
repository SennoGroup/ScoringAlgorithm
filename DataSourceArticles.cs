namespace ScoringAlgorithm
{
    public class DataSourceArticles
    {
        /// <summary>
        /// Total number of articles, published by the data source during the specified day
        /// </summary>
        public int NumberOfArticles { get; set; }

        /// <summary>
        /// Number of articles, used for the report generation and related to the lexeme
        /// </summary>
        public int NumberOfUsefulArticles { get; set; }
    }
}

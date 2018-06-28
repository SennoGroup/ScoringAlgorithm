using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace ScoringAlgorithm
{
    /// <summary>
    /// This class is responsible for the scores account.
    /// Can be a manager or service - depending on the curent architecture of BL and 
    /// the way how it is going to be used.
    /// </summary>
    public class ScoringAlgorithm
    {
      private IConfiguration _config;

      public ScoringAlgorithm(IConfiguration config)
      {
        _config = config;
      }

        /// <summary>
        /// Count the score per each data_source/expert
        /// </summary>
        /// <param name="annualArticlesPerSource">The Dictionary of (data_source_id, data_source_articels) per each day of the past year</param>
        /// <returns>Returns weighed score per each data source</returns>
        public Dictionary<int, decimal> CountScorePerDataSource(List<DataSourceArticlesPerDay> annualArticlesPerSource)
        {
            var result = new Dictionary<int, decimal>();
            // cache the input
            annualArticlesPerSource = annualArticlesPerSource.OrderBy(aps => aps.PublishingDate).ToList();

            //get the learningSpeedScore parameter from the configuration here
            int.TryParse(_config["LearningSpeedScore"], out int learningSpeedScore);

            //each data source should be accounted, 
            //even if there were no useful articles form some data source for today or some other day
            foreach (var dataSourceId in annualArticlesPerSource.SelectMany(a => a.DataSourceArticles.Keys).Distinct())
            {
                var score = GetScorePerDataSource(dataSourceId, annualArticlesPerSource);

                //The algorithm learning score is going to be configurable. Also, in future it can differ per data source
                var dataSourceWeighedScore = score * learningSpeedScore;

                //include the counted score into the resulting dictionary
                result.Add(dataSourceId, dataSourceWeighedScore);
            }

            return result;
        }

        /// <summary>
        /// Count the Weighted arithmetic mean for the data source
        /// </summary>
        /// <param name="dataSourceId">The data_source/expert id</param>
        /// <param name="annualArticlesPerSource"></param>
        /// <returns>Returns the score for current data source for the day, specified by the index</returns>
        private decimal GetScorePerDataSource(int dataSourceId, List<DataSourceArticlesPerDay> annualArticlesPerSource)
        {
            //the default score
            var score = 0M;
            //The current day index
            var index = 0;
            //Sum of scores up to previous day
            var previousSum = 1M;

            while (true)
            {
                //get the processed articles per each data source for the day, specified by the index
                var dailyArticlesPerSource = annualArticlesPerSource[index];

                //if there were no articles during the specified day, 0 is used as a score
                if (dailyArticlesPerSource.DataSourceArticles.ContainsKey(dataSourceId))
                {
                    //get the number of processed articles from the specific data  source.
                    var dataSourceArticles = dailyArticlesPerSource.DataSourceArticles[dataSourceId];

                    //count the score by the useful articles, balances by total number of articles published by the certain data source
                    var absoluteScore = (decimal)dataSourceArticles.NumberOfUsefulArticles /
                                        dataSourceArticles.NumberOfArticles;

                    //count the score by the useful articles, balances by total number of articles published by all used data sources
                    absoluteScore = absoluteScore * dataSourceArticles.NumberOfUsefulArticles 
                                    / dailyArticlesPerSource.DataSourceArticles.Select(dsa => dsa.Value.NumberOfArticles).Sum();

                    //count the score, balanced by scores of the certain data source counted for all previous days
                    score = absoluteScore * previousSum;
                }

                //count the score for the next day of the scope
                if (index < annualArticlesPerSource.Count - 1)
                {
                    index = index + 1;
                    previousSum = previousSum + score;
                    continue;
                }

                //return the counted score for the last day of the scope
                return score;
            }
        }
    }
}

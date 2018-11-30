﻿/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using QuantConnect.Algorithm.Framework.Alphas;
using QuantConnect.Interfaces;

namespace QuantConnect
{
    /// <summary>
    /// Contains insight population run time statistics
    /// </summary>
    public class AlphaRuntimeStatistics
    {
        private DateTime _startDate;
        private double _daysCompleted;
        // this is only used when deserializing to this type since it represents a computed property dependent on internal state
        private decimal _overrideEstimatedMonthlyAlphaValue;
        private readonly IAccountCurrencyProvider _accountCurrencyProvider;

        /// <summary>
        /// Creates a new instance
        /// </summary>
        public AlphaRuntimeStatistics(IAccountCurrencyProvider accountCurrencyProvider)
        {
            _accountCurrencyProvider = accountCurrencyProvider;
        }

        /// <summary>
        /// Gets the mean scores for the entire population of insights
        /// </summary>
        public InsightScore MeanPopulationScore { get; } = new InsightScore();

        /// <summary>
        /// Gets the 100 insight ema of insight scores
        /// </summary>
        public InsightScore RollingAveragedPopulationScore { get; } = new InsightScore();

        /// <summary>
        /// Gets the total number of insights with an up direction
        /// </summary>
        public long LongCount { get; set; }

        /// <summary>
        /// Gets the total number of insights with a down direction
        /// </summary>
        public long ShortCount { get; set; }

        /// <summary>
        /// The ratio of <see cref="InsightDirection.Up"/> over <see cref="InsightDirection.Down"/>
        /// </summary>
        public decimal LongShortRatio => ShortCount == 0 ? 1m : LongCount / (decimal) ShortCount;

        /// <summary>
        /// The total accumulated estimated value of trading all insights
        /// </summary>
        public decimal TotalAccumulatedEstimatedAlphaValue { get; set; }

        /// <summary>
        /// Suggested Value of the Alpha On A Monthly Basis For Licensing
        /// </summary>
        [JsonProperty]
        public decimal EstimatedMonthlyAlphaValue
        {
            get
            {
                if (_daysCompleted == 0)
                {
                    return _overrideEstimatedMonthlyAlphaValue;
                }
                return (TotalAccumulatedEstimatedAlphaValue / (decimal) _daysCompleted) * 30;
            }
            private set { _overrideEstimatedMonthlyAlphaValue = value; }
        }

        /// <summary>
        /// The total number of insight signals generated by the algorithm
        /// </summary>
        public long TotalInsightsGenerated { get; set; }

        /// <summary>
        /// The total number of insight signals generated by the algorithm
        /// </summary>
        public long TotalInsightsClosed { get; set; }

        /// <summary>
        /// The total number of insight signals generated by the algorithm
        /// </summary>
        public long TotalInsightsAnalysisCompleted { get; set; }

        /// <summary>
        /// Gets the mean estimated insight value
        /// </summary>
        public decimal MeanPopulationEstimatedInsightValue => TotalInsightsClosed > 0 ? TotalAccumulatedEstimatedAlphaValue / TotalInsightsClosed : 0;

        /// <summary>
        /// Creates a dictionary containing the statistics
        /// </summary>
        public Dictionary<string, string> ToDictionary()
        {
            var accountCurrencySymbol = Currencies.GetCurrencySymbol(_accountCurrencyProvider.AccountCurrency);
            return new Dictionary<string, string>
            {
                {"Total Insights Generated", $"{TotalInsightsGenerated}"},
                {"Total Insights Closed", $"{TotalInsightsClosed}"},
                {"Total Insights Analysis Completed", $"{TotalInsightsAnalysisCompleted}"},
                {"Long Insight Count", $"{LongCount}"},
                {"Short Insight Count", $"{ShortCount}"},
                {"Long/Short Ratio", $"{Math.Round(100*LongShortRatio, 2)}%"},
                {"Estimated Monthly Alpha Value", $"{accountCurrencySymbol}{EstimatedMonthlyAlphaValue.SmartRounding()}"},
                {"Total Accumulated Estimated Alpha Value", $"{accountCurrencySymbol}{TotalAccumulatedEstimatedAlphaValue.SmartRounding()}"},
                {"Mean Population Estimated Insight Value", $"{accountCurrencySymbol}{MeanPopulationEstimatedInsightValue.SmartRounding()}"},
                {"Mean Population Direction", $"{Math.Round(100 * MeanPopulationScore.Direction, 4)}%"},
                {"Mean Population Magnitude", $"{Math.Round(100 * MeanPopulationScore.Magnitude, 4)}%"},
                {"Rolling Averaged Population Direction", $"{Math.Round(100 * RollingAveragedPopulationScore.Direction, 4)}%"},
                {"Rolling Averaged Population Magnitude", $"{Math.Round(100 * RollingAveragedPopulationScore.Magnitude, 4)}%"},
            };
        }

        /// <summary>
        /// Set the current date of the backtest
        /// </summary>
        /// <param name="now"></param>
        public void SetDate(DateTime now)
        {
            _daysCompleted = (now - _startDate).TotalDays;
        }

        /// <summary>
        /// Set the date range of the statistics
        /// </summary>
        /// <param name="algorithmStartDate"></param>
        public void SetStartDate(DateTime algorithmStartDate)
        {
            _startDate = algorithmStartDate;
        }
    }
}
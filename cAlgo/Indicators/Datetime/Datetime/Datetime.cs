using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Datetime : Indicator
    {
        [Parameter(DefaultValue = 0.0)]
        public double Parameter { get; set; }

        [Output("Main")]
        public IndicatorDataSeries Result { get; set; }


        protected override void Initialize()
        {
            // Initialize and create nested indicators
        }

        public override void Calculate(int index)
        {
            // Calculate value at specified index
            // Result[index] = ...
            if (index == 0)
                return;

            DateTime currentTime = MarketSeries.OpenTime[index];
            DateTime previousTime = MarketSeries.OpenTime[index - 1];

            if (currentTime.Month != previousTime.Month)
            {
                // first bar of the month  
                Print(currentTime);
            }
        }
    }
}

using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

namespace cAlgo
{
    [Indicator("HeikenMA", IsOverlay = true, TimeZone = TimeZones.UTC, AutoRescale = false, AccessRights = AccessRights.None)]
    public class HeikenMAind : Indicator
    {
        private MovingAverage smaOpen;
        private MovingAverage smaClose;
        private MovingAverage smaLow;
        private MovingAverage smaHigh;

        [Parameter()]
        public DataSeries Source { get; set; }

        [Parameter("MA Type", DefaultValue = MovingAverageType.Exponential)]
        public MovingAverageType MAType { get; set; }

        [Parameter(DefaultValue = 5, MinValue = 1)]
        public int Periods { get; set; }

        [Output("Main", LineColor = "Turquoise")]
        public IndicatorDataSeries Result { get; set; }
        [Output("haHigh", LineColor = "Green")]
        public IndicatorDataSeries haHigh { get; set; }
        [Output("haLow", LineColor = "Red")]
        public IndicatorDataSeries haLow { get; set; }
        [Output("haOpen", LineColor = "Yellow")]
        public IndicatorDataSeries haOpen { get; set; }
        [Output("haClose", LineColor = "Purple")]
        public IndicatorDataSeries haClose { get; set; }



        protected override void Initialize()
        {
            // Initialize and create nested indicators
            smaOpen = Indicators.MovingAverage(MarketSeries.Open, Periods, MAType);
            smaClose = Indicators.MovingAverage(MarketSeries.Close, Periods, MAType);
            smaLow = Indicators.MovingAverage(MarketSeries.Low, Periods, MAType);
            smaHigh = Indicators.MovingAverage(MarketSeries.High, Periods, MAType);
            //haOpen = CreateDataSeries();
            //haClose = CreateDataSeries();
        }

        public override void Calculate(int index)
        {
            // Calculate value at specified index
            double haHighd;
            double haLowd;
            double sum = 0.0;

            for (int i = index - Periods + 1; i <= index; i++)
            {
                sum += Source[i];
            }
            // Result[index] = ...
            Result[index] = sum / Periods;

            if (index > 0 && !double.IsNaN(smaOpen.Result[index - 1]))
            {
                haOpen[index] = (smaOpen.Result[index - 1] + smaClose.Result[index - 1]) / 2;
                haClose[index] = (smaOpen.Result[index] + smaHigh.Result[index] + smaLow.Result[index] + smaClose.Result[index]) / 4;
                haHigh[index] = Math.Max(smaHigh.Result[index], Math.Max(haOpen.LastValue, haClose.LastValue));
                haLow[index] = Math.Min(smaLow.Result[index], Math.Min(haOpen.LastValue, haClose.LastValue));
            }
            else if (!double.IsNaN(smaOpen.Result[index]))
            {
                haOpen[index] = (smaOpen.Result[index] + smaClose.Result[index]) / 2;
                haClose[index] = (smaOpen.Result[index] + smaClose.Result[index] + smaHigh.Result[index] + smaLow.Result[index]) / 4;
                //haHighd = smaHigh.Result[index];
                //haLowd = smaLow.Result[index];
            }

        }

    }
}





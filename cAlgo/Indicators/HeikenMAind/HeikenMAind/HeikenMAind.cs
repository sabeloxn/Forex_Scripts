using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

namespace cAlgo
{
    [Indicator( IsOverlay = true, TimeZone = TimeZones.UTC, AutoRescale = false, AccessRights = AccessRights.None)]
    public class HeikenMAind : Indicator
    {
        private MovingAverage smaOpen;
        private MovingAverage smaClose;
        private MovingAverage smaLow;
        private MovingAverage smaHigh;

        [Parameter("EMA Timeframe1", DefaultValue = "Minute15")]

        public TimeFrame EMATimeframe1 { get; set; }
        [Parameter()]
        public DataSeries Source { get; set; }

        [Parameter("MA Type", DefaultValue = MovingAverageType.Exponential)]
        public MovingAverageType MAType { get; set; }

        [Parameter(DefaultValue = 5, MinValue = 1)]
        public int Periods { get; set; }

        [Output("EMA1", LineColor = "Blue")]
        public IndicatorDataSeries EMA1 { get; set; }

        [Output("Main", LineColor = "Orange")]
        public IndicatorDataSeries Result { get; set; }
        [Output("haHigh", LineColor = "Green")]
        public IndicatorDataSeries haHigh { get; set; }
        [Output("haLow", LineColor = "Red")]
        public IndicatorDataSeries haLow { get; set; }
        [Output("haOpen", LineColor = "Yellow")]
        public IndicatorDataSeries haOpen { get; set; }
        [Output("haClose", LineColor = "Purple")]
        public IndicatorDataSeries haClose { get; set; }


        private Bars series1;
        private ExponentialMovingAverage Ema1;

        protected override void Initialize()
        {
            // Initialize and create nested indicators
            smaOpen = Indicators.MovingAverage(Bars.OpenPrices, Periods, MAType);
            smaClose = Indicators.MovingAverage(Bars.ClosePrices, Periods, MAType);
            smaLow = Indicators.MovingAverage(Bars.LowPrices, Periods, MAType);
            smaHigh = Indicators.MovingAverage(Bars.HighPrices, Periods, MAType);

            series1 = MarketData.GetBars(EMATimeframe1);

            Ema1 = Indicators.ExponentialMovingAverage(series1.ClosePrices, Periods);

        }

        public override void Calculate(int index)
        {
            // Calculate value at specified index

            var index1 = GetIndexByDate(series1, Bars.OpenTimes[index]);
            if (index1 != -1)
            {
                EMA1[index] = Ema1.Result[index1];
            }

            double sum = 0.0;

            for (int i = index - Periods + 1; i <= index; i++)
            {
                sum += Source[i];
            }
            // Result[index] = ...
            Result[index] = sum / Periods;
            haOpen[index] = (smaOpen.Result[index - 1] + smaClose.Result[index - 1]) / 2;
            haClose[index] = (smaOpen.Result[index] + smaHigh.Result[index] + smaLow.Result[index] + smaClose.Result[index]) / 4;
            haHigh[index] = Math.Max(smaHigh.Result[index], Math.Max(haOpen.LastValue, haClose.LastValue));
            haLow[index] = Math.Min(smaLow.Result[index], Math.Min(haOpen.LastValue, haClose.LastValue));
        }

        private int GetIndexByDate(Bars series, DateTime time)
        {
            for (int i = series.ClosePrices.Count - 1; i > 0; i--)
            {
                if (time == series.OpenTimes[i])
                    return i;
            }
            return -1;

        }

    }
}





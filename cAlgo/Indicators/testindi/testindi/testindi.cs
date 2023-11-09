using System;
using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = true, AccessRights = AccessRights.None)]
    public class testindi : Indicator
    {
        [Parameter(DefaultValue = 40, MinValue = 1)]
        public int Periods { get; set; }

        [Parameter("MA Type", DefaultValue = MovingAverageType.Exponential)]
        public MovingAverageType MAType { get; set; }

        public MovingAverage maOpen;
        public MovingAverage maClose;
        public MovingAverage maHigh;
        public MovingAverage maLow;
        public IndicatorDataSeries haClose;
        public IndicatorDataSeries haOpen;

        protected override void Initialize()
        {
            maOpen = Indicators.MovingAverage(Bars.OpenPrices, Periods, MAType);
            maClose = Indicators.MovingAverage(Bars.ClosePrices, Periods, MAType);
            maHigh = Indicators.MovingAverage(Bars.HighPrices, Periods, MAType);
            maLow = Indicators.MovingAverage(Bars.LowPrices, Periods, MAType);
            haOpen = CreateDataSeries();
            haClose = CreateDataSeries();

        }

        public override void Calculate(int index)
        {
            double haHigh;
            double haLow;

            if (index > 0 && !double.IsNaN(maOpen.Result[index - 1]))
            {
                haOpen[index] = (haOpen[index - 1] + haClose[index - 1]) / 2;
                haClose[index] = (maOpen.Result[index] + maClose.Result[index] + maHigh.Result[index] + maLow.Result[index]) / 4;
                haHigh = Math.Max(maHigh.Result[index], Math.Max(haOpen[index], haClose[index]));
                haLow = Math.Min(maLow.Result[index], Math.Min(haOpen[index], haClose[index]));
            }
            else if (!double.IsNaN(maOpen.Result[index]))
            {
                haOpen[index] = (maOpen.Result[index] + maClose.Result[index]) / 2;
                haClose[index] = (maOpen.Result[index] + maClose.Result[index] + maHigh.Result[index] + maLow.Result[index]) / 4;
                haHigh = maHigh.Result[index];
                haLow = maLow.Result[index];
            }

        }
    }
}

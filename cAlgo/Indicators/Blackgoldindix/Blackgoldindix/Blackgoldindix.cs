using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = true, AccessRights = AccessRights.None, AutoRescale = false)]
    public class BlackgoldIndicatorx : Indicator
    {


        [Parameter(DefaultValue = 17, MinValue = 1)]
        public int Periods { get; set; }
        [Parameter(DefaultValue = 17, MinValue = 1)]
        public int stfPeriods { get; set; }
        [Parameter("MA Type", DefaultValue = MovingAverageType.Simple)]
        public MovingAverageType MAType { get; set; }

        [Parameter("STF", DefaultValue = "Minute15")]
        public TimeFrame STF { get; set; }

        public MarketSeries stfSeries;

        public MovingAverage maOpen;
        public MovingAverage maClose;
        public MovingAverage maHigh;
        public MovingAverage maLow;

        public MovingAverage stfmaOpen;
        public MovingAverage stfmaClose;
        public MovingAverage stfmaHigh;
        public MovingAverage stfmaLow;

        public IndicatorDataSeries haClose;
        public IndicatorDataSeries haOpen;
        public IndicatorDataSeries stfhaClose;
        public IndicatorDataSeries stfhaOpen;

        [Output("High", LineColor = "Green")]
        public IndicatorDataSeries haHighOutput { get; set; }
        [Output("Low", LineColor = "Red")]
        public IndicatorDataSeries haLowOutput { get; set; }
        [Output("Open", LineColor = "Yellow")]
        public IndicatorDataSeries haOpenOutput { get; set; }
        [Output("Close", LineColor = "Purple")]
        public IndicatorDataSeries haCloseOutput { get; set; }

        [Output("stfHigh", LineColor = "Green")]
        public IndicatorDataSeries stfhaHighOutput { get; set; }
        [Output("stfLow", LineColor = "Red")]
        public IndicatorDataSeries stfhaLowOutput { get; set; }
        [Output("stfOpen", LineColor = "Yellow")]
        public IndicatorDataSeries stfhaOpenOutput { get; set; }
        [Output("stfClose", LineColor = "Purple")]
        public IndicatorDataSeries stfhaCloseOutput { get; set; }


        protected override void Initialize()
        {
            stfSeries = MarketData.GetSeries(STF);

            maOpen = Indicators.MovingAverage(MarketSeries.Open, Periods, MAType);
            maClose = Indicators.MovingAverage(MarketSeries.Close, Periods, MAType);
            maHigh = Indicators.MovingAverage(MarketSeries.High, Periods, MAType);
            maLow = Indicators.MovingAverage(MarketSeries.Low, Periods, MAType);

            stfmaOpen = Indicators.MovingAverage(stfSeries.Open, stfPeriods, MAType);
            stfmaClose = Indicators.MovingAverage(stfSeries.Close, stfPeriods, MAType);
            stfmaHigh = Indicators.MovingAverage(stfSeries.High, stfPeriods, MAType);
            stfmaLow = Indicators.MovingAverage(stfSeries.Low, stfPeriods, MAType);

            haOpen = CreateDataSeries();
            haClose = CreateDataSeries();

            stfhaOpen = CreateDataSeries();
            stfhaClose = CreateDataSeries();
        }

        public override void Calculate(int index)
        {
            var stfindex = GetIndexByDate(stfSeries, MarketSeries.OpenTime[index]);
            double haHigh;
            double haLow;
            double stfhaHigh;
            double stfhaLow;

            if (index > 0 && !double.IsNaN(maOpen.Result[index - 1]))
            {
                haOpen[index] = (haOpen[index - 1] + haClose[index - 1]) / 2;
                haClose[index] = (maOpen.Result[index] + maClose.Result[index] + maHigh.Result[index] + maLow.Result[index]) / 4;
                haHigh = Math.Max(maHigh.Result[index], Math.Max(haOpen[index], haClose[index]));
                haLow = Math.Min(maLow.Result[index], Math.Min(haOpen[index], haClose[index]));

                if (stfindex != -1)
                {
                    stfhaOpen[index] = (stfhaOpen[stfindex - 1] + stfhaClose[stfindex - 1]) / 2;
                    stfhaClose[index] = (stfmaOpen.Result[stfindex] + stfmaClose.Result[stfindex] + stfmaHigh.Result[stfindex] + stfmaLow.Result[stfindex]) / 4;
                    stfhaHigh = Math.Max(stfmaHigh.Result[stfindex], Math.Max(stfhaOpen[stfindex], stfhaClose[stfindex]));
                    stfhaLow = Math.Min(stfmaLow.Result[stfindex], Math.Min(stfhaOpen[stfindex], stfhaClose[stfindex]));
                }
            }
            else if (!double.IsNaN(maOpen.Result[index]))
            {
                haOpen[index] = (maOpen.Result[index] + maClose.Result[index]) / 2;
                haClose[index] = (maOpen.Result[index] + maClose.Result[index] + maHigh.Result[index] + maLow.Result[index]) / 4;
                haHigh = maHigh.Result[index];
                haLow = maLow.Result[index];

                if (stfindex != -1)
                {
                    stfhaOpen[index] = (stfmaOpen.Result[stfindex] + stfmaClose.Result[stfindex]) / 2;
                    stfhaClose[index] = (stfmaOpen.Result[stfindex] + stfmaClose.Result[stfindex] + stfmaHigh.Result[stfindex] + stfmaLow.Result[stfindex]) / 4;
                    stfhaHigh = stfmaHigh.Result[stfindex];
                    stfhaLow = stfmaLow.Result[stfindex];
                }
            }

            haOpenOutput[index] = (maOpen.Result[index - 1] + maClose.Result[index - 1]) / 2;
            haCloseOutput[index] = (maOpen.Result[index] + maHigh.Result[index] + maLow.Result[index] + maClose.Result[index]) / 4;
            haHighOutput[index] = Math.Max(maHigh.Result[index], Math.Max(haOpenOutput.LastValue, haCloseOutput.LastValue));
            haLowOutput[index] = Math.Min(maLow.Result[index], Math.Min(haOpenOutput.LastValue, haCloseOutput.LastValue));

            stfhaOpenOutput[index] = (stfmaOpen.Result[stfindex - 1] + stfmaClose.Result[stfindex - 1]) / 2;
            stfhaCloseOutput[index] = (stfmaOpen.Result[stfindex] + stfmaHigh.Result[stfindex] + stfmaLow.Result[stfindex] + stfmaClose.Result[stfindex]) / 4;
            stfhaHighOutput[index] = Math.Max(stfmaHigh.Result[stfindex], Math.Max(stfhaOpenOutput.LastValue, stfhaCloseOutput.LastValue));
            stfhaLowOutput[index] = Math.Min(stfmaLow.Result[stfindex], Math.Min(stfhaOpenOutput.LastValue, stfhaCloseOutput.LastValue));
        }

        private int GetIndexByDate(MarketSeries series, DateTime time)
        {
            for (int i = series.Close.Count - 1; i > 0; i--)
            {
                if (time == series.OpenTime[i])
                    return i;
            }
            return -1;
        }
    }
}

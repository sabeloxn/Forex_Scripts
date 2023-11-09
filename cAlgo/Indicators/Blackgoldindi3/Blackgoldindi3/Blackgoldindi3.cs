using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = true, AccessRights = AccessRights.None, AutoRescale = false)]
    public class Blackgoldindi3 : Indicator
    {
        [Parameter("ChartTF", DefaultValue = true)]
        public bool ChartTF { get; set; }
        [Parameter(DefaultValue = 18, MinValue = 1)]
        public int Periods { get; set; }
        [Parameter("MA Type", DefaultValue = MovingAverageType.Exponential)]
        public MovingAverageType MAType { get; set; }

//-----------------------------------------------STF Parameters-------------------------------------------------------------------------------
        [Parameter("SHATF Enable", DefaultValue = true)]
        public bool STFenabled { get; set; }
        [Parameter("SHA Timeframe", DefaultValue = "Hour4")]
        public TimeFrame SHATF { get; set; }
        [Parameter("SHA Bars Timeframe", DefaultValue = "Minute30")]
        public TimeFrame SHAbarsTF { get; set; }
        [Parameter("SHA MA Type", DefaultValue = MovingAverageType.Exponential)]
        public MovingAverageType SHAMAType { get; set; }
        [Parameter("SHA Periods", DefaultValue = 12, MinValue = 1)]
        public int stfPeriods { get; set; }

        [Output("stfHigh", LineColor = "Green")]
        public IndicatorDataSeries SHAHighOutput { get; set; }
        [Output("stfLow", LineColor = "Red")]
        public IndicatorDataSeries SHALowOutput { get; set; }
        [Output("stfOpen", LineColor = "Yellow")]
        public IndicatorDataSeries SHAOpenOutput { get; set; }
        [Output("stfClose", LineColor = "Purple")]
        public IndicatorDataSeries SHACloseOutput { get; set; }

        public MarketSeries SHAseries;
        public MarketSeries SHAbars;
        public MovingAverage SHAmaOpen;
        public MovingAverage SHAmaClose;
        public MovingAverage SHAmaHigh;
        public MovingAverage SHAmaLow;
        public IndicatorDataSeries SHAClose;
        public IndicatorDataSeries SHAOpen;
        public IndicatorDataSeries SHAbarsClose;
        public IndicatorDataSeries SHAbarsOpen;
//--------------------------------------------------------------------------------------------------------------------------------------------
        public MarketSeries series;
        public MovingAverage maOpen;
        public MovingAverage maClose;
        public MovingAverage maHigh;
        public MovingAverage maLow;
        public IndicatorDataSeries haClose;
        public IndicatorDataSeries haOpen;
        public double Open;
        public double Close;
        public double High;
        public double Low;
        [Output("High", LineColor = "Green")]
        public IndicatorDataSeries haHighOutput { get; set; }
        [Output("Low", LineColor = "Red")]
        public IndicatorDataSeries haLowOutput { get; set; }
        [Output("Open", LineColor = "Yellow")]
        public IndicatorDataSeries haOpenOutput { get; set; }
        [Output("Close", LineColor = "Purple")]
        public IndicatorDataSeries haCloseOutput { get; set; }
//============================================================================================================================================

        protected override void Initialize()
        {
            series = MarketData.GetSeries(TimeFrame);
            SHAseries = MarketData.GetSeries(SHATF);
            SHAbars = MarketData.GetSeries(SHAbarsTF);

            maOpen = Indicators.MovingAverage(MarketSeries.Open, Periods, MAType);
            maClose = Indicators.MovingAverage(MarketSeries.Close, Periods, MAType);
            maHigh = Indicators.MovingAverage(MarketSeries.High, Periods, MAType);
            maLow = Indicators.MovingAverage(MarketSeries.Low, Periods, MAType);

            Open = series.Open.LastValue;
            Close = series.Close.LastValue;
            High = series.Close.LastValue;
            Low = series.Low.LastValue;

            SHAmaOpen = Indicators.MovingAverage(SHAseries.Open, stfPeriods, MAType);
            SHAmaClose = Indicators.MovingAverage(SHAseries.Close, stfPeriods, MAType);
            SHAmaHigh = Indicators.MovingAverage(SHAseries.High, stfPeriods, MAType);
            SHAmaLow = Indicators.MovingAverage(SHAseries.Low, stfPeriods, MAType);

            haOpen = CreateDataSeries();
            haClose = CreateDataSeries();

            SHAOpen = CreateDataSeries();
            SHAClose = CreateDataSeries();

            SHAbarsOpen = CreateDataSeries();
            SHAbarsClose = CreateDataSeries();
        }

        public override void Calculate(int index)
        {
            var SHAindex = GetIndexByDate(SHAseries, MarketSeries.OpenTime[index]);
            var SHABarsindex = GetIndexByDate(SHAbars, MarketSeries.OpenTime[index]);
            double haHigh;
            double haLow;
            double SHAHigh;
            double SHALow;
            //------------------------------------STF--------------------------------------------------------------------------------
            if (STFenabled == true)
            {
                if (SHAindex > 0 && !double.IsNaN(SHAmaOpen.Result[SHAindex - 1]))
                {
                    if (SHAindex != -1)
                    {
                        SHAOpen[index] = (SHAOpen[SHAindex - 1] + SHAClose[SHAindex - 1]) / 2;
                        SHAClose[index] = (SHAmaOpen.Result[SHAindex] + SHAmaClose.Result[SHAindex] + SHAmaHigh.Result[SHAindex] + SHAmaLow.Result[SHAindex]) / 4;
                        SHAHigh = Math.Max(SHAmaHigh.Result[SHAindex], Math.Max(SHAOpen[SHAindex], SHAClose[SHAindex]));
                        SHALow = Math.Min(SHAmaLow.Result[SHAindex], Math.Min(SHAOpen[SHAindex], SHAClose[SHAindex]));
                    }
                }
                else if (!double.IsNaN(SHAmaOpen.Result[SHAindex]))
                {
                    if (SHAindex != -1)
                    {
                        SHAOpen[index] = (SHAmaOpen.Result[SHAindex] + SHAmaClose.Result[SHAindex]) / 2;
                        SHAClose[index] = (SHAmaOpen.Result[SHAindex] + SHAmaClose.Result[SHAindex] + SHAmaHigh.Result[SHAindex] + SHAmaLow.Result[SHAindex]) / 4;
                        SHAHigh = SHAmaHigh.Result[SHAindex];
                        SHALow = SHAmaLow.Result[SHAindex];
                    }
                }
                SHAOpenOutput[index] = (SHAmaOpen.Result[SHAindex - 1] + SHAmaClose.Result[SHAindex - 1]) / 2;
                SHACloseOutput[index] = (SHAmaOpen.Result[SHAindex] + SHAmaHigh.Result[SHAindex] + SHAmaLow.Result[SHAindex] + SHAmaClose.Result[SHAindex]) / 4;
                SHAHighOutput[index] = Math.Max(SHAmaHigh.Result[SHAindex], Math.Max(SHAOpenOutput.LastValue, SHACloseOutput.LastValue));
                SHALowOutput[index] = Math.Min(SHAmaLow.Result[SHAindex], Math.Min(SHAOpenOutput.LastValue, SHACloseOutput.LastValue));
            }

            //-----------------------------------------END STF-------------------------------------------------------------------------------
            if (ChartTF == true)
            {
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
                haOpenOutput[index] = (maOpen.Result[index - 1] + maClose.Result[index - 1]) / 2;
                haCloseOutput[index] = (maOpen.Result[index] + maHigh.Result[index] + maLow.Result[index] + maClose.Result[index]) / 4;
                haHighOutput[index] = Math.Max(maHigh.Result[index], Math.Max(haOpenOutput.LastValue, haCloseOutput.LastValue));
                haLowOutput[index] = Math.Min(maLow.Result[index], Math.Min(haOpenOutput.LastValue, haCloseOutput.LastValue));
            }
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

using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = true, AccessRights = AccessRights.None, AutoRescale = false)]
    public class Blackgoldindi2 : Indicator
    {
        [Parameter("xLot", DefaultValue = 1, MinValue = 1)]
        public double xLot { get; set; }
        [Parameter("xStop", DefaultValue = 1, MinValue = 1)]
        public double xStop { get; set; }
        [Parameter(DefaultValue = 2, MinValue = 2)]
        public int qdp { get; set; }
        [Parameter(DefaultValue = 18, MinValue = 1)]
        public int Periods { get; set; }
        [Parameter("MA Type", DefaultValue = MovingAverageType.Exponential)]
        public MovingAverageType MAType { get; set; }
        [Parameter("Stop Buffer", DefaultValue = 0.3, MinValue = 0.01)]
        public double stopBuffer { get; set; }
        [Parameter("Volatility Threshold ", DefaultValue = 0.3, MinValue = 0.01)]
        public double volatilityThreshold { get; set; }
        [Parameter("longBuffer", DefaultValue = 0.1, MinValue = 0.01)]
        public double longBuffer { get; set; }
        [Parameter("ShortBuffer", DefaultValue = 0.1, MinValue = 0.01)]
        public double shortBuffer { get; set; }
        [Parameter("Stop Loss", DefaultValue = 1, MinValue = 1)]
        public double stopLoss { get; set; }
        [Parameter("Take Profit", DefaultValue = 1, MinValue = 1)]
        public double takeProfit { get; set; }

        /*[Output("Up Fractal", Color = Colors.Red, PlotType = PlotType.Points, Thickness = 5)]
        public IndicatorDataSeries UpFractal { get; set; }
        [Output("Down Fractal", Color = Colors.Blue, PlotType = PlotType.Points, Thickness = 5)]
        public IndicatorDataSeries DownFractal { get; set; }*/
//-----------------------------------------------STF Parameters-------------------------------------------------------------------------------
        [Parameter("Enable STF", DefaultValue = true)]
        public bool STFenabled { get; set; }
        [Parameter("STF", DefaultValue = "Weekly")]
        public TimeFrame STF { get; set; }
        [Parameter("stfPeriods", DefaultValue = 5, MinValue = 1)]
        public int stfPeriods { get; set; }
        [Parameter("stfVolatility Threshold", DefaultValue = 0.3, MinValue = 0.1)]
        public double stfvolatilityThreshold { get; set; }
        [Parameter("stfStop Buffer", DefaultValue = 0.3, MinValue = 0.1)]
        public double stfstopBuffer { get; set; }

        [Output("stfHigh", LineColor = "Gold")]
        public IndicatorDataSeries stfhaHighOutput { get; set; }
        [Output("stfLow", LineColor = "Gold")]
        public IndicatorDataSeries stfhaLowOutput { get; set; }
        [Output("stfOpen", LineColor = "Yellow")]
        public IndicatorDataSeries stfhaOpenOutput { get; set; }
        [Output("stfClose", LineColor = "Purple")]
        public IndicatorDataSeries stfhaCloseOutput { get; set; }
        [Parameter("stfLongBuffer", DefaultValue = 0.1, MinValue = 0.01)]
        public double stflongBuffer { get; set; }
        [Parameter("stfShortBuffer", DefaultValue = 0.1, MinValue = 0.01)]
        public double stfshortBuffer { get; set; }
        public MarketSeries stfSeries;
        public MarketSeries series;
        public MovingAverage stfmaOpen;
        public MovingAverage stfmaClose;
        public MovingAverage stfmaHigh;
        public MovingAverage stfmaLow;
        public IndicatorDataSeries stfhaClose;
        public IndicatorDataSeries stfhaOpen;
//----------------------------------------------------------RANGE PARAMETERS------------------------------------------------------------------
        [Parameter("Range enabled")]
        public bool range { get; set; }
//--------------------------------------------------------------------------------------------------------------------------------------------
        public bool hasCrossedAbove;
        public bool hasCrossedBelow;
        public bool activeLong;
        public bool activeShort;

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
        public MarketSeries dailySeries;
        protected override void Initialize()
        {
            series = MarketData.GetSeries(STF);
            //dailySeries = MarketData.GetSeries();

            maOpen = Indicators.MovingAverage(series.Open, Periods, MAType);
            maClose = Indicators.MovingAverage(series.Close, Periods, MAType);
            maHigh = Indicators.MovingAverage(series.High, Periods, MAType);
            maLow = Indicators.MovingAverage(series.Low, Periods, MAType);

            Open = series.Open.LastValue;
            Close = series.Close.LastValue;
            High = series.Close.LastValue;
            Low = series.Low.LastValue;

            haOpen = CreateDataSeries();
            haClose = CreateDataSeries();

        }

        public override void Calculate(int index)
        {
            double haHigh;
            double haLow;
            //var stfindex = GetIndexByDate(stfSeries, MarketSeries.OpenTime[index]);
            var myindex = GetIndexByDate(series, MarketSeries.OpenTime[index]);

            if (index > 0 && !double.IsNaN(maOpen.Result[index - 1]))
            {
                haOpen[index] = (haOpen[myindex - 1] + haClose[myindex - 1]) / 2;
                haClose[index] = (maOpen.Result[myindex] + maClose.Result[myindex] + maHigh.Result[myindex] + maLow.Result[myindex]) / 4;
                haHigh = Math.Max(maHigh.Result[myindex], Math.Max(haOpen[myindex], haClose[myindex]));
                haLow = Math.Min(maLow.Result[myindex], Math.Min(haOpen[myindex], haClose[myindex]));
            }
            else if (!double.IsNaN(maOpen.Result[index]))
            {
                haOpen[index] = (maOpen.Result[myindex] + maClose.Result[myindex]) / 2;
                haClose[index] = (maOpen.Result[myindex] + maClose.Result[myindex] + maHigh.Result[myindex] + maLow.Result[myindex]) / 4;
                haHigh = maHigh.Result[myindex];
                haLow = maLow.Result[myindex];
            }
            haOpenOutput[index] = (maOpen.Result[myindex - 1] + maClose.Result[myindex - 1]) / 2;
            haCloseOutput[index] = (maOpen.Result[myindex] + maHigh.Result[myindex] + maLow.Result[myindex] + maClose.Result[myindex]) / 4;
            haHighOutput[index] = Math.Max(maHigh.Result[myindex], Math.Max(haOpenOutput.LastValue, haCloseOutput.LastValue));
            haLowOutput[index] = Math.Min(maLow.Result[myindex], Math.Min(haOpenOutput.LastValue, haCloseOutput.LastValue));
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






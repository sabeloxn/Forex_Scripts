using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = true, AccessRights = AccessRights.None, AutoRescale = false)]
    public class BlackgoldIndicator : Indicator
    {
        [Parameter(DefaultValue = 1, MinValue = 1)]
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

        [Output("stfHigh", LineColor = "Green")]
        public IndicatorDataSeries stfhaHighOutput { get; set; }
        [Output("stfLow", LineColor = "Red")]
        public IndicatorDataSeries stfhaLowOutput { get; set; }
        [Output("stfOpen", LineColor = "Yellow")]
        public IndicatorDataSeries stfhaOpenOutput { get; set; }
        [Output("stfClose", LineColor = "Purple")]
        public IndicatorDataSeries stfhaCloseOutput { get; set; }
        [Parameter("stfLongBuffer", DefaultValue = 0.1, MinValue = 0.01)]
        public double stflongBuffer { get; set; }
        [Parameter("stfShortBuffer", DefaultValue = 0.1, MinValue = 0.01)]
        public double stfshortBuffer { get; set; }
        public Bars stfSeries;
        public Bars series;
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
        private double LotSize;
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
        public Bars dailySeries;
        protected override void Initialize()
        {
            series = MarketData.GetBars(TimeFrame);
            stfSeries = MarketData.GetBars(STF);
            dailySeries = MarketData.GetBars(TimeFrame.Daily);

            maOpen = Indicators.MovingAverage(Bars.OpenPrices, Periods, MAType);
            maClose = Indicators.MovingAverage(Bars.ClosePrices, Periods, MAType);
            maHigh = Indicators.MovingAverage(Bars.HighPrices, Periods, MAType);
            maLow = Indicators.MovingAverage(Bars.LowPrices, Periods, MAType);

            Open = series.OpenPrices.LastValue;
            Close = series.ClosePrices.LastValue;
            High = series.ClosePrices.LastValue;
            Low = series.LowPrices.LastValue;

            stfmaOpen = Indicators.MovingAverage(stfSeries.OpenPrices, stfPeriods, MAType);
            stfmaClose = Indicators.MovingAverage(stfSeries.ClosePrices, stfPeriods, MAType);
            stfmaHigh = Indicators.MovingAverage(stfSeries.HighPrices, stfPeriods, MAType);
            stfmaLow = Indicators.MovingAverage(stfSeries.LowPrices, stfPeriods, MAType);

            haOpen = CreateDataSeries();
            haClose = CreateDataSeries();

            stfhaOpen = CreateDataSeries();
            stfhaClose = CreateDataSeries();

            string myChart = Symbol.Name;
            double myPipvalue = Symbol.PipSize;
            double myBalance = Account.Balance;
            hasCrossedAbove = true;
            hasCrossedBelow = true;
            activeLong = false;
            activeShort = false;
        }

        public override void Calculate(int index)
        {
            var stfindex = GetIndexByDate(stfSeries, Bars.OpenTimes[index]);
            double haHigh;
            double haLow;
            double stfhaHigh;
            double stfhaLow;
            //------------------------------------STF--------------------------------------------------------------------------------
            if (STFenabled == true)
            {
                if (stfindex > 0 && !double.IsNaN(stfmaOpen.Result[stfindex - 1]))
                {
                    if (stfindex != -1)
                    {
                        stfhaOpen[index] = (stfhaOpen[stfindex - 1] + stfhaClose[stfindex - 1]) / 2;
                        stfhaClose[index] = (stfmaOpen.Result[stfindex] + stfmaClose.Result[stfindex] + stfmaHigh.Result[stfindex] + stfmaLow.Result[stfindex]) / 4;
                        stfhaHigh = Math.Max(stfmaHigh.Result[stfindex], Math.Max(stfhaOpen[stfindex], stfhaClose[stfindex]));
                        stfhaLow = Math.Min(stfmaLow.Result[stfindex], Math.Min(stfhaOpen[stfindex], stfhaClose[stfindex]));
                    }
                }
                else if (!double.IsNaN(stfmaOpen.Result[stfindex]))
                {
                    if (stfindex != -1)
                    {
                        stfhaOpen[index] = (stfmaOpen.Result[stfindex] + stfmaClose.Result[stfindex]) / 2;
                        stfhaClose[index] = (stfmaOpen.Result[stfindex] + stfmaClose.Result[stfindex] + stfmaHigh.Result[stfindex] + stfmaLow.Result[stfindex]) / 4;
                        stfhaHigh = stfmaHigh.Result[stfindex];
                        stfhaLow = stfmaLow.Result[stfindex];
                    }
                }
                stfhaOpenOutput[index] = (stfmaOpen.Result[stfindex - 1] + stfmaClose.Result[stfindex - 1]) / 2;
                stfhaCloseOutput[index] = (stfmaOpen.Result[stfindex] + stfmaHigh.Result[stfindex] + stfmaLow.Result[stfindex] + stfmaClose.Result[stfindex]) / 4;
                stfhaHighOutput[index] = Math.Max(stfmaHigh.Result[stfindex], Math.Max(stfhaOpenOutput.LastValue, stfhaCloseOutput.LastValue));
                stfhaLowOutput[index] = Math.Min(stfmaLow.Result[stfindex], Math.Min(stfhaOpenOutput.LastValue, stfhaCloseOutput.LastValue));
            }

            //-----------------------------------------END STF-------------------------------------------------------------------------------
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
//------------------------On Bar---------------------------------------------------------------------------------------------------         
            DateTime today = Bars.OpenTimes[index].Date;
            DateTime tomorrow = today.AddDays(1);

            var previousClose = Bars.ClosePrices.LastValue;
            var previousHigh = Bars.ClosePrices.LastValue;
            var previousLow = Bars.ClosePrices.LastValue;

            var smaOpen = maOpen.Result[index];
            var smaClose = maClose.Result[index];
            var smaHigh = maHigh.Result[index];
            var smaLow = maLow.Result[index];

            haHigh = Math.Round(maHigh.Result[index], qdp);
            haLow = Math.Round(maLow.Result[index], qdp);

            //double LongStopLoss = Math.Round(haLow, qdp);
            //double ShortStopLoss = Math.Round(Math.Abs(previousClose - haHigh) * 100, qdp);
            double LongStopLoss = Math.Round(Math.Abs(previousClose % haLow) * xStop, qdp);
            double ShortStopLoss = Math.Round(Math.Abs(haHigh % previousClose) * xStop, qdp);

            double volatility = Math.Round(haHigh - haLow, qdp);
            double Risk = Math.Ceiling(Account.Balance * 10) / 100;
            double Lot = Convert.ToDouble(Risk.ToString().Substring(0, 3));
            LotSize = Lot * xLot;

            var longPosition = Positions.Find("Buy", SymbolName, TradeType.Buy);
            var shortPosition = Positions.Find("Sell", SymbolName, TradeType.Sell);

            if (previousClose > haHigh)
            {
                hasCrossedBelow = false;

                if (hasCrossedAbove == false && activeLong == false)
                {
                    if (volatility <= volatilityThreshold)
                    {
                        string trade = "Currency: " + Symbol.Name + " Trade : LONG " + "Entry: " + previousClose + " Stop Loss:  " + haLow + " Lots: " + LotSize;
                        // Chart.DrawStaticText(SymbolName, trade, VerticalAlignment.Top, HorizontalAlignment.Right, Color.Blue);
                        //Chart.DrawIcon("Buy Signal", ChartIconType.UpArrow, index, previousLow, Color.Gold);
                        activeLong = true;
                        activeShort = false;


                    }
                }
            }
            if (previousClose < haLow)
            {
                hasCrossedAbove = false;
                if (hasCrossedBelow == false && activeShort == false)
                {
                    if (volatility <= volatilityThreshold)
                    {
                        string trade = "Currency: " + Symbol.Name + " Trade : SHORT " + "Entry: " + previousClose + " Stop Loss:  " + haLow + " Lots: " + LotSize;
                        //Chart.DrawStaticText(SymbolName, trade, VerticalAlignment.Top, HorizontalAlignment.Right, Color.Blue);
                        // Chart.DrawIcon("Sell Signal", ChartIconType.DownArrow, index, previousHigh, Color.Gold);
                        activeShort = true;
                        activeLong = false;
                    }
                }
            }
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






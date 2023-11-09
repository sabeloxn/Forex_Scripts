using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class WTIDailyTrail : Robot
    {
        [Parameter(DefaultValue = 0.01, MinValue = 1E-05)]
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
        [Parameter("Enable STF", DefaultValue = false)]
        public bool STFenabled { get; set; }
        [Parameter("STF", DefaultValue = "Minute15")]
        public TimeFrame STF { get; set; }
        [Parameter("stfPeriods", DefaultValue = 17, MinValue = 1)]
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
        //public bool hasCrossedAbove;
        //public bool hasCrossedBelow;
        public bool activeLong;
        public bool activeShort;
        //private double LotSize;
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

        private BlackgoldIndicator myIndicator;

        private double LotSize;
        //private double stopLoss;
        public bool hasCrossedAbove;
        public bool hasCrossedBelow;
        public MarketSeries dailySeries;

        protected override void OnStart()
        {
            // Put your initialization logic here
            myIndicator = Indicators.GetIndicator<BlackgoldIndicator>(xLot, xStop, qdp, Periods, MAType, stopBuffer, volatilityThreshold, longBuffer, shortBuffer, stopLoss,
            takeProfit, STFenabled, STF, stfPeriods, stfvolatilityThreshold, stfstopBuffer, stflongBuffer, stfshortBuffer, range);

            stopLoss = 200;
            //stopBuffer = 1;
            volatilityThreshold = 2.0;
            //LotSize = Account.Balance * 10 / 100;
            hasCrossedAbove = true;
            hasCrossedBelow = true;
        }

        protected override void OnBar()
        {
            // Put your core logic here
            int index = MarketSeries.Close.Count - 1;
            var smaOpen = myIndicator.maOpen.Result[index];
            var smaClose = myIndicator.maClose.Result[index];
            var smaHigh = myIndicator.maHigh.Result[index];
            var smaLow = myIndicator.maLow.Result[index];

            LotSize = (long)Math.Ceiling(Account.Balance / 1000) * 10;

            var haOpen = (smaClose + smaOpen) / 2;
            var haClose = (smaOpen + smaClose + smaHigh + smaLow) / 4;
            var haHigh = Math.Max(smaHigh, Math.Max(haOpen, haClose));
            var haLow = Math.Min(smaLow, Math.Min(haOpen, haClose));

            var previousClose = (MarketSeries.Close.LastValue) + 0.2;

            var longPosition = Positions.Find("Buy", SymbolName, TradeType.Buy);
            var shortPosition = Positions.Find("Sell", SymbolName, TradeType.Sell);


            double volatility = Math.Round(haHigh - haLow, 2);
            if (previousClose > haHigh)
            {
                hasCrossedBelow = false;

                if (longPosition == null && hasCrossedAbove == false)
                {
                    Print("Volatility : {0}", volatility);
                    if (volatility <= volatilityThreshold)
                    {
                        double LongStopLoss = stopLoss - stopBuffer;
                        Print("Current buy stop level : {0}", LongStopLoss);
                        var buy = ExecuteMarketOrder(TradeType.Buy, SymbolName, LotSize, "Buy", LongStopLoss, null);

                    }

                }

                if (longPosition != null)
                {
                    double newLongStopLoss = haLow - stopBuffer;
                    ModifyPosition(longPosition, newLongStopLoss, null);
                }

            }


            if (previousClose < haLow)
            {
                hasCrossedAbove = false;
                if (shortPosition == null && hasCrossedBelow == false)
                {
                    //Print("Volatility : {0}", volatility);
                    if (volatility <= volatilityThreshold)
                    {
                        double ShortStopLoss = stopLoss + stopBuffer;
                        Print("Current sell stop level : {0}", ShortStopLoss);
                        var sell = ExecuteMarketOrder(TradeType.Sell, SymbolName, LotSize, "Sell", ShortStopLoss, null);
                    }
                    ;
                }
                if (shortPosition != null)
                {
                    double newShortStopLoss = haHigh + stopBuffer;
                    ModifyPosition(shortPosition, newShortStopLoss, null);
                }

            }

        }
        protected override void OnStop()
        {
            // Put your deinitialization logic here
            Stop();
        }
    }
}

using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class SampleTrendcBot2 : Robot
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
        [Parameter("STF", DefaultValue = "Weekly")]
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
        public bool stfhasCrossedAbove;
        public bool stfhasCrossedBelow;
        public MarketSeries dailySeries;

        protected override void OnStart()
        {
            myIndicator = Indicators.GetIndicator<BlackgoldIndicator>(xLot, xStop, qdp, Periods, MAType, stopBuffer, volatilityThreshold, longBuffer, shortBuffer, stopLoss,
            takeProfit, STFenabled, STF, stfPeriods, stfvolatilityThreshold, stfstopBuffer, stflongBuffer, stfshortBuffer, range);
            dailySeries = MarketData.GetSeries(TimeFrame.Daily);
            stfSeries = MarketData.GetSeries(STF);
            //stopLoss = 100;
            //volatilityThreshold = 2.0;
            hasCrossedAbove = true;
            hasCrossedBelow = true;
            stfhasCrossedAbove = true;
            stfhasCrossedBelow = true;
        }

        protected override void OnBar()
        {
            // Put your core logic here
            int index = MarketSeries.Close.Count - 1;
            int stfindex = stfSeries.Close.Count - 1;
            DateTime today = dailySeries.OpenTime[index].Date;
            DateTime tomorrow = today.AddDays(1);

            var smaOpen = myIndicator.maOpen.Result[index];
            var smaClose = myIndicator.maClose.Result[index];
            var smaHigh = myIndicator.maHigh.Result[index];
            var smaLow = myIndicator.maLow.Result[index];

            var stfsmaOpen = myIndicator.stfmaOpen.Result[stfindex];
            var stfsmaClose = myIndicator.stfmaClose.Result[stfindex];
            var stfsmaHigh = myIndicator.stfmaHigh.Result[stfindex];
            var stfsmaLow = myIndicator.stfmaLow.Result[stfindex];

            var haOpen = (smaClose + smaOpen) / 2;
            var haClose = (smaOpen + smaClose + smaHigh + smaLow) / 4;
            var haHigh = Math.Round(Math.Max(smaHigh, Math.Max(haOpen, haClose)), qdp);
            var haLow = Math.Round(Math.Min(smaLow, Math.Min(haOpen, haClose)), qdp);

            var stfhaOpen = (stfsmaClose + stfsmaOpen) / 2;
            var stfhaClose = (stfsmaOpen + stfsmaClose + stfsmaHigh + stfsmaLow) / 4;
            var stfhaHigh = Math.Round(Math.Max(stfsmaHigh, Math.Max(stfhaOpen, stfhaClose)), qdp);
            var stfhaLow = Math.Round(Math.Min(stfsmaLow, Math.Min(stfhaOpen, stfhaClose)), qdp);

            var previousClose = MarketSeries.Close.LastValue;
            var previousHigh = MarketSeries.High.LastValue;
            var previousLow = MarketSeries.Low.Last(1);

            var stfpreviousClose = stfSeries.Close.LastValue;
            var stfpreviousHigh = stfSeries.High.LastValue;
            var stfpreviousLow = stfSeries.Low.Last(1);

            var longPosition = Positions.Find("Buy", SymbolName, TradeType.Buy);
            var shortPosition = Positions.Find("Sell", SymbolName, TradeType.Sell);

            var stflongPosition = Positions.Find("stfBuy", SymbolName, TradeType.Buy);
            var stfshortPosition = Positions.Find("stfSell", SymbolName, TradeType.Sell);

            double LongStopLoss = Math.Round(Math.Abs(previousClose % haLow) * xStop, qdp);
            double ShortStopLoss = Math.Round(Math.Abs(haHigh % previousClose) * xStop, qdp);

            double stfLongStopLoss = Math.Round(Math.Abs(previousClose % stfhaLow) * xStop, qdp);
            double stfShortStopLoss = Math.Round(Math.Abs(stfhaHigh % previousClose) * xStop, qdp);

            //double LongStopLoss = Math.Round(Math.Abs(previousClose - haLow) * 10000, 2);
            //double ShortStopLoss = Math.Round(Math.Abs(previousClose - haHigh) * 100, 2);
            double volatility = Math.Round(haHigh - haLow, 2);
            double stfvolatility = Math.Round(stfhaHigh - stfhaLow, 2);

            double Risk = Math.Ceiling(Account.Balance * 10) / 100;
            int Lot = Convert.ToInt32(Risk.ToString().Substring(0, 3));
            LotSize = Symbol.QuantityToVolumeInUnits(Convert.ToInt32(Lot) * xLot);

                        /*double Risk = Math.Ceiling(Account.Balance * 10) / 100;
            double Lot = Convert.ToDouble(Risk.ToString().Substring(0, 3));*/
//LotSize = Symbol.NormalizeVolumeInUnits(Risk * xLot, RoundingMode.ToNearest);
//int lsCount = 0;
            //int stCount = 0;
            //Convert.ToInt32((Account.Balance * 10) / 100) * 100;
            //------------------------LONG-------------------------------------------------------------
if (previousClose > haHigh)
            {

                hasCrossedBelow = false;

                if (longPosition == null && hasCrossedAbove == false)
                {

                    if (volatility <= volatilityThreshold)
                    {

                        //Print("Previous close: {0}, haHigh: {1} == BUY Stoploss: {2}", previousClose, haHigh, LongStopLoss);
                        //Print("Lots: {0}", LotSize);
                        //Print("this: " + Symbol.NormalizeVolumeInUnits(LotSize, RoundingMode.Down));
                        //Chart.DrawIcon("Buy Signal", ChartIconType.UpArrow, today, previousLow, Color.DodgerBlue);
                        //Print("Long losses: {0}", lsCount);
                        //Print("Risk: {0}; Lot: {1}; Lot Size:{2}", Risk, Lot, LotSize);
                        //var buy = ExecuteMarketOrder(TradeType.Buy, SymbolName, LotSize, "Buy", LongStopLoss, null);
                        //string trade = "Currency: " + Symbol.Name + " Trade : LONG " + "Entry: " + previousClose + " Stop Loss:  " + LongStopLoss + " Lots: " + LotSize;
                        //Chart.DrawStaticText(SymbolName, trade, VerticalAlignment.Top, HorizontalAlignment.Left, Color.GhostWhite);
                        //Chart.DrawIcon("Buy Signal", ChartIconType.UpArrow, index, previousLow, Color.Gold);
                    }

                }
                //-----------------TRAILING STOP 
                if (longPosition != null)
                {
                    double newLongStopLoss = haLow;
                    ModifyPosition(longPosition, newLongStopLoss, null);
                }

            }

            //------------------Short-------------------------------------
            if (previousClose < haLow)
            {
                //Print("Volatility Threshold:{1} ; Short Volatility : {0}", volatility, volatilityThreshold);

                hasCrossedAbove = false;
                if (shortPosition == null && hasCrossedBelow == false)
                {

                    if (volatility <= volatilityThreshold)
                    {

                        //Print("Previous close: {0}, haLow: {1} == SELL Stoploss: {2}", previousClose, haLow, ShortStopLoss);
                        //Print("Lots: {0}", LotSize);
                        //Chart.DrawIcon("Sell Signal", ChartIconType.DownArrow, today, previousHigh, Color.Gold);
                        //Print("Risk: {0}; Lot: {1}; Lot Size:{2}", Risk, Lot, LotSize);
                        //string trade = "Currency: " + Symbol.Name + " Trade : SHORT " + "Entry: " + previousClose + " Stop Loss:  " + ShortStopLoss + " Lots: " + LotSize;
                        //Chart.DrawStaticText(SymbolName, trade, VerticalAlignment.Top, HorizontalAlignment.Left, Color.GhostWhite);
                        //Chart.DrawIcon("Sell Signal", ChartIconType.DownArrow, index, previousHigh, Color.Gold);
                        //Print("Short losses: {0}", stCount);
                        //var sell = ExecuteMarketOrder(TradeType.Sell, SymbolName, LotSize, "Sell", ShortStopLoss, null);

                    }

                }
                //-----------------TRAILING STOP 
                if (shortPosition != null)
                {
                    double newShortStopLoss = haHigh;
                    ModifyPosition(shortPosition, newShortStopLoss, null);
                }

            }
            //-----------------------------------------------STF--------------------------------------------------------------------------

            if (STFenabled)
            {
                //Print("STF Enabled");
                if (previousClose > stfhaHigh)
                {

                    stfhasCrossedBelow = false;

                    if (stflongPosition == null && stfhasCrossedBelow == false)
                    {

                        if (stfvolatility <= stfvolatilityThreshold)
                        {
                            Print(stfhaLow, stfhaHigh, stfpreviousClose + " " + stfvolatility);
                            Print("Previous close: {0}, haHigh: {1} == BUY Stoploss: {2}", stfpreviousClose, stfhaHigh, stfLongStopLoss);
                            //Print("Lots: {0}", LotSize);
                            //Print("this: " + Symbol.NormalizeVolumeInUnits(LotSize, RoundingMode.Down));
                            // Chart.DrawIcon("Buy Signal", ChartIconType.UpArrow, today, previousLow, Color.DodgerBlue);
                            //Print("Long losses: {0}", lsCount);
                            Print("*STF: Risk: {0}; Lot: {1}; Lot Size:{2}", Risk, Lot, LotSize);
                            var stfsell = ExecuteMarketOrder(TradeType.Sell, SymbolName, LotSize, "stfSell", 100, 200);

                            string stftrade = "Currency: " + Symbol.Name + " stfTrade : stfLONG " + "stfEntry: " + stfpreviousClose + "stf Stop Loss:  " + stfLongStopLoss + " Lots: " + LotSize;
                            Chart.DrawStaticText(SymbolName, stftrade, VerticalAlignment.Top, HorizontalAlignment.Right, Color.GhostWhite);
                            Chart.DrawIcon("stf Buy Signal", ChartIconType.UpArrow, stfindex, stfpreviousLow, Color.Gold);

                        }

                    }
                    //-----------------TRAILING STOP 
                    if (stflongPosition != null)
                    {
                        double stfnewLongStopLoss = stfhaHigh;
                        ModifyPosition(stflongPosition, 100, stfnewLongStopLoss);
                    }

                }

                //------------------Short-------------------------------------
                if (stfpreviousClose < stfhaLow)
                {
                    //Print("Volatility Threshold:{1} ; Short Volatility : {0}", volatility, volatilityThreshold);

                    stfhasCrossedAbove = false;
                    if (stfshortPosition == null && stfhasCrossedAbove == false)
                    {

                        if (stfvolatility <= stfvolatilityThreshold)
                        {

                            Print("Previous close: {0}, haLow: {1} == SELL Stoploss: {2}", previousClose, haLow, ShortStopLoss);
                            //Print("Lots: {0}", LotSize);
                            //Chart.DrawIcon("Sell Signal", ChartIconType.DownArrow, today, previousHigh, Color.Gold);
                            Print("STF Risk: {0}; Lot: {1}; Lot Size:{2}", Risk, Lot, LotSize);
                            string stftrade = "Currency: " + Symbol.Name + " stfTrade : SHORT " + "stfEntry: " + stfpreviousClose + "stf Stop Loss:  " + stfShortStopLoss + " stfLots: " + LotSize;
                            Chart.DrawStaticText(SymbolName, stftrade, VerticalAlignment.Top, HorizontalAlignment.Right, Color.GhostWhite);
                            Chart.DrawIcon("stf Sell Signal", ChartIconType.DownArrow, stfindex, stfpreviousHigh, Color.Gold);
                            //Print("Short losses: {0}", stCount);
                            var stfbuy = ExecuteMarketOrder(TradeType.Buy, SymbolName, LotSize, "stfBuy", 100, 200);

                        }

                    }
                    //-----------------TRAILING STOP 
                    if (stfshortPosition != null)
                    {
                        double stfnewShortStopLoss = stfhaLow;
                        ModifyPosition(stfshortPosition, 100, stfnewShortStopLoss);
                    }

                }
                //-----------------------------------------------END--------------------------------------------------------------------------
                if (index == 0)
                    return;

                DateTime currentTime = MarketSeries.OpenTime[index];
                DateTime previousTime = MarketSeries.OpenTime[index - 1];


                if (currentTime.Month != previousTime.Month)
                {
                }
            }
        }
        /*first bar of the month  
                Print(currentTime);
                if (longPosition.GrossProfit < 0)
                {
                    lsCount += 1;
                }
                if (shortPosition.GrossProfit < 0)
                {
                    stCount += 1;
                }*/
        protected override void OnStop()
        {
            Stop();
        }
    }
}

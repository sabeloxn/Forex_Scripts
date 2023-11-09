using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class SA40 : Robot
    {

        [Parameter(DefaultValue = 18, MinValue = 1)]
        public int Periods { get; set; }

        [Parameter("MA Type", DefaultValue = MovingAverageType.Simple)]
        public MovingAverageType MAType { get; set; }

        [Parameter("Stop Buffer", DefaultValue = 0.3, MinValue = 0.01)]
        public double stopBuffer { get; set; }

        [Parameter("Volatility ", DefaultValue = 0.3, MinValue = 0.01)]
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

        [Parameter("Use STF")]
        public bool useSTF { get; set; }

        [Parameter("STF", DefaultValue = "Minute15")]
        public TimeFrame STF { get; set; }

        [Parameter("stfPeriods", DefaultValue = 17, MinValue = 1)]
        public int stfPeriods { get; set; }

        [Parameter("stfVolatility ", DefaultValue = 0.3, MinValue = 0.1)]
        public double stfvolatilityThreshold { get; set; }

        //[Parameter("Enable STF", DefaultValue = false)]
        public bool STFenabled { get; set; }

        [Parameter("stfStop Buffer", DefaultValue = 0.3, MinValue = 0.1)]
        public double stfstopBuffer { get; set; }

        /*[Parameter("stfLongBuffer", DefaultValue = 0.1, MinValue = 0.01)]
        public double stflongBuffer { get; set; }
        [Parameter("stfShortBuffer", DefaultValue = 0.1, MinValue = 0.01)]
        public double stfshortBuffer { get; set; }*/
//----------------------------------------------------------RANGE PARAMETERS------------------------------------------------------------------
        [Parameter("Range enabled")]
        public bool range { get; set; }

//------------------------------------------------------------------------------------------------------------------------------------        
        public double longPreviousClose;
        public double shortPreviousClose;
        public double stflongPreviousClose;
        public double stfshortPreviousClose;

        private double LotSize;
        private double stfLotSize;
        private double stfstopLoss;
        public bool hasCrossedAbove;
        public bool hasCrossedBelow;
        public bool stfhasCrossedAbove;
        public bool stfhasCrossedBelow;

        public MarketSeries stfSeries;
        private BlackgoldIndicator myIndicator;

        protected override void OnStart()
        {
            // Put your initialization logic here
            myIndicator = Indicators.GetIndicator<BlackgoldIndicator>(Periods, stfPeriods, MAType, STFenabled, STF);
            //stopLoss = 80;
            //takeProfit = 33;
            //stopBuffer = 0.3;
            hasCrossedAbove = true;
            hasCrossedBelow = true;
            //LotSize = Account.Balance * 10 / 100;

            stfstopLoss = 200;
            stfvolatilityThreshold = 2.0;
            //stfLotSize = Account.Balance * 10 / 100;
            stfhasCrossedAbove = true;
            stfhasCrossedBelow = true;

            stfSeries = MarketData.GetSeries(STF);
        }

        protected override void OnBar()
        {
            // Put your core logic here
            int index = MarketSeries.Close.Count - 1;
            var stfindex = GetIndexByDate(stfSeries, MarketSeries.OpenTime[index]);

            var smaOpen = myIndicator.maOpen.Result[index];
            var smaClose = myIndicator.maClose.Result[index];
            var smaHigh = myIndicator.maHigh.Result[index];
            var smaLow = myIndicator.maLow.Result[index];

            var stfsmaOpen = myIndicator.stfmaOpen.Result[stfindex];
            var stfsmaClose = myIndicator.stfmaClose.Result[stfindex];
            var stfsmaHigh = myIndicator.stfmaHigh.Result[stfindex];
            var stfsmaLow = myIndicator.stfmaLow.Result[stfindex];

            LotSize = (long)Math.Ceiling(Account.Balance / 100000) * 100;
            stfLotSize = (long)Math.Ceiling(Account.Balance / 1000) * 100;

            var haOpen = (smaClose + smaOpen) / 2;
            var haClose = (smaOpen + smaClose + smaHigh + smaLow) / 4;
            var haHigh = Math.Max(smaHigh, Math.Max(haOpen, haClose));
            var haLow = Math.Min(smaLow, Math.Min(haOpen, haClose));

            var stfhaOpen = (stfsmaClose + stfsmaOpen) / 2;
            var stfhaClose = (stfsmaOpen + stfsmaClose + stfsmaHigh + stfsmaLow) / 4;
            var stfhaHigh = Math.Max(stfsmaHigh, Math.Max(stfhaOpen, stfhaClose));
            var stfhaLow = Math.Min(stfsmaLow, Math.Min(stfhaOpen, stfhaClose));

            longPreviousClose = MarketSeries.Close.LastValue;
            //+ longBuffer;
            shortPreviousClose = MarketSeries.Close.LastValue;
            //- shortBuffer;
            stflongPreviousClose = MarketSeries.Close.LastValue;
            //+ stflongBuffer;
            stfshortPreviousClose = MarketSeries.Close.LastValue;
            // - stfshortBuffer;
            var longPosition = Positions.Find("Buy", SymbolName, TradeType.Buy);
            var shortPosition = Positions.Find("Sell", SymbolName, TradeType.Sell);

            var stflongPosition = Positions.Find("stfBuy", SymbolName, TradeType.Buy);
            var stfshortPosition = Positions.Find("stfSell", SymbolName, TradeType.Sell);

            double volatility = Math.Round(haHigh - haLow, 2);
            double stfvolatility = Math.Round(stfhaHigh - stfhaLow, 2);

            var previousClose = (MarketSeries.Close.LastValue) + 0.2;
            var stfpreviousClose = (stfSeries.Close.LastValue) + 0.2;

            if (previousClose > haHigh)
            {
                hasCrossedBelow = false;
                //&& isGap == false)
                if (longPosition == null && hasCrossedAbove == false)
                {
                    Print("Volatility : {0}", volatility);
                    if (volatility <= volatilityThreshold)
                    {
                        hasCrossedAbove = true;
                        double LongStopLoss = stopLoss;
                        //- stopBuffer;
                        //Print("Current buy stop level : {0}", LongStopLoss);
                        var buy = ExecuteMarketOrder(TradeType.Buy, SymbolName, LotSize, "Buy", LongStopLoss, takeProfit);
                    }
                    ;
                }

            }
            if (longPosition != null)
            {
                double newLongStopLoss = haLow;
                //- stopBuffer;
                //double newtakeprofit = longPosition.EntryPrice + 1;
                ModifyPosition(longPosition, newLongStopLoss, null);
            }

            if (shortPreviousClose < haLow)
            {
                hasCrossedAbove = false;
                if (shortPosition == null && hasCrossedBelow == false)
                {
                    Print("Volatility : {0}", volatility);
                    if (volatility <= volatilityThreshold)
                    {
                        hasCrossedBelow = true;
                        double ShortStopLoss = stopLoss;
                        //+ stopBuffer;
                        // Print("Current sell stop level : {0}", ShortStopLoss);
                        var sell = ExecuteMarketOrder(TradeType.Sell, SymbolName, LotSize, "Sell", ShortStopLoss, takeProfit);
                    }

                }
            }
            if (shortPosition != null)
            {
                double newShortStopLoss = haHigh;
                //+ stopBuffer;
                //double newtakeprofit = shortPosition.EntryPrice - 1;
                ModifyPosition(shortPosition, newShortStopLoss, null);
            }
//---------------------------------------------------------------------HIGHS AND LOWS----------------------------------------------
            DateTime today = MarketSeries.OpenTime[index].Date;
            DateTime tomorrow = today.AddDays(1);

            double high = MarketSeries.High.LastValue;
            double low = MarketSeries.Low.LastValue;

            for (int i = MarketSeries.Close.Count - 1; i > 0; i--)
            {
                if (MarketSeries.OpenTime[i].Date < today)
                    break;

                high = Math.Max(high, MarketSeries.High[i]);
                low = Math.Min(low, MarketSeries.Low[i]);
            }

            //ChartObjects.DrawLine("high " + today, today, high, tomorrow, high, Colors.Pink);
            //ChartObjects.DrawLine("low " + today, today, low, tomorrow, low, Colors.Pink);

//--------------------------------------------------------RANGE TRADING--------------------------------------------------------------
            if (range == true)
            {
                if (volatility > volatilityThreshold)
                {
                    if (longPreviousClose > haHigh)
                    {
                        hasCrossedBelow = false;
                        if (longPosition == null && hasCrossedAbove == false)
                        {
                            Print("Range trade with Volatility : {0}", volatility);

                            hasCrossedAbove = true;
                            double LongStopLoss = low;
                            double rangeProfit = high - low;
                            //Print("Current buy stop level : {0}", LongStopLoss);
                            var buy = ExecuteMarketOrder(TradeType.Buy, SymbolName, LotSize, "rangeBuy", LongStopLoss, rangeProfit);
                        }

                    }

                    if (shortPreviousClose < haLow)
                    {
                        hasCrossedAbove = false;
                        if (shortPosition == null && hasCrossedBelow == false)
                        {
                            Print("range trade with Volatility : {0}", volatility);

                            hasCrossedBelow = true;
                            double ShortStopLoss = high;
                            double rangeProfit = high - low;
                            // Print("Current sell stop level : {0}", ShortStopLoss);
                            var sell = ExecuteMarketOrder(TradeType.Sell, SymbolName, LotSize, "rangeSell", ShortStopLoss, rangeProfit);
                        }
                    }
                }

            }

            //--------------------------------------------------------STF--------------------------------------------------------------------
            if (useSTF == true)
            {

                if (stfindex != -1)
                {

                    if (stflongPreviousClose > stfhaHigh)
                    {
                        stfhasCrossedBelow = false;

                        if (stflongPosition == null && stfhasCrossedAbove == false)
                        {
                            Print("STF buy position executed");
                            // Print("Volatility : {0}", volatility);
                            if (stfvolatility <= stfvolatilityThreshold)
                            {
                                double stfLongStopLoss = stfstopLoss;
                                //- stfstopBuffer;
                                //Print("Current buy stop level : {0}", LongStopLoss);  
                                var stfbuy = ExecuteMarketOrder(TradeType.Buy, SymbolName, stfLotSize, "stfBuy", stfLongStopLoss, null);

                            }

                        }

                        if (stflongPosition != null)
                        {
                            double newstfLongStopLoss = stfhaLow - stfstopBuffer;
                            ModifyPosition(stflongPosition, newstfLongStopLoss, null);
                        }

                    }


                    if (stfshortPreviousClose < stfhaLow)
                    {
                        stfhasCrossedAbove = false;
                        if (stfshortPosition == null && stfhasCrossedBelow == false)
                        {
                            //Print("Volatility : {0}", volatility);
                            if (stfvolatility <= stfvolatilityThreshold)
                            {
                                Print("STF sell position executed");
                                double stfShortStopLoss = stfstopLoss;
                                //+ stfstopBuffer;
                                // Print("Current sell stop level : {0}", ShortStopLoss);
                                var stfsell = ExecuteMarketOrder(TradeType.Sell, SymbolName, stfLotSize, "stfSell", stfShortStopLoss, null);
                            }

                        }
                        if (stfshortPosition != null)
                        {
                            double newstfShortStopLoss = stfhaHigh + stfstopBuffer;
                            ModifyPosition(stfshortPosition, newstfShortStopLoss, null);
                        }

                    }


                }

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
        protected override void OnStop()
        {
            // Put your deinitialization logic here
            Stop();
        }
    }
}

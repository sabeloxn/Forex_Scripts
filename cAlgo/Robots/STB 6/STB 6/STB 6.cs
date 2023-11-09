using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using System.Diagnostics;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class SampleTrendcBot2 : Robot
    {
        [Parameter("ChartTF", DefaultValue = true)]
        public bool ChartTF { get; set; }
        [Parameter(DefaultValue = 18, MinValue = 1)]
        public int Periods { get; set; }
        [Parameter("MA Type", DefaultValue = MovingAverageType.Exponential)]
        public MovingAverageType MAType { get; set; }

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

        [Parameter("Units", DefaultValue = 100000, MinValue = 1)]
        public int units { get; set; }
        [Parameter("Units", DefaultValue = 100000, MinValue = 1)]
        public int LotSize;
        [Parameter("xStop", DefaultValue = 1, MinValue = 1)]
        public double xStop { get; set; }
        [Parameter(DefaultValue = 2, MinValue = 2)]
        public int qdp { get; set; }
        [Parameter("Trail", DefaultValue = true)]
        public bool trail { get; set; }
        [Parameter("SHA Trail", DefaultValue = true)]
        public bool SHAtrail { get; set; }
        [Parameter("Volatility Threshold ", DefaultValue = 0.3, MinValue = 1E-05)]
        public double volatilityThreshold { get; set; }

        [Parameter("Stop Loss", DefaultValue = 1, MinValue = 1)]
        public double stopLoss { get; set; }
        [Parameter("Take Profit", DefaultValue = 1, MinValue = 1)]
        public double takeProfit { get; set; }

//-----------------------------------------------STF Parameters-------------------------------------------------------------------------------

        [Parameter("stfVolatility Threshold", DefaultValue = 0.3, MinValue = 0.1)]
        public double SHAvolatilityThreshold { get; set; }
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

//----------------------------------------------------------RANGE PARAMETERS------------------------------------------------------------------
        [Parameter("Range enabled")]
        public bool range { get; set; }
//--------------------------------------------------------------------------------------------------------------------------------------------
        [Parameter("Gap Theshold", DefaultValue = 1E-05, MinValue = 1E-05)]
        public double gapThreshold { get; set; }
        [Parameter("SHA Gap Theshold", DefaultValue = 1E-05, MinValue = 1E-05)]
        public double SHAgapThreshold { get; set; }
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
//============================================================================================================================================
        private Blackgoldindi3 myIndicator;
        public bool hasCrossedAbove, hasCrossedBelow;
        public bool hasCrossedAboveSHA, hasCrossedBelowSHA;
        public MarketSeries SHAseries;
        public MarketSeries SHAbars;
        public MarketSeries series;
        private bool gapUp, gapDown;
        private bool SHAgapUp, SHAgapDown;
        protected override void OnStart()
        {
            myIndicator = Indicators.GetIndicator<Blackgoldindi3>(ChartTF, Periods, MAType, STFenabled, SHATF, SHAMAType, stfPeriods);
            SHAseries = MarketData.GetSeries(SHATF);
            series = MarketData.GetSeries(TimeFrame.Daily);
            SHAbars = MarketData.GetSeries(SHAbarsTF);
            hasCrossedAbove = true;
            hasCrossedBelow = true;
            hasCrossedAboveSHA = true;
            hasCrossedBelowSHA = true;
            // Positions.Closed += PositionClosed;
            Positions.Closed += OnPositionsClosed;

        }


        protected override void OnBar()
        {
            int index = MarketSeries.Close.Count - 1;
            int SHAindex = SHAseries.Close.Count - 1;

            DateTime today = series.OpenTime[index].Date;
            DateTime tomorrow = today.AddDays(1);

            var smaOpen = myIndicator.maOpen.Result[index];
            var smaClose = myIndicator.maClose.Result[index];
            var smaHigh = myIndicator.maHigh.Result[index];
            var smaLow = myIndicator.maLow.Result[index];

            var haOpen = (smaClose + smaOpen) / 2;
            var haClose = (smaOpen + smaClose + smaHigh + smaLow) / 4;
            var haHigh = Math.Round(Math.Max(smaHigh, Math.Max(haOpen, haClose)), qdp);
            var haLow = Math.Round(Math.Min(smaLow, Math.Min(haOpen, haClose)), qdp);

            var previousClose = SHAbars.Close.LastValue;
            var previousHigh = SHAbars.High.LastValue;
            var previousLow = SHAbars.Low.Last(1);

            double LongStopLoss = Math.Round(Math.Abs(previousClose % haLow) * xStop, qdp);
            double ShortStopLoss = Math.Round(Math.Abs(haHigh % previousClose) * xStop, qdp);

            var SHAOpen = myIndicator.SHAmaOpen.Result[SHAindex];
            var SHAClose = myIndicator.SHAmaClose.Result[SHAindex];
            var SHAHigh = myIndicator.SHAmaHigh.Result[SHAindex];
            var SHALow = myIndicator.SHAmaLow.Result[SHAindex];

            var SHAmaOpen = (SHAClose + SHAOpen) / 2;
            var SHAmaClose = (SHAOpen + SHAClose + SHAHigh + SHALow) / 4;
            var SHAmaHigh = Math.Round(Math.Max(SHAHigh, Math.Max(SHAOpen, SHAClose)), qdp);
            var SHAmaLow = Math.Round(Math.Min(SHALow, Math.Min(SHAOpen, SHAClose)), qdp);

            var SHApreviousClose = SHAbars.Close.LastValue;
            var SHApreviousHigh = SHAbars.High.Last(1);
            var SHApreviousLow = SHAbars.Low.Last(1);

            double SHALongStopLoss = Math.Round(Math.Abs(SHApreviousClose % SHALow) * xStop, qdp);
            double SHAShortStopLoss = Math.Round(Math.Abs(SHAHigh % SHApreviousClose) * xStop, qdp);




            //LotSize = Symbol.QuantityToVolumeInUnits(Convert.ToInt32(Lot) * xLot);
            double totalPipsLong = LongStopLoss + Symbol.Spread;
            double totalPipsShort = ShortStopLoss + Symbol.Spread;


            double volatility = Math.Round(haHigh - haLow, qdp);
            double SHAvolatility = Math.Round(SHAHigh - SHALow, qdp);
            //Print(" Volatility : {0}; Volatility Threshold:{1}", volatility, volatilityThreshold);
            //Print("Previous close: {0}, haHigh: {1} haLow: {2}", previousClose, haHigh, haLow);
            //Print(previousClose + "    " + MarketSeries.Open.LastValue);

            double gapDownSize = Math.Round(previousLow - MarketSeries.Open.LastValue, qdp);
            double gapUpSize = Math.Round(MarketSeries.Open.LastValue - previousHigh, qdp);
            double SHAgapDownSize = Math.Round(SHApreviousLow - SHAbars.Open.LastValue, qdp);
            double SHAgapUpSize = Math.Round(SHAbars.Open.LastValue - SHApreviousHigh, qdp);
//---------------------------------------------------STF===========================================================================
            if (STFenabled)
            {
                var SHAlongPosition = Positions.Find("SHABuy", SymbolName, TradeType.Buy);
                var SHAshortPosition = Positions.Find("SHASell", SymbolName, TradeType.Sell);
                //----------------------SHORT-------------------------------------
                //Print("Previous close: {0}, haHigh: {1} haLow: {2}", previousClose, haHigh, haLow);
                if (SHApreviousClose < SHALow)
                {
                    //Print("Long " + hasCrossedBelow + " " + LotSize);
                    hasCrossedAboveSHA = false;

                    if (SHAbars.Open.LastValue < SHApreviousLow)
                    {
                        SHAgapDown = true;
                        Print("SHA Gap Down" + " " + SHAgapDownSize);
                        if (SHAgapDownSize > SHAgapThreshold)
                        {
                            hasCrossedBelowSHA = true;
                        }
                    }

                    if (SHAlongPosition == null && hasCrossedBelowSHA == false)
                    {
                        if (SHAvolatility <= SHAvolatilityThreshold)
                        {
                            Chart.DrawIcon("SHA Sell Signal", ChartIconType.DownArrow, SHAindex, SHApreviousHigh, Color.Gold);
                            //Print("Stoploss: {1}; Lots: {0}", Short_volume, ShortStopLoss);
                            var SHAsell = ExecuteMarketOrder(TradeType.Sell, SymbolName, units, "Sell", SHAShortStopLoss, null);
                            hasCrossedBelowSHA = true;
                            Stopwatch timer = new Stopwatch();
                            int seconds = 30;

                        }
                    }
                }
                //-----------------TRAILING STOP 
                if (SHAtrail == true)
                {
                    if (SHAshortPosition != null)
                    {
                        double SHAnewShortStopLoss = SHAHigh;
                        ModifyPosition(SHAshortPosition, SHAnewShortStopLoss, null);

                    }

                }
                //------------------------LONG-------------------------------------------------------------
                if (SHApreviousClose > SHAHigh)
                {
                    hasCrossedBelowSHA = false;

                    if (SHAbars.Open.LastValue > SHApreviousHigh)
                    {
                        SHAgapUp = true;
                        Print("SHA Gap Up" + " " + SHAgapUpSize);

                        if (SHAgapUpSize > SHAgapThreshold)
                        {
                            hasCrossedAboveSHA = true;
                        }
                    }

                    if (SHAlongPosition == null && hasCrossedAboveSHA == false)
                    {
                        if (SHAvolatility <= SHAvolatilityThreshold)
                        {
                            //Print(" Volatility : {0}; Volatility Threshold:{1}", volatility, volatilityThreshold);
                            Chart.DrawIcon("Buy Signal", ChartIconType.UpArrow, SHAindex, SHApreviousLow, Color.Gold);

                            //Print("Stoploss: {1}; Lots: {0}", Long_volume, LongStopLoss);
                            var buy = ExecuteMarketOrder(TradeType.Buy, SymbolName, units, "SHABuy", SHALongStopLoss, null);
                            hasCrossedAboveSHA = true;
                            Stopwatch timer = new Stopwatch();
                            int seconds = 30;

                        }
                    }
                }

                //-----------------TRAILING STOP 
                if (SHAtrail == true)
                {
                    if (SHAlongPosition != null)
                    {
                        double newSHALongStopLoss = SHALow;
                        ModifyPosition(SHAlongPosition, newSHALongStopLoss, null);

                    }
                }
            }

            if (ChartTF == true)
            {
                var longPosition = Positions.Find("Buy", SymbolName, TradeType.Buy);
                var shortPosition = Positions.Find("Sell", SymbolName, TradeType.Sell);
                //----------------------SHORT-------------------------------------
                //Print("Previous close: {0}, haHigh: {1} haLow: {2}", previousClose, haHigh, haLow);
                if (previousClose < haLow)
                {
                    //Print("Long " + hasCrossedBelow + " " + LotSize);
                    hasCrossedAbove = false;
                    if (longPosition == null && hasCrossedBelow == false)
                    {
                        if (volatility <= volatilityThreshold)
                        {
                            Print(" Volatility : {0}; Volatility Threshold:{1}", volatility, volatilityThreshold);
                            string trade = "Currency: " + Symbol.Name + " Trade : SHORT " + "Entry: " + previousClose + " Stop Loss:  " + ShortStopLoss + " Lots: " + LotSize;
                            Chart.DrawIcon("Sell Signal", ChartIconType.DownArrow, index, previousHigh, Color.Gold);
                            //Print("Stoploss: {1}; Lots: {0}", Short_volume, ShortStopLoss);
                            var sell = ExecuteMarketOrder(TradeType.Sell, SymbolName, 100000, "Sell", ShortStopLoss, null);
                            hasCrossedBelow = true;
                            Stopwatch timer = new Stopwatch();
                            int seconds = 30;

                        }
                    }
                }
                /*timer.Start();
                            while (timer.Elapsed.TotalSeconds < seconds)
                            {
                                // do something
                                Notifications.PlaySound("C:\\to-the-point.mp3");
                            }
                            timer.Stop();*/

                //-----------------TRAILING STOP 
                if (trail == true)
                {
                    if (shortPosition != null)
                    {
                        double newShortStopLoss = haHigh;
                        ModifyPosition(shortPosition, newShortStopLoss, null);
                    }
                }
                //------------------------LONG-------------------------------------------------------------
                if (previousClose > haHigh)
                {
                    hasCrossedBelow = false;

                    if (longPosition == null && hasCrossedAbove == false)
                    {
                        if (volatility <= volatilityThreshold)
                        {
                            // Print(" Volatility : {0}; Volatility Threshold:{1}", volatility, volatilityThreshold);
                            string trade = "Currency: " + Symbol.Name + " Trade : LONG " + "Entry: " + previousClose + " Stop Loss:  " + LongStopLoss + " Lots: " + LotSize;
                            Chart.DrawIcon("Buy Signal", ChartIconType.UpArrow, index, previousLow, Color.Gold);

                            //Print("Stoploss: {1}; Lots: {0}", Long_volume, LongStopLoss);
                            var buy = ExecuteMarketOrder(TradeType.Buy, SymbolName, 100000, "Buy", LongStopLoss, null);
                            hasCrossedAbove = true;
                            Stopwatch timer = new Stopwatch();
                            int seconds = 30;

                            timer.Start();
                            while (timer.Elapsed.TotalSeconds < seconds)
                            {
                                // do something
                                Notifications.PlaySound("C:\\to-the-point.mp3");
                            }
                            timer.Stop();

                        }
                    }
                }
                //-----------------TRAILING STOP 
                if (trail == true)
                {
                    if (longPosition != null)
                    {
                        double newLongStopLoss = haLow;
                        ModifyPosition(longPosition, newLongStopLoss, null);
                    }
                }
            }


        }

        private void OnPositionsClosed(PositionClosedEventArgs args)
        {
            Print("Closed");
            hasCrossedAbove = false;
            Print("Has crossed above: " + hasCrossedAbove);

            hasCrossedBelow = false;
            Print("Has crossed below: " + hasCrossedBelow);

        }

        protected override void OnStop()
        {
            Stop();
        }
    }
}

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
    public class STB5SP500 : Robot
    {
        [Parameter("Risk %", DefaultValue = 10, MinValue = 1)]
        public int riskPercentage { get; set; }
        [Parameter("Max Volume", DefaultValue = 100000, MinValue = 1000)]
        public int maxVol { get; set; }

        [Parameter("xLot", DefaultValue = 0.01, MinValue = 1E-05)]
        public double xLot { get; set; }
        [Parameter("xStop", DefaultValue = 1, MinValue = 1)]
        public double xStop { get; set; }
        [Parameter(DefaultValue = 2, MinValue = 2)]
        public int qdp { get; set; }
        [Parameter("Trail", DefaultValue = false)]
        public bool trail { get; set; }
        [Parameter(DefaultValue = 18, MinValue = 1)]
        public int Periods { get; set; }
        [Parameter("MA Type", DefaultValue = MovingAverageType.Exponential)]
        public MovingAverageType MAType { get; set; }
        [Parameter("Stop Buffer", DefaultValue = 0.3, MinValue = 0.01)]
        public double stopBuffer { get; set; }
        [Parameter("Volatility Threshold ", DefaultValue = 0.3, MinValue = 1E-05)]
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

//----------------------------------------------------------RANGE PARAMETERS------------------------------------------------------------------
        [Parameter("Range enabled")]
        public bool range { get; set; }
//--------------------------------------------------------------------------------------------------------------------------------------------
        [Parameter("Gap Theshold", DefaultValue = 1E-05, MinValue = 1E-05)]
        public double gapThreshold { get; set; }
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
        private Blackgoldindi2 myIndicator;
        public bool hasCrossedAbove;
        public bool hasCrossedBelow;
        public MarketSeries series;
        private double LotSize;
        private int Short_volume;
        private int Long_volume;
        private bool gapUp, gapDown;

        protected override void OnStart()
        {
            myIndicator = Indicators.GetIndicator<Blackgoldindi2>(xLot, xStop, qdp, Periods, MAType, stopBuffer, volatilityThreshold, longBuffer, shortBuffer, stopLoss,
            takeProfit, STFenabled, STF, stfPeriods, stfvolatilityThreshold, stfstopBuffer, stflongBuffer, stfshortBuffer, range);
            series = MarketData.GetSeries(STF);

            hasCrossedAbove = true;
            hasCrossedBelow = true;
            Positions.Closed += PositionClosed;

        }


        protected override void OnBar()
        {
            int index = MarketSeries.Close.Count - 1;
            int myindex = series.Close.Count - 1;

            DateTime today = series.OpenTime[index].Date;
            DateTime tomorrow = today.AddDays(1);

            var smaOpen = myIndicator.maOpen.Result[myindex];
            var smaClose = myIndicator.maClose.Result[myindex];
            var smaHigh = myIndicator.maHigh.Result[myindex];
            var smaLow = myIndicator.maLow.Result[myindex];

            var haOpen = (smaClose + smaOpen) / 2;
            var haClose = (smaOpen + smaClose + smaHigh + smaLow) / 4;
            var haHigh = Math.Round(Math.Max(smaHigh, Math.Max(haOpen, haClose)), qdp);
            var haLow = Math.Round(Math.Min(smaLow, Math.Min(haOpen, haClose)), qdp);

            var previousClose = MarketSeries.Close.LastValue;
            var previousHigh = MarketSeries.High.Last(1);
            var previousLow = MarketSeries.Low.Last(1);

            double LongStopLoss = Math.Round(Math.Abs(previousClose % haLow) * xStop, qdp);
            double ShortStopLoss = Math.Round(Math.Abs(haHigh % previousClose) * xStop, qdp);

            double Risk = Math.Ceiling(Account.Balance * riskPercentage) / 100;
            int Lot = Convert.ToInt32(Risk.ToString().Substring(0, 3));
            //LotSize = Symbol.QuantityToVolumeInUnits(Convert.ToInt32(Lot) * xLot);
            double totalPipsLong = LongStopLoss + Symbol.Spread;
            double totalPipsShort = ShortStopLoss + Symbol.Spread;
            double exactVolumeLong = Math.Round(Risk / (Symbol.PipValue * totalPipsLong), 2);
            double exactVolumeShort = Math.Round(Risk / (Symbol.PipValue * totalPipsShort), 2);
            Short_volume = (((int)exactVolumeLong) / 100000) * 100000;
            Long_volume = (((int)exactVolumeShort) / 100000) * 100000;

            double volatility = Math.Round(haHigh - haLow, qdp);
            //Print(" Volatility : {0}; Volatility Threshold:{1}", volatility, volatilityThreshold);
            //Print("Previous close: {0}, haHigh: {1} haLow: {2}", previousClose, haHigh, haLow);
            //Print(previousClose + "    " + MarketSeries.Open.LastValue);

            double gapDownSize = Math.Round(previousLow - MarketSeries.Open.LastValue, qdp);
            double gapUpSize = Math.Round(MarketSeries.Open.LastValue - previousHigh, qdp);

            if (STFenabled)
            {
                var longPosition = Positions.Find("Buy", SymbolName, TradeType.Buy);
                var shortPosition = Positions.Find("Sell", SymbolName, TradeType.Sell);
                //----------------------SHORT-------------------------------------
                //Print("Previous close: {0}, haHigh: {1} haLow: {2}", previousClose, haHigh, haLow);
                if (previousClose < haLow)
                {
                    //Print("Long " + hasCrossedBelow + " " + LotSize);
                    hasCrossedAbove = false;

                    if (MarketSeries.Open.LastValue < previousLow)
                    {
                        gapDown = true;
                        Print("Gap Down" + " " + gapDownSize);
                        if (gapDownSize > gapThreshold)
                        {
                            hasCrossedBelow = true;
                        }
                    }

                    if (longPosition == null && hasCrossedBelow == false)
                    {
                        if (volatility <= volatilityThreshold)
                        {
                            //Print(" Volatility : {0}; Volatility Threshold:{1}", volatility, volatilityThreshold);
                            string trade = "Currency: " + Symbol.Name + " Trade : SHORT " + "Entry: " + previousClose + " Stop Loss:  " + ShortStopLoss + " Lots: " + LotSize;
                            Chart.DrawIcon("Sell Signal", ChartIconType.DownArrow, index, previousHigh, Color.Gold);
                            //Print("Stoploss: {1}; Lots: {0}", Short_volume, ShortStopLoss);
                            var sell = ExecuteMarketOrder(TradeType.Sell, SymbolName, 10, "Sell", ShortStopLoss, null);
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

                    if (MarketSeries.Open.LastValue > previousHigh)
                    {
                        gapUp = true;
                        Print("Gap Up" + " " + gapUpSize);

                        if (gapUpSize > gapThreshold)
                        {
                            hasCrossedAbove = true;
                        }
                    }

                    if (longPosition == null && hasCrossedAbove == false)
                    {
                        if (volatility <= volatilityThreshold)
                        {
                            //Print(" Volatility : {0}; Volatility Threshold:{1}", volatility, volatilityThreshold);
                            string trade = "Currency: " + Symbol.Name + " Trade : LONG " + "Entry: " + previousClose + " Stop Loss:  " + LongStopLoss + " Lots: " + LotSize;
                            Chart.DrawIcon("Buy Signal", ChartIconType.UpArrow, index, previousLow, Color.Gold);

                            //Print("Stoploss: {1}; Lots: {0}", Long_volume, LongStopLoss);
                            var buy = ExecuteMarketOrder(TradeType.Buy, SymbolName, 10, "Buy", LongStopLoss, null);
                            hasCrossedAbove = true;
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
                    if (longPosition != null)
                    {
                        double newLongStopLoss = haLow;
                        ModifyPosition(longPosition, newLongStopLoss, null);

                    }
                }
            }



        }

        private void PositionClosed(PositionClosedEventArgs args)
        {
            var pos = args.Position;
            if (pos.Label == "Buy" && pos.NetProfit < 0)
            {
                hasCrossedAbove = false;
            }
            if (pos.Label == "Sell" && pos.NetProfit < 0)
            {
                hasCrossedBelow = false;
            }
        }

        protected override void OnStop()
        {
            Stop();
        }
    }
}

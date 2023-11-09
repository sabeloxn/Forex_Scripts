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
        [Parameter("Trail", DefaultValue = false)]
        public bool trail { get; set; }
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

//----------------------------------------------------------RANGE PARAMETERS------------------------------------------------------------------
        [Parameter("Range enabled")]
        public bool range { get; set; }
//--------------------------------------------------------------------------------------------------------------------------------------------
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

        protected override void OnStart()
        {
            myIndicator = Indicators.GetIndicator<Blackgoldindi2>(xLot, xStop, qdp, Periods, MAType, stopBuffer, volatilityThreshold, longBuffer, shortBuffer, stopLoss,
            takeProfit, STFenabled, STF, stfPeriods, stfvolatilityThreshold, stfstopBuffer, stflongBuffer, stfshortBuffer, range);
            series = MarketData.GetSeries(STF);

            hasCrossedAbove = true;
            hasCrossedBelow = true;
        }

        protected override void OnBar()
        {
            // Put your core logic here
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
            var previousHigh = MarketSeries.High.LastValue;
            var previousLow = MarketSeries.Low.Last(1);



            double LongStopLoss = Math.Round(Math.Abs(previousClose % haLow) * xStop, qdp);
            double ShortStopLoss = Math.Round(Math.Abs(haHigh % previousClose) * xStop, qdp);

            LotSize = Symbol.NormalizeVolumeInUnits(Math.Ceiling(Account.Balance * xLot) / 100, RoundingMode.ToNearest);
            //Print("Lots: {0}", LotSize);
            //int Lot = Convert.ToInt32(Risk.ToString().Substring(0, 3));
            //LotSize = Symbol.QuantityToVolumeInUnits(Convert.ToInt32(Lot) * xLot);
            double volatility = Math.Round(haHigh - haLow, qdp);
            //Print(" Volatility : {0}; Volatility Threshold:{1}", volatility, volatilityThreshold);
            //Print("Previous close: {0}, haHigh: {1} haLow: {2}", previousClose, haHigh, haLow);

            if (STFenabled)
            {
                var longPosition = Positions.Find("Buy", SymbolName, TradeType.Buy);
                var shortPosition = Positions.Find("Sell", SymbolName, TradeType.Sell);
                //----------------------LONG-------------------------------------
                //Print("Previous close: {0}, haHigh: {1} haLow: {2}", previousClose, haHigh, haLow);
                if (previousClose < haLow)
                {
                    //Print("Long " + hasCrossedBelow + " " + LotSize);
                    hasCrossedAbove = false;
                    if (longPosition == null && hasCrossedBelow == false)
                    {
                        // Print(" Volatility : {0}; Volatility Threshold:{1}", volatility, volatilityThreshold);
                        if (volatility <= volatilityThreshold)
                        {
                            Print("Previous close: {0}, haHigh: {1} == SELL Stoploss: {2}", previousClose, haHigh, ShortStopLoss);
                            //Print("Lots: {0}", LotSize);
                            //Chart.DrawIcon("Sell Signal", ChartIconType.DownArrow, today, previousHigh, Color.Gold);
                            //Print("Risk: {0}; Lot: {1}; Lot Size:{2}", Risk, Lot, LotSize);
                            //string trade = "Currency: " + Symbol.Name + " Trade : SHORT " + "Entry: " + previousClose + " Stop Loss:  " + ShortStopLoss + " Lots: " + LotSize;
                            //Chart.DrawStaticText(SymbolName, trade, VerticalAlignment.Top, HorizontalAlignment.Left, Color.GhostWhite);
                            //Chart.DrawIcon("Sell Signal", ChartIconType.UpArrow, index, previousHigh, Color.Gold);
                            //Print("Short losses: {0}", stCount);
                            var buy = ExecuteMarketOrder(TradeType.Buy, SymbolName, LotSize, "Buy", stopLoss, takeProfit);
                            hasCrossedBelow = true;
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

                //------------------------SHORT-------------------------------------------------------------
                //Print("Previous close: {0}, stfhaHigh: {1} == BUY stfStoploss: {2}", previousClose, stfhaHigh, stfLongStopLoss);
                if (previousClose > haHigh)
                {

                    hasCrossedBelow = false;

                    if (shortPosition == null && hasCrossedAbove == false)
                    {

                        if (volatility <= volatilityThreshold)
                        {
                            Print("Previous close: {0}, haHigh: {1} == BUY Stoploss: {2}", previousClose, haHigh, LongStopLoss);
                            //Print("Lots: {0}", LotSize);
                            //Print("this: " + Symbol.NormalizeVolumeInUnits(LotSize, RoundingMode.Down));
                            //Chart.DrawIcon("Buy Signal", ChartIconType.UpArrow, today, previousLow, Color.DodgerBlue);
                            //Print("Long losses: {0}", lsCount);
                            //Print("Risk: {0}; Lot: {1}; Lot Size:{2}", Risk, Lot, LotSize);
                            //string trade = "Currency: " + Symbol.Name + " Trade : LONG " + "Entry: " + previousClose + " Stop Loss:  " + LongStopLoss + " Lots: " + LotSize;
                            //Chart.DrawStaticText(SymbolName, trade, VerticalAlignment.Top, HorizontalAlignment.Left, Color.GhostWhite);
                            //Chart.DrawIcon("Buy Signal", ChartIconType.UpArrow, index, previousLow, Color.Gold);
                            var sell = ExecuteMarketOrder(TradeType.Sell, SymbolName, LotSize, "Sell", stopLoss, takeProfit);
                            hasCrossedAbove = true;
                        }
                    }
                }
                //-----------------TRAILING STOP 
                if (trail == true)
                {
                    if (shortPosition != null)
                    {
                        double newShortStopLoss = haHigh;
                        ModifyPosition(shortPosition, newShortStopLoss, null);


                    }
                }
            }
        }

        protected override void OnStop()
        {
            Stop();
        }
    }
}

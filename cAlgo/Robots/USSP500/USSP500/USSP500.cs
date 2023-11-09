using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class USSP500 : Robot
    {

        [Parameter(DefaultValue = 18, MinValue = 1)]
        public int Periods { get; set; }

        [Parameter("Volatility ", DefaultValue = 0.3, MinValue = 0.01)]
        public double volatilityThreshold { get; set; }

        [Parameter(DefaultValue = 17, MinValue = 1)]
        public int stfPeriods { get; set; }

        [Parameter("MA Type", DefaultValue = MovingAverageType.Exponential)]
        public MovingAverageType MAType { get; set; }

        [Parameter("STF")]
        public TimeFrame STF { get; set; }

        [Parameter("Enable STF")]
        public bool STFenabled { get; set; }

        private BlackgoldIndicator myIndicator;

        private int LotSize;
        //private double stopLoss;
        public bool hasCrossedAbove;
        public bool hasCrossedBelow;

        protected override void OnStart()
        {
            myIndicator = Indicators.GetIndicator<BlackgoldIndicator>(Periods, stfPeriods, MAType, STFenabled, STF);
            //stopLoss = 100;
            //volatilityThreshold = 2.0;
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

            var haOpen = (smaClose + smaOpen) / 2;
            var haClose = (smaOpen + smaClose + smaHigh + smaLow) / 4;
            var haHigh = Math.Round(Math.Max(smaHigh, Math.Max(haOpen, haClose)), 5);
            var haLow = Math.Round(Math.Min(smaLow, Math.Min(haOpen, haClose)), 5);

            var previousClose = MarketSeries.Close.LastValue;
            var previousHigh = MarketSeries.High.LastValue;
            var previousLow = MarketSeries.Low.Last(1);

            var longPosition = Positions.Find("Buy", SymbolName, TradeType.Buy);
            var shortPosition = Positions.Find("Sell", SymbolName, TradeType.Sell);


            double LongStopLoss = Math.Round(Math.Abs(previousClose - haLow), 2);
            double ShortStopLoss = Math.Round(Math.Abs(previousClose - haHigh), 2);
            double volatility = Math.Round(haHigh - haLow, 2);

            // double Risk = Math.Ceiling(Account.Balance * 1) / 100;
            //int Lot = Convert.ToInt32(Risk.ToString().Substring(0, 3));
            LotSize = Convert.ToInt32(Account.Balance / 1000);

            //Convert.ToInt32((Account.Balance * 10) / 100) * 100;
            //------------------------LONG-------------------------------------------------------------
            if (previousClose > haHigh)
            {
                Print("Volatility Threshold:{1} ; Long Volatility : {0}", volatility, volatilityThreshold);

                hasCrossedBelow = false;

                if (longPosition == null && hasCrossedAbove == false)
                {

                    if (volatility <= volatilityThreshold)
                    {

                        Print("Previous close: {0}, haHigh: {1} == BUY Stoploss: {2}", previousClose, haHigh, LongStopLoss);
                        Print("Lots: {0}", LotSize);

                        var buy = ExecuteMarketOrder(TradeType.Buy, SymbolName, LotSize, "Buy", LongStopLoss, null);

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
                Print("Volatility Threshold:{1} ; Short Volatility : {0}", volatility, volatilityThreshold);

                hasCrossedAbove = false;
                if (shortPosition == null && hasCrossedBelow == false)
                {

                    if (volatility <= volatilityThreshold)
                    {

                        Print("Previous close: {0}, haLow: {1} == SELL Stoploss: {2}", previousClose, haLow, ShortStopLoss);
                        Print("Lots: {0}", LotSize);
                        var sell = ExecuteMarketOrder(TradeType.Sell, SymbolName, LotSize, "Sell", ShortStopLoss, null);
                    }
                    ;
                }
                //-----------------TRAILING STOP 
                if (shortPosition != null)
                {
                    double newShortStopLoss = haHigh;
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

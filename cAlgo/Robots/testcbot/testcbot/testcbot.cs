using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class testcbot : Robot
    {
        [Parameter(DefaultValue = 5, MinValue = 1)]
        public int Periods { get; set; }

        [Parameter("MA Type", DefaultValue = MovingAverageType.Simple)]
        public MovingAverageType MAType { get; set; }
        private testindi myIndicator;

        [Parameter(DefaultValue = 0.0)]
        public double Parameter { get; set; }
        public bool hasCrossedAbove { get; set; }
        public bool hasCrossedBelow { get; set; }

        private double LotSize;
        private int stopLoss;
        private int takeprofit;

        protected override void OnStart()
        {
            // Put your initialization logic here
            myIndicator = Indicators.GetIndicator<testindi>(Periods, MAType);
            LotSize = 10000;
            stopLoss = 100;
            takeprofit = 35;
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
            var haHigh = Math.Max(smaHigh, Math.Max(haOpen, haClose));
            var haLow = Math.Min(smaLow, Math.Min(haOpen, haClose));

            var previousClose = MarketSeries.Close.LastValue;

            var longPosition = Positions.Find("Buy", SymbolName, TradeType.Buy);
            var shortPosition = Positions.Find("Sell", SymbolName, TradeType.Sell);

            if (previousClose > haHigh)
            {
                hasCrossedBelow = false;
                if (longPosition == null && hasCrossedAbove == false)
                {


                    var buy = ExecuteMarketOrder(TradeType.Buy, SymbolName, LotSize, "Buy", stopLoss, takeprofit);
                    hasCrossedAbove = true;
                }
            }
            ;
            if (previousClose < haLow)
            {
                hasCrossedAbove = false;

                if (shortPosition == null && hasCrossedBelow == false)
                {
                    var sell = ExecuteMarketOrder(TradeType.Sell, SymbolName, LotSize, "Sell", stopLoss, takeprofit);
                    hasCrossedBelow = true;
                }
            }
            ;
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
            Stop();
        }
    }
}

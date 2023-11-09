using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class DayTrader : Robot
    {
        private Bars dailySeries;

        [Parameter("Take Profit", DefaultValue = 100)]
        public int takeProfit { get; set; }

        private Showweekdays _myIndicator;
        
        public bool SellOrderExecuted;
        public bool BuyOrderExecuted;
        double ShortEntry ;
        double LongEntry;
        public double LastLongEntry;
        public double LastShortEntry;
        double dayClose;
        double lotSize;
        
        protected override void OnStart()
        {
            dailySeries = MarketData.GetBars(TimeFrame.Daily);
            _myIndicator = Indicators.GetIndicator<Showweekdays>();
        }

        protected override void OnBar()
        {

            lotSize = 100000;

            double dayOpen = Bars.OpenPrices.Last(1);
            dayClose = Bars.ClosePrices.Last(1);
            double dayHigh = Bars.HighPrices.Last(1);
            double dayLow = Bars.LowPrices.Last(1);
            var pdayLow = Bars.LowPrices;

            var prevDayHigh = dayHigh;
            var prevDayLow = pdayLow.Last(1);

            ShortEntry = _myIndicator.ShortEntry.LastValue;
            LongEntry = _myIndicator.LongEntry.LastValue;
            
            //Print("Short Entry : {0}", ShortEntry);
            //Print("Long Entry : {0}", LongEntry);
            //Print("open {0} close {1} Low {2} High {3}", dayOpen, dayClose, prevDayLow, prevDayHigh);

            OpenPosition();
            
        }
        void OpenPosition()
        {
        //----Long
            if (dayClose > LongEntry )//&& !BuyOrderExecuted)//(LongEntry!=LastLongEntry))
            {
                SellOrderExecuted=false;
                double stopLoss = (LongEntry-ShortEntry)*10000;
                var buy = ExecuteMarketOrder(TradeType.Buy, SymbolName, lotSize, "Buy", stopLoss, 100);
                BuyOrderExecuted=true;
                LastLongEntry=LongEntry;
            }
            
            //----Short
            if (dayClose < ShortEntry )//&& !SellOrderExecuted)//(ShortEntry != LastShortEntry))
            {
                BuyOrderExecuted=false;
                double stopLoss = (LongEntry-ShortEntry)*10000;
                var sell = ExecuteMarketOrder(TradeType.Sell, SymbolName, lotSize, "Sell", stopLoss, 100);
                SellOrderExecuted=true;
                LastShortEntry=ShortEntry;
            }
        }
    }
}

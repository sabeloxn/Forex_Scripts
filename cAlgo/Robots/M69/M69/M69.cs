using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using System.Collections;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class M69 : Robot
    {
        private DailyHighLow dhl;
        protected override void OnStart()
        {
            // Put your initialization logic here
            dhl = Indicators.GetIndicator<DailyHighLow>();
        }

        protected override void OnBar()
        {
            // Put your core logic here
            int index = MarketSeries.Close.Count - 1;
            var high = dhl.high;
            var low = dhl.low;
            var open = MarketSeries.Open.LastValue;
            var close = MarketSeries.Close.LastValue;

            bool bullishBar = close > open;
            bool bearishBar = close < open;

            var Bearhigh = dhl.bearHighs.LastValue;
            var Bearlow = dhl.bearLows.LastValue;
            var Bullhigh = dhl.bullHighs.LastValue;
            var Bulllow = dhl.bullLows.LastValue;

            double LotSize = (long)Math.Ceiling(Account.Balance / 10) * 100;



            double shortStop = Bullhigh;
            Print("Previous short stop removed New Short stop candidate = {0}", shortStop);

            double shortEntryCandidate = Bulllow;
            Print("Previous Short Entry removed New entry candidate = {0}", shortEntryCandidate);

            bool isshortEntry = close < shortEntryCandidate;
            Print("is Short Entry parameters: Close{0} Short Entry Candidate{1}", close, shortEntryCandidate);

            if (isshortEntry)
            {
                Print("Short position parameters");
                PlaceLimitOrder(TradeType.Sell, SymbolName, LotSize, shortEntryCandidate, "Sell Limit", shortStop, 80);
            }




            if (bullishBar)
            {

                double longStop = Bearlow;
                Print("Previous long stop removed New Long stop candidate = {0}", longStop);

                double longEntryCandidate = Bearhigh;
                Print("Previous Long Entry removed New entry candidate = {0}", longEntryCandidate);

                bool islongEntry = close > longEntryCandidate;
                Print("is long Entry parameters: Close{0} Long Entry Candidate{1}", close, longEntryCandidate);

                if (islongEntry)
                {
                    Print("Long position parameters");
                    PlaceLimitOrder(TradeType.Buy, SymbolName, LotSize, longEntryCandidate, "BuyLimit", longStop, 80);

                }
            }
        }


        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}

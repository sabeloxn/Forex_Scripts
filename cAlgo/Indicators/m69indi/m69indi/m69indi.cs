using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;
using System.Collections;

namespace cAlgo
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class m69indi : Indicator
    {

        public double high;
        public double low;
        public double open;
        public double close;
        public int lastCalculatedIdx = -1;

        public DataSeries bullHighs;
        public DataSeries bearHighs;
        public DataSeries bearLows;
        public DataSeries bullLows;

        private ArrayList lastBullHighCache, lastBullLowCache;
        private ArrayList lastBearHighCache, lastBearLowCache;

        protected override void Initialize()
        {
            // Initialize and create nested indicators
            lastBullHighCache = new ArrayList();
            lastBullLowCache = new ArrayList();
            lastBearHighCache = new ArrayList();
            lastBearLowCache = new ArrayList();

            bullLows = MarketSeries.Low;
            bullHighs = MarketSeries.High;
            bearLows = MarketSeries.Low;
            bearHighs = MarketSeries.High;


        }

        public override void Calculate(int index)
        {
            // Calculate value at specified index
            // Result[index] = ...
            high = MarketSeries.High.Last(1);
            low = MarketSeries.Low.Last(1);
            open = MarketSeries.Open.Last(1);
            close = MarketSeries.Close.Last(1);

            double prevDayHigh = MarketSeries.High.Last(2);
            double prevDayLow = MarketSeries.Low.Last(2);
            double prevDayOpen = MarketSeries.Open.Last(2);
            double prevDayClose = MarketSeries.Close.Last(2);



            bool bullishBar = close > open;
            bool bearishBar = close < open;

            bool prevDayBullBar = prevDayClose > prevDayOpen;
            bool prevDayBearBar = prevDayClose < prevDayOpen;

            //bool longEntry = close < (double)lastLowCache[0];
            //bool shortEntry = close > (double)lastHighCache[0];

            bool gotANewIdxSoANewBar = index > lastCalculatedIdx;

            if (gotANewIdxSoANewBar)
            {
                lastCalculatedIdx = index;
                // Do some calculations

                if (bearishBar)
                {
                    lastBearHighCache.Add(bearHighs.Last(0));
                    lastBearLowCache.Add(bearLows.Last(0));

                    if (lastBearHighCache.Count > 1)
                    {
                        lastBearHighCache.RemoveAt(0);
                        lastBearLowCache.RemoveAt(0);
                        double longStop = (double)lastBearLowCache[0];
                        double longEntryCandidate = (double)lastBearHighCache[0];
                        Print("Previous Bearish Entry removed New entry candidate = {0}", longEntryCandidate);
                    }

                }

                if (bullishBar)
                {
                    lastBullHighCache.Add(bullHighs.Last(0));
                    lastBullLowCache.Add(bullLows.Last(0));

                    if (lastBullLowCache.Count > 1)
                    {
                        lastBullLowCache.RemoveAt(0);
                        lastBullHighCache.RemoveAt(0);
                        double shortEntryCandidate = (double)lastBullLowCache[0];
                        double shortStop = (double)lastBullHighCache[0];
                        Print("Previous Bullish Entry removed New entry candidate = {0}", shortEntryCandidate);
                    }

                }
            }

        }
    }
}

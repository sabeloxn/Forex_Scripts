using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;
using System.Collections;
namespace cAlgo
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Mustang69 : Indicator
    {
        public DataSeries bullHighs;
        public DataSeries bearHighs;
        public DataSeries bearLows;
        public DataSeries bullLows;
        public int strength = 1;
        public int lastCalculatedIdx = -1;
        //[Parameter("Strength", DefaultValue = 5, MinValue = 1)]
        //public int strength { get; set; }

        [Output("bullHigh", LineColor = "Green", Thickness = 2, PlotType = PlotType.Points)]
        public IndicatorDataSeries BullHighPlot { get; set; }

        [Output("bearHigh", LineColor = "Yellow", Thickness = 2, PlotType = PlotType.Points)]
        public IndicatorDataSeries BearHighPlot { get; set; }

        [Output("bearLow", LineColor = "Blue", Thickness = 2, PlotType = PlotType.Points)]
        public IndicatorDataSeries BearLowPlot { get; set; }

        [Output("BullLow", LineColor = "Orange", Thickness = 2, PlotType = PlotType.Points)]
        public IndicatorDataSeries BullLowPlot { get; set; }


        private double currentBullSwingHigh = 0;
        private double currentBearSwingHigh = 0;

        private double currentBullSwingLow = 0;
        private double currentBearSwingLow = 0;

        private double lastBullSwingHighValue = 0;
        private double lastBearSwingHighValue = 0;

        private double lastBullSwingLowValue = 0;
        private double lastBearSwingLowValue = 0;

        private int CurrentBullBar, bullCount;
        private int CurrentBearBar, bearCount;

        private int saveCurrentBullBar = -1;
        private int saveCurrentBearBar = -1;

        private ArrayList lastBullHighCache, lastBullLowCache;
        private ArrayList lastBearHighCache, lastBearLowCache;

        private IndicatorDataSeries bullswingHighSeries, bullswingHighSwings, bullswingLowSeries, bullswingLowSwings;
        private IndicatorDataSeries bearswingHighSeries, bearswingHighSwings, bearswingLowSeries, bearswingLowSwings;
        //private MarketSeries series;
        protected override void Initialize()
        {
            lastBullHighCache = new ArrayList();
            lastBullLowCache = new ArrayList();
            lastBearHighCache = new ArrayList();
            lastBearLowCache = new ArrayList();
            //series = MarketData.GetSeries(TimeFrame.Daily);
            bullswingHighSeries = CreateDataSeries();
            bullswingHighSwings = CreateDataSeries();
            bullswingLowSeries = CreateDataSeries();
            bullswingLowSwings = CreateDataSeries();

            bearswingHighSeries = CreateDataSeries();
            bearswingHighSwings = CreateDataSeries();
            bearswingLowSeries = CreateDataSeries();
            bearswingLowSwings = CreateDataSeries();

            bullHighs = MarketSeries.High;
            bullLows = MarketSeries.Low;
            bearHighs = MarketSeries.High;
            bearLows = MarketSeries.Low;
        }

        public override void Calculate(int index)
        {

            bool gotANewIdxSoANewBar = index > lastCalculatedIdx;

            if (gotANewIdxSoANewBar)
            {
                lastCalculatedIdx = index;

                double high = MarketSeries.High.Last(1);
                double low = MarketSeries.Low.Last(1);
                double open = MarketSeries.Open.Last(1);
                double close = MarketSeries.Close.Last(1);

                double prevDayHigh = MarketSeries.High.Last(2);
                double prevDayLow = MarketSeries.Low.Last(2);
                double prevDayOpen = MarketSeries.Open.Last(2);
                double prevDayClose = MarketSeries.Close.Last(2);



                bool bullishBar = close > open;
                bool bearishBar = close < open;

                bool prevDayBullBar = prevDayClose > prevDayOpen;
                bool prevDayBearBar = prevDayClose < prevDayOpen;
//-------------------------------------------------------------------------------------------------------
                CurrentBullBar = index;
                CurrentBearBar = index;

                bullCount = bullHighs.Count;
                bearCount = bearHighs.Count;

                if (saveCurrentBullBar != CurrentBullBar)
                {
                    bullswingHighSwings[index] = 0;
                    bullswingLowSwings[index] = 0;
                    bullswingHighSeries[index] = 0;
                    bullswingLowSeries[index] = 0;
                    lastBullHighCache.Add(bullHighs.Last(0));

                    if (bullishBar)
                    {
                        lastBullLowCache.Add(bullLows.Last(0));
                        lastBullHighCache.Add(bullHighs.Last(0));

                        if (lastBullHighCache.Count > 1)
                            lastBullHighCache.RemoveAt(0);
                        if (lastBullLowCache.Count > 1)
                            lastBullLowCache.RemoveAt(0);
                        Print("yyyyyyyyyyy");

                        if (lastBullHighCache.Count == 1)
                        {
                            bool isSwingHigh = true;
                            double swingHighCandidateValue = (double)lastBullHighCache[strength];
                            for (int i = 0; i < 1; i++)
                                if ((double)lastBullHighCache[i] >= swingHighCandidateValue - double.Epsilon)
                                    isSwingHigh = false;

                            for (int i = 1 + 1; i < lastBullHighCache.Count; i++)
                                if ((double)lastBullHighCache[i] > swingHighCandidateValue - double.Epsilon)
                                    isSwingHigh = false;

                            bullswingHighSwings[index - 1] = isSwingHigh ? swingHighCandidateValue : 0.0;
                            if (isSwingHigh)
                                lastBullSwingHighValue = swingHighCandidateValue;

                            if (isSwingHigh)
                            {
                                currentBullSwingHigh = swingHighCandidateValue;
                                for (int i = 0; i <= 1; i++)
                                    BullHighPlot[index - i] = currentBullSwingHigh;
                            }
                            else if (bullHighs.Last(0) > currentBullSwingHigh)
                            {
                                currentBullSwingHigh = 0.0;
                                BullHighPlot[index] = double.NaN;
                            }
                            else
                                BullHighPlot[index] = currentBullSwingHigh;

                            if (isSwingHigh)
                            {
                                for (int i = 0; i <= 1; i++)
                                    bullswingHighSeries[index - i] = lastBullSwingHighValue;
                            }
                            else
                            {
                                bullswingHighSeries[index] = lastBullSwingHighValue;
                            }
                        }

                        if (lastBullLowCache.Count == 1)
                        {
                            bool isSwingLow = true;
                            double swingLowCandidateValue = (double)lastBullLowCache[strength];
                            for (int i = 0; i < 1; i++)
                                if ((double)lastBullLowCache[i] <= swingLowCandidateValue + double.Epsilon)
                                    isSwingLow = false;

                            for (int i = 1 + 1; i < lastBullLowCache.Count; i++)
                                if ((double)lastBullLowCache[i] < swingLowCandidateValue + double.Epsilon)
                                    isSwingLow = false;

                            bullswingLowSwings[index - 1] = isSwingLow ? swingLowCandidateValue : 0.0;
                            if (isSwingLow)
                                lastBullSwingLowValue = swingLowCandidateValue;

                            if (isSwingLow)
                            {
                                currentBullSwingLow = swingLowCandidateValue;
                                for (int i = 0; i <= strength; i++)
                                    BullLowPlot[index - i] = currentBullSwingLow;
                            }
                            else if (bullLows.Last(0) < currentBullSwingLow)
                            {
                                currentBullSwingLow = double.MaxValue;
                                BullLowPlot[index] = double.NaN;
                            }
                            else
                                BullLowPlot[index] = currentBullSwingLow;

                            if (isSwingLow)
                            {
                                for (int i = 0; i <= strength; i++)
                                    bullswingLowSeries[index - i] = lastBullSwingLowValue;
                            }
                            else
                            {
                                bullswingLowSeries[index] = lastBullSwingLowValue;
                            }
                        }

                        saveCurrentBullBar = CurrentBullBar;
                    }
                    else
                    {
                        if (bullHighs.Last(0) > bullHighs.Last(strength) && bullswingHighSwings.Last(strength) > 0.0)
                        {
                            bullswingHighSwings[index - strength] = 0.0;
                            for (int i = 0; i <= strength; i++)
                                BullHighPlot[index - i] = double.NaN;
                            currentBullSwingHigh = 0.0;
                        }
                        else if (bullHighs.Last(0) > bullHighs.Last(strength) && currentBullSwingHigh != 0.0)
                        {
                            BullHighPlot[index] = double.NaN;
                            currentBullSwingHigh = 0.0;
                        }
                        else if (bullHighs.Last(0) <= currentBullSwingHigh)
                            BullHighPlot[index] = currentBullSwingHigh;

                        if (bullLows.Last(0) < bullLows.Last(strength) && bullswingLowSwings.Last(strength) > 0.0)
                        {
                            bullswingLowSwings[index - strength] = 0.0;
                            for (int i = 0; i <= strength; i++)
                                BullLowPlot[index - i] = double.NaN;
                            currentBullSwingLow = double.MaxValue;
                        }
                        else if (bullLows.Last(0) < bullLows.Last(strength) && currentBullSwingLow != double.MaxValue)
                        {
                            BullLowPlot[index] = double.NaN;
                            currentBullSwingLow = double.MaxValue;
                        }
                        else if (bullLows.Last(0) >= currentBullSwingLow)
                            BullLowPlot[index] = currentBullSwingLow;
                    }
                }

                //---------------------------------BEARISH------------------------------------------------------------------

                if (bearishBar)
                {
                    lastBearLowCache.Add(bearLows.Last(0));
                    lastBearHighCache.Add(bearHighs.Last(0));

                    if (lastBearLowCache.Count > 1)
                        lastBearHighCache.RemoveAt(0);

                    if (lastBearHighCache.Count > 1)
                        lastBearLowCache.RemoveAt(0);
                    Print("yyyyyyyyyyyxxxxxxxxxxx");

                    if (lastBearHighCache.Count == 1)
                    {
                        bool isSwingHigh = true;
                        double swingHighCandidateValue = (double)lastBearHighCache[0];
                        for (int i = 0; i < strength; i++)
                            if ((double)lastBearHighCache[i] >= swingHighCandidateValue - double.Epsilon)
                                isSwingHigh = false;

                        for (int i = strength + 1; i < lastBearHighCache.Count; i++)
                            if ((double)lastBearHighCache[i] > swingHighCandidateValue - double.Epsilon)
                                isSwingHigh = false;

                        bearswingHighSwings[index - strength] = isSwingHigh ? swingHighCandidateValue : 0.0;
                        if (isSwingHigh)
                            lastBearSwingHighValue = swingHighCandidateValue;

                        if (isSwingHigh)
                        {
                            currentBearSwingHigh = swingHighCandidateValue;
                            for (int i = 0; i <= strength; i++)
                                BearHighPlot[index - i] = currentBearSwingHigh;
                        }
                        else if (bearHighs.Last(0) > currentBearSwingHigh)
                        {
                            currentBearSwingHigh = 0.0;
                            BearHighPlot[index] = double.NaN;
                        }
                        else
                            BearHighPlot[index] = currentBearSwingHigh;

                        if (isSwingHigh)
                        {
                            for (int i = 0; i <= strength; i++)
                                bearswingHighSeries[index - i] = lastBearSwingHighValue;
                        }
                        else
                        {
                            bearswingHighSeries[index] = lastBearSwingHighValue;
                        }
                    }

                    if (lastBearLowCache.Count == 1)
                    {
                        bool isSwingLow = true;
                        double swingLowCandidateValue = (double)lastBearLowCache[0];
                        for (int i = 0; i < strength; i++)
                            if ((double)lastBearLowCache[i] <= swingLowCandidateValue + double.Epsilon)
                                isSwingLow = false;

                        for (int i = strength + 1; i < lastBearLowCache.Count; i++)
                            if ((double)lastBearLowCache[i] < swingLowCandidateValue + double.Epsilon)
                                isSwingLow = false;

                        bearswingLowSwings[index - strength] = isSwingLow ? swingLowCandidateValue : 0.0;
                        if (isSwingLow)
                            lastBearSwingLowValue = swingLowCandidateValue;

                        if (isSwingLow)
                        {
                            currentBearSwingLow = swingLowCandidateValue;
                            for (int i = 0; i <= strength; i++)
                                BearLowPlot[index - i] = currentBearSwingLow;
                        }
                        else if (bearLows.Last(0) < currentBearSwingLow)
                        {
                            currentBearSwingLow = double.MaxValue;
                            BearHighPlot[index] = double.NaN;
                        }
                        else
                            BearHighPlot[index] = currentBearSwingLow;

                        if (isSwingLow)
                        {
                            for (int i = 0; i <= strength; i++)
                                bearswingLowSeries[index - i] = lastBearSwingLowValue;
                        }
                        else
                        {
                            bearswingLowSeries[index] = lastBearSwingLowValue;
                        }
                    }

                    saveCurrentBearBar = CurrentBearBar;
                }
                else
                {
                    if (bearHighs.Last(0) > bearHighs.Last(strength) && bearswingHighSwings.Last(strength) > 0.0)
                    {
                        bearswingHighSwings[index - strength] = 0.0;
                        for (int i = 0; i <= strength; i++)
                            BearHighPlot[index - i] = double.NaN;
                        currentBearSwingHigh = 0.0;
                    }
                    else if (bearHighs.Last(0) > bearHighs.Last(strength) && currentBearSwingHigh != 0.0)
                    {
                        BearHighPlot[index] = double.NaN;
                        currentBearSwingHigh = 0.0;
                    }
                    else if (bearHighs.Last(0) <= currentBearSwingHigh)
                        BearHighPlot[index] = currentBearSwingHigh;

                    if (bearLows.Last(0) < bearLows.Last(strength) && bearswingLowSwings.Last(strength) > 0.0)
                    {
                        bearswingLowSwings[index - strength] = 0.0;
                        for (int i = 0; i <= strength; i++)
                            BearLowPlot[index - i] = double.NaN;
                        currentBearSwingLow = double.MaxValue;
                    }
                    else if (bearLows.Last(0) < bearLows.Last(strength) && currentBearSwingLow != double.MaxValue)
                    {
                        BearLowPlot[index] = double.NaN;
                        currentBearSwingLow = double.MaxValue;
                    }
                    else if (bearLows.Last(0) >= currentBearSwingLow)
                        BearLowPlot[index] = currentBearSwingLow;
                }

            }
        }

    }

}





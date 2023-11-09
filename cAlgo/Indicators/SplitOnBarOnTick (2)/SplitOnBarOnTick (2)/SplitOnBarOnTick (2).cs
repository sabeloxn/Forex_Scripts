//Clean
using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;
using System.Collections.Generic;
using System.Net;
namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = true, AccessRights = AccessRights.None)]
    
    public class CleanChartv110 : Indicator
    {

        public double High { get; set; }
        public double Low { get; set; }
        public double Open { get; set; }
        public double Close { get; set; }

        public double Bearhigh { get; set; }
        public double Bearlow { get; set; }
        public double Bullhigh { get; set; }
        public double Bulllow { get; set; }

        public double PrevDayClose { get; set; }
        public double PrevDayOpen { get; set; }
        public double PrevDayHigh { get; set; }
        public double PrevDayLow { get; set; }

        public IndicatorDataSeries bullHighs;
        public IndicatorDataSeries bearHighs;
        public IndicatorDataSeries bearLows;
        public IndicatorDataSeries bullLows;
        public Bars dailySeries;

        public double ShortEntry;
        public double LongEntry;
        
        public List<int> BearIndexes=new List <int>{};
        public int[] BearIndexesArr = new int[]{};
        
        public List<int> BullIndexes=new List <int>{};
        public int[] BullIndexesArr = new int[]{};
        
        bool BearSignalNotSent=true;
        bool BullSignalNotSent=true;
         
        [Parameter("Timeframe", DefaultValue = "Daily")]
        public TimeFrame iTimeFrame { get; set; }

        public int lastCalculatedIdx = -1;
        
        [Output("Short Main",LineColor = "#FFFF999A",PlotType =PlotType.Points, Thickness = 5)]
        public IndicatorDataSeries SellSignal{get;set;}
        [Output("Long Main",LineColor = "#709CFFF7",PlotType =PlotType.Points, Thickness = 5)]
        public IndicatorDataSeries BuySignal{get;set;}
        
        public bool bullishBar ;
        public bool bearishBar;
        double previousOpen ;
        double previousClose ;
        protected override void Initialize()
        {
            Chart.TryChangeTimeFrame(iTimeFrame);
            dailySeries = MarketData.GetBars(iTimeFrame);
            bullLows = CreateDataSeries();
            bullHighs = CreateDataSeries();
            bearLows = CreateDataSeries();
            bearHighs = CreateDataSeries();
        }
        public override void Calculate(int index)
        {  
            High = dailySeries.HighPrices[index-1];
            Low = dailySeries.LowPrices[index-1];

            previousOpen = dailySeries.OpenPrices[index-1];
            previousClose = dailySeries.ClosePrices[index-1];
                
            bullishBar = previousClose > previousOpen;
            bearishBar = previousClose < previousOpen;

                if (bearishBar)
                {
                    BearIndexes.Add(index-1);
                    
                    Bearhigh = Math.Max(Bearhigh, dailySeries.HighPrices[index-1]);
                    Bearlow = Math.Min(Bearlow, dailySeries.LowPrices[index-1]);
                    bearLows[index] = Math.Min(Bearlow, dailySeries.LowPrices[index-1]);   
                    bearHighs[index] = Math.Max(Bearhigh, dailySeries.HighPrices[index-1]);

                    BearIndexesArr = BearIndexes.ToArray();
                    
                    OnBar(index);
                }
                if (bullishBar)
                {

                    BullIndexes.Add(index-1);
                    
                    Bullhigh = Math.Max(Bullhigh, dailySeries.HighPrices[index-1]);
                    Bulllow = Math.Max(Bulllow, dailySeries.LowPrices[index-1]);
                    bullHighs[index] = Math.Max(Bullhigh, dailySeries.HighPrices[index-1]);
                    bullLows[index] = Math.Max(Bulllow, dailySeries.LowPrices[index-1]);

                    BullIndexesArr = BullIndexes.ToArray();
                    
                    OnBar(index);
                }

        }
        void OnBar(int index)
            {
            Bearhigh = dailySeries.HighPrices.LastValue;
            Bearlow = dailySeries.LowPrices.LastValue;
            Bullhigh = dailySeries.HighPrices.LastValue;
            Bulllow = dailySeries.LowPrices.LastValue;
            
            bool gotANewIdxSoANewBar = index > lastCalculatedIdx;

                if (gotANewIdxSoANewBar)
                {
                    lastCalculatedIdx = index;
                    if (bearishBar)
                    {
                        if (BearIndexesArr.Length>0 )
                        {
                        LongEntry = bearHighs.LastValue;
                        BearSignalNotSent=true;
                        }
                        if(previousClose<ShortEntry & BullSignalNotSent )
                        {
                        SellSignal[index-1]=High;
                        BullSignalNotSent=false;
                        }
                    }
                    if (bullishBar)
                    {
                        if (BullIndexesArr.Length>0 )
                        {
                        ShortEntry = bullLows.LastValue;
                        BullSignalNotSent=true;
                        }
                        if(previousClose>LongEntry & BearSignalNotSent )
                        {
                        BuySignal[index-1]=Low;
                        BearSignalNotSent=false;
                        }
                    }
                }
            }

    }
}

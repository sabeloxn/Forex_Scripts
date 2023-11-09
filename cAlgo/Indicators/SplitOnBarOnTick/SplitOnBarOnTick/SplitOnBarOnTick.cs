//Does not paint on tick
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
        
        private ParabolicSAR _parabolic;
        [Parameter("Min AF", DefaultValue = 0.02)]
        public double minaf { get; set; }
        [Parameter("Max AF", DefaultValue = 0.2)]
        public double maxaf { get; set; }
        [Parameter("Shift",DefaultValue =0)]
        public int PSARshift {get;set;}
        [Output("PSAR Main",LineColor = "#66FFE699",PlotType =PlotType.Points, Thickness = 3)]
        public IndicatorDataSeries PSARResult { get; set; }
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

            _parabolic = Indicators.ParabolicSAR(minaf, maxaf);
        }
        public override void Calculate(int index)
        {
            
                
            PSARResult[index+PSARshift] = _parabolic.Result[index];
            
            DateTime today = dailySeries.OpenTimes[index].Date;
            DateTime todayCheck = dailySeries.OpenTimes[index].Date;
            DateTime tomorrow = today.AddDays(1);
    
            High = dailySeries.HighPrices[index-1];
            Low = dailySeries.LowPrices[index-1];
            Open = dailySeries.OpenPrices[index-1];
            Close = dailySeries.ClosePrices[index-1];
            
            
            
            
            previousOpen = dailySeries.OpenPrices[index-1];
            previousClose = dailySeries.ClosePrices[index-1];
                
            bullishBar = previousClose > previousOpen;
            bearishBar = previousClose < previousOpen;
            var previousLow = Bars.LowPrices.Last(1);
            var previousHigh = Bars.HighPrices.Last(1);
            
            bool priceAboveSAR = Close>PSARResult.LastValue;
            bool priceBelowSAR = Close<PSARResult.LastValue;

            //for (int i = dailySeries.ClosePrices.Count - 1; i > 0; i--)
            //{
            //bool gotANewIdxSoANewBar = index > lastCalculatedIdx;

            //if (gotANewIdxSoANewBar)
            //{
                //lastCalculatedIdx = index;
                //if (dailySeries.OpenTimes[i].Date < todayCheck)
                    //break;
                 //High = Math.Max(High, dailySeries.HighPrices[index]);
                //Low = Math.Min(Low, dailySeries.LowPrices[index]);
                Open = dailySeries.OpenPrices[index];
                Close = dailySeries.ClosePrices[index];
                
                
                
                //Print("Close: "+Close+" Prev Close: "+previousClose);
                
                if (bearishBar)
                {
//Print("Bear: Close: "+Close+" Prev Close: "+previousClose);
                    BearIndexes.Add(index-1);
                    
                    Bearhigh = Math.Max(Bearhigh, dailySeries.HighPrices[index-1]);
                    Bearlow = Math.Min(Bearlow, dailySeries.LowPrices[index-1]);
                    bearLows[index] = Math.Min(Bearlow, dailySeries.LowPrices[index-1]);   
                    bearHighs[index] = Math.Max(Bearhigh, dailySeries.HighPrices[index-1]);
                    //Print(" Last Bear Low "+bearLows.LastValue);
                    BearIndexesArr = BearIndexes.ToArray();
                    
                    OnBar(index);
                }
                    
                    
                
                if (bullishBar)
                {
                //Print("Bull: Close: "+Close+" Prev Close: "+previousClose);
                    BullIndexes.Add(index-1);
                    
                    Bullhigh = Math.Max(Bullhigh, dailySeries.HighPrices[index-1]);
                    Bulllow = Math.Max(Bulllow, dailySeries.LowPrices[index-1]);
                    bullHighs[index] = Math.Max(Bullhigh, dailySeries.HighPrices[index-1]);
                    bullLows[index] = Math.Max(Bulllow, dailySeries.LowPrices[index-1]);
                    //Print(" Long Entries "+bullHighs.LastValue);
                    BullIndexesArr = BullIndexes.ToArray();
                    
                    OnBar(index);
                }
               
            //}
            //}
            
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
                {//Print("Bear: Close: "+Close+" Prev Close: "+previousClose);
                Print("Short Entry: "+ShortEntry +" Previous Close: "+previousClose +" "+(previousClose<ShortEntry));
                    if (BearIndexesArr.Length>0 )//& previousClose>previousOpen)
                    {
                    LongEntry = bearHighs.LastValue;
                    //Print("Long Entry: "+LongEntry);
                    BearSignalNotSent=true;
                    }
                    if(previousClose<ShortEntry & BullSignalNotSent )//& priceBelowSAR)
                    {
                    SellSignal[index-1]=High;
                    //Chart.DrawIcon("Sell Signal"+index, ChartIconType.DownTriangle, index-1, High+0.00, Color.Crimson);
                    BullSignalNotSent=false;
                    }
                }
                if (bullishBar)
                {//Print("Bull: Close: "+(BullIndexesArr.Length>0)+" Prev Close: "+(previousClose<previousOpen));
                    //Print("Long Entry: "+LongEntry +" Previous Close: "+previousClose +" "+(previousClose>LongEntry));
                    
                    if (BullIndexesArr.Length>0 )//& previousClose<previousOpen)
                    {
                    ShortEntry = bullLows.LastValue;
                    //Print("Short Entry: "+ShortEntry);
                    BullSignalNotSent=true;
                    }
                    if(previousClose>LongEntry & BearSignalNotSent )//& priceAboveSAR)
                    {
                    BuySignal[index-1]=Low;
                    //Chart.DrawIcon("Buy Signal"+index, ChartIconType.UpTriangle, index-1, Low, Color.DodgerBlue);
                    BearSignalNotSent=false;
                    }
                }
            }
            }
    
    
    
    }
}

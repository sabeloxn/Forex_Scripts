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
    
    public class CleanChartv1102 : Indicator
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
        [Output("PSAR Main",LineColor = "#877841A6",PlotType =PlotType.Points, Thickness = 3)]
        public IndicatorDataSeries PSARResult { get; set; }
        public int lastCalculatedIdx = -1;
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
    
            High = dailySeries.HighPrices.LastValue;
            Low = dailySeries.LowPrices.LastValue;
            Open = dailySeries.OpenPrices.LastValue;
            Close = dailySeries.ClosePrices.LastValue;
            
            Bearhigh = dailySeries.HighPrices.LastValue;
            Bearlow = dailySeries.LowPrices.LastValue;
            Bullhigh = dailySeries.HighPrices.LastValue;
            Bulllow = dailySeries.LowPrices.LastValue;
            
            

            bool bullishBar = Close > Open;
            bool bearishBar = Close < Open;
            var previousLow = Bars.LowPrices.Last(1);
            var previousHigh = Bars.HighPrices.Last(1);
            
            bool priceAboveSAR = Close>PSARResult.LastValue;
            bool priceBelowSAR = Close<PSARResult.LastValue;

            for (int i = dailySeries.ClosePrices.Count - 1; i > 0; i--)
            {
            bool gotANewIdxSoANewBar = index > lastCalculatedIdx;

            if (gotANewIdxSoANewBar)
            {
                lastCalculatedIdx = index;
                if (dailySeries.OpenTimes[i].Date < todayCheck)
                    break;
                High = Math.Max(High, dailySeries.HighPrices[i]);
                Low = Math.Min(Low, dailySeries.LowPrices[i]);
                Open = dailySeries.OpenPrices[i];
                Close = dailySeries.ClosePrices[i];
                
                double previousOpen = dailySeries.OpenPrices[i+1];
                double previousClose = dailySeries.ClosePrices[i+1];
                
                if (bearishBar)
                {

                    BearIndexes.Add(index);
                    
                    Bearhigh = Math.Max(Bearhigh, dailySeries.HighPrices[i]);
                    Bearlow = Math.Min(Bearlow, dailySeries.LowPrices[i]);
                    bearLows[index] = Math.Min(Bearlow, dailySeries.LowPrices[i]);   
                    bearHighs[index] = Math.Max(Bearhigh, dailySeries.HighPrices[i]);
                    
                    BearIndexesArr = BearIndexes.ToArray();
                    
                    if (BearIndexesArr.Length>0 & previousClose>previousOpen)
                    {
                    LongEntry = bearHighs.LastValue;
                    
                    }
                    if(Close<ShortEntry & BullSignalNotSent & priceBelowSAR)
                    {
                    Chart.DrawIcon("Sell Signal"+index, ChartIconType.DownTriangle, index, High+0.00, Color.Crimson);
                    BullSignalNotSent=false;
                    BearSignalNotSent=true;
                    }

                }
                    
                    
                
                if (bullishBar)
                {
                    BullIndexes.Add(index);
                    
                    Bullhigh = Math.Max(Bullhigh, dailySeries.HighPrices[i]);
                    Bulllow = Math.Max(Bulllow, dailySeries.LowPrices[i]);
                    bullHighs[index] = Math.Max(Bullhigh, dailySeries.HighPrices[i]);
                    bullLows[index] = Math.Max(Bulllow, dailySeries.LowPrices[i]);
                    
                    BullIndexesArr = BullIndexes.ToArray();
                    
                    if (BullIndexesArr.Length>0 & previousClose<previousOpen)
                    {
                    ShortEntry = bullLows.LastValue;
                    
                    }
                    if(Close>LongEntry & BearSignalNotSent & priceAboveSAR)
                    {
                    Chart.DrawIcon("Buy Signal"+index, ChartIconType.UpTriangle, index, Low, Color.DodgerBlue);
                    BearSignalNotSent=false;
                    BullSignalNotSent=true;
                    }
                }
            }
            }
            
        }
    }
}

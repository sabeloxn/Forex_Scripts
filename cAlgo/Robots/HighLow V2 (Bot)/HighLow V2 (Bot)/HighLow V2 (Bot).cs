using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using System.Net;
namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    
    public class HighLowV2Bot : Robot
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
        public string BarType ="";
        
        Color BullHighcolor = "Purple";
        Color BullLowcolor = "Purple";
        Color BearHighcolor = "White";
        Color BearLowcolor = "White";

        public IndicatorDataSeries bullHighs;
        public IndicatorDataSeries bearHighs;
        public IndicatorDataSeries bearLows;
        public IndicatorDataSeries bullLows;
        public Bars dailySeries;

        public double ShortEntry;
        public double LongEntry;
        public double LongStopLoss;
        public double ShortStopLoss;
        
        public List<int> BearIndexes=new List <int>{};
        //public int[] BearIndexesArr = new int[]{};
        
        public List<int> BullIndexes=new List <int>{};
        bool BearSignalNotSent=true;
        bool BullSignalNotSent=true;
        
        protected override void OnStart()
        {
            dailySeries = MarketData.GetBars(TimeFrame.Daily);
            bullLows = CreateDataSeries();
            bullHighs = CreateDataSeries();
            bearLows = CreateDataSeries();
            bearHighs = CreateDataSeries();
        }
        
        protected override void OnBar()
        {
        double previousOpen = dailySeries.OpenPrices.Last(1);
        double previousClose = dailySeries.ClosePrices.Last(1);
        int index =dailySeries.ClosePrices.Count - 1;
        for (int i = dailySeries.ClosePrices.Count - 1; i > 0; i--)
            {
            
            
            // Handle price updates here
            DateTime today = dailySeries.OpenTimes[index].Date;
            DateTime todayCheck = dailySeries.OpenTimes[index].Date;
            DateTime tomorrow = today.AddDays(1);

            High = dailySeries.HighPrices.Last(1);
            Low = dailySeries.LowPrices.Last(1);
            Open = dailySeries.OpenPrices.Last(1);
            Close = dailySeries.ClosePrices.Last(1);

            Bearhigh = dailySeries.HighPrices.Last(1);
            Bearlow = dailySeries.LowPrices.Last(1);
            Bullhigh = dailySeries.HighPrices.Last(1);
            Bulllow = dailySeries.LowPrices.Last(1);
            
            

            bool bullishBar = Close > Open;
            bool bearishBar = Close < Open;
            var previousLow = Bars.LowPrices.Last(1);
            var previousHigh = Bars.HighPrices.Last(1);
            
            //bool CrossOver = Close>LongEntry | Close<ShortEntry;
            
            //int[] BullIndexes = new int[]{};
            
                
                //if (dailySeries.OpenTimes[i].Date < todayCheck)
                    //break;
                High = Math.Max(High, dailySeries.HighPrices[i]);
                Low = Math.Min(Low, dailySeries.LowPrices[i]);
                Open = dailySeries.OpenPrices[i];
                Close = dailySeries.ClosePrices[i];
                
                
                
                if (bearishBar)
                {

                    BearIndexes.Add(index);
                    
                    Bearhigh = Math.Max(Bearhigh, dailySeries.HighPrices[i]);
                    Bearlow = Math.Min(Bearlow, dailySeries.LowPrices[i]);
                    bearLows[index] = Math.Min(Bearlow, dailySeries.LowPrices[i]);   //stoploss
                    bearHighs[index] = Math.Max(Bearhigh, dailySeries.HighPrices[i]);//entry
                    
                    // Print("Long Entry : {0} ", LongEntry);
                    int[]BearIndexesArr = BearIndexes.ToArray();
                    //Print(previousClose+" ~ "+ previousOpen);
                    if (BearIndexesArr.Length>0 )
                    {
                    //Print(BearIndexesArr.Length);
                    //int differenceInDays=(BearIndexesArr[BearIndexesArr.Length-1])-(BearIndexesArr[BearIndexesArr.Length-2]);
                    //Chart.DrawTrendLine("Bear high " + today, today, Bearhigh, tomorrow.AddDays(differenceInDays), Bearhigh, BearHighcolor);
                    LongEntry = bearHighs.LastValue;//set entry price for buy trades
                    Chart.DrawIcon("Sell Signal"+today, ChartIconType.DownTriangle, today, LongEntry, Color.Crimson);
                    LongStopLoss = bearLows.LastValue;//set stoploss for buy trades
                    Chart.DrawIcon("Sellstop Signal"+today, ChartIconType.DownTriangle, today, LongStopLoss, Color.Aqua);
                    //Print("Long Entry: "+LongEntry);
                    //Print((LongEntries.LastValue));
                    BearSignalNotSent=true;
                    }
                    Print("Close: "+Close+" ShortEntry: "+ShortEntry+" SL: "+LongStopLoss);
                    //BUY on red close below bullbar short entry
                    if(Close<ShortEntry & BullSignalNotSent)
                    {
                    //ShortEntries[index]=index;
                    //Chart.DrawIcon("Sell Signal"+today, ChartIconType.DownTriangle, today.AddDays(1), High+0.00, Color.Crimson);
                    PlaceLimitOrder(TradeType.Sell,SymbolName,10000,ShortEntry,"SellStop",ShortStopLoss,200);
                    BullSignalNotSent=false;
                    }
                    //Chart.DrawIcon("Buy Signal" + today, ChartIconType.UpTriangle, today.AddDays(1), previousLow, Color.Crimson);
                    //Print(BearIndexesArr[BearIndexesArr.Length-1]);
                }
                    
                    
                
                if (bullishBar)
                {
                    BullIndexes.Add(index);
                    
                    Bullhigh = Math.Max(Bullhigh, dailySeries.HighPrices[i]);
                    Bulllow = Math.Max(Bulllow, dailySeries.LowPrices[i]);
                    bullHighs[index] = Math.Max(Bullhigh, dailySeries.HighPrices[i]);
                    bullLows[index] = Math.Max(Bulllow, dailySeries.LowPrices[i]);
                    
                    int[] BullIndexesArr = BullIndexes.ToArray();


                    if (BullIndexesArr.Length>0)
                    {
                    //int differenceInDays=(BullIndexesArr[BullIndexesArr.Length-1])-(BearIndexesArr[BearIndexesArr.Length-1]);
                    //Chart.DrawTrendLine("Bull low " + today, today, Bulllow, tomorrow.AddDays(differenceInDays), Bulllow, BullLowcolor);
                    ShortEntry = bullLows.LastValue;//entry trigger
                    ShortStopLoss = bullHighs.LastValue;
                    //Print("Short Entry: "+ShortEntry);
                    BullSignalNotSent=true;
                    }
                    Print("Close: "+Close+" Long Entry: "+LongEntry+" SL: "+ShortStopLoss);
                    if(Close>LongEntry & BearSignalNotSent)
                    {
                    //LongEntries[index]=index;
                    //Chart.DrawIcon("Buy Signal"+today, ChartIconType.UpTriangle, today.AddDays(1), Low, Color.DodgerBlue);
                    PlaceLimitOrder(TradeType.Buy,SymbolName,10000,LongEntry,"BuyStop",LongStopLoss,200);
                    BearSignalNotSent=false;
                    }
                }
            }
        }

        protected override void OnStop()
        {
            // Handle cBot stop here
        }
    }
}
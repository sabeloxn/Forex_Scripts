using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class XAUQuickScopev101 : Robot
    {
        private ParabolicSAR _PSAR;
        public double PSARResult;
        public Bars _Series;
        public double High { get; set; }
        public double Low { get; set; }
        public double Open { get; set; }
        double Close;
        bool bullishBar;
        bool bearishBar;
        double previousOpen ;
        double previousClose ;
        
        public double bullHighs;
        public double bearHighs;
        public double bearLows;
        public double bullLows;
        
        public double Bearhigh { get; set; }
        public double Bearlow { get; set; }
        public double Bullhigh { get; set; }
        public double Bulllow { get; set; }
        
        public List<int> BearIndexes=new List <int>{};
        public int[] BearIndexesArr = new int[]{};
        
        public List<int> BullIndexes=new List <int>{};
        public int[] BullIndexesArr = new int[]{};
        
        [Parameter("Initial Quantity (Lots)", Group = "Volume", DefaultValue = 1, MinValue = 0.01, Step = 0.01)]
        public double InitialQuantity { get; set; }

        [Parameter("Stop Loss", Group = "Protection", DefaultValue = 40)]
        public int StopLoss { get; set; }

        [Parameter("Take Profit", Group = "Protection", DefaultValue = 40)]
        public int TakeProfit { get; set; }
        
         public double ShortEntry{get;set;}
        public double LongEntry{get;set;}
        //Entry Prices and Stop Losses , May be deleted.
        bool BearSignalNotSent=true;
        bool BullSignalNotSent=true;
        
        public double ShortEntrySeries{get;set;}
        
        public double LongEntrySeries{get;set;}
        
        public double ShortStoplossSeries{get;set;} 
        
        public double LongStoplossSeries{get;set;}
        
        public double SellSignalSent{get;set;}

        public double BuySignalSent{get;set;}
        
        public string bartype;
        
        public IndicatorDataSeries OnStartOpen;
        public IndicatorDataSeries OnstartLow;
        public IndicatorDataSeries OnStartHigh;
        public IndicatorDataSeries OnstartClose;
        public IndicatorDataSeries OnStartCrosses;
        public double PreviousClose;
        
        public double PriceBeingCrossed;
        public bool BuyExecuted, SellExecuted;
        protected override void OnStart()
        {
            _Series = MarketData.GetBars(TimeFrame.Minute15);
            _PSAR = Indicators.ParabolicSAR(0.02, 0.2); 
            
            OnStartHigh=CreateDataSeries();
            OnstartLow=CreateDataSeries();
            OnStartOpen=CreateDataSeries();
            OnstartClose=CreateDataSeries();
             OnStartCrosses=CreateDataSeries();
            PerformOnStartPreChecks();
        }
        protected override void OnBar()
        {
           int index = Bars.ClosePrices.Count-1;
           PSARResult = _PSAR.Result.LastValue; 
           
           
           Close=_Series.ClosePrices.Last(1);
           High = _Series.HighPrices.LastValue;
           Low = _Series.LowPrices.LastValue;

           previousOpen = _Series.OpenPrices.Last(1);
           previousClose = _Series.ClosePrices.Last(1);
           Close=_Series.ClosePrices.Last(1);//Did not want to change to previousClose 36+ times
           Open = _Series.OpenPrices.Last(1);
            
           bullishBar = previousClose > previousOpen;
           bearishBar = previousClose < previousOpen;
           
           
           Bearhigh = _Series.HighPrices.LastValue;
            Bearlow = _Series.LowPrices.LastValue;
            Bullhigh = _Series.HighPrices.LastValue;
            Bulllow = _Series.LowPrices.LastValue;
           
           if (bearishBar)
                {
                    BearIndexes.Add(index-1);
                    
                    Bearhigh = Math.Max(Bearhigh, _Series.HighPrices.Last(1));
                    Bearlow = Math.Min(Bearlow, _Series.LowPrices.Last(1));
                    bearLows= Math.Min(Bearlow, _Series.LowPrices.Last(1));   
                    bearHighs = Math.Max(Bearhigh, _Series.HighPrices.Last(1));

                    BearIndexesArr = BearIndexes.ToArray();
                    
                    OnBar(index);
                }
           if (bullishBar)
                {
                    BullIndexes.Add(index-1);
                    
                    Bullhigh = Math.Max(Bullhigh, _Series.HighPrices.Last(1));
                    Bulllow = Math.Max(Bulllow, _Series.LowPrices.Last(1));
                    bullHighs = Math.Max(Bullhigh, _Series.HighPrices.Last(1));
                    bullLows = Math.Max(Bulllow, _Series.LowPrices.Last(1));

                    BullIndexesArr = BullIndexes.ToArray();
                    
                    OnBar(index);
                }
       //
       
       WriteOut(LongEntry,ShortEntry,index, previousClose);            
        }
        void OnBar(int index)
        {
        bool priceAboveSAR = Close>PSARResult;
        bool priceBelowSAR = Close<PSARResult;
            if (bearishBar)
            {
                LongEntry = bearHighs;
                LongEntrySeries= LongEntry;
                
                BearSignalNotSent=true;
                BuySignalSent=0;
                    
                //bartype="bearish";

                //BuyExecuted=false;

                if(Close<ShortEntry & BullSignalNotSent & priceBelowSAR)  //& !SellExecuted
                {
                SellSignalSent=1;
                //SellSignal=High+SignalSpace;
                BullSignalNotSent=false;
                BuySignalSent=0;
                LongStoplossSeries=bearLows;
                ExecuteOrder(InitialQuantity);
                }
              }
              
             if (bullishBar)
                {
                ShortEntry = bullLows;
                ShortEntrySeries = ShortEntry;
                
                //ShortStoplossSeries = bullHighs;
                BullSignalNotSent=true;
                SellSignalSent=0;
                
                //bartype="bullish";
               
                
                //SellExecuted=false;

                if(Close>LongEntry & BearSignalNotSent & priceAboveSAR ) //& !BuyExecuted
                {
                BuySignalSent=1;
                //BuySignal=Low-SignalSpace;
                BearSignalNotSent=false;
                SellSignalSent=0;
                ShortStoplossSeries= bullHighs;
                ExecuteOrder(InitialQuantity);
                }
            }
        }
        protected override void OnStop()
        {
           
        }
        
        /*private void ExecuteOrder(double quantity)
        {
            var volumeInUnits = Symbol.QuantityToVolumeInUnits(quantity);
            
            if(bartype=="bullish" )
            {     
            ExecuteMarketOrder(TradeType.Sell, SymbolName, volumeInUnits, "360SellMarket", StopLoss, TakeProfit);
            PriceBeingCrossed=LongEntry;
            BuyExecuted=true;
            }
            else if(bartype=="bearish")
            {
            ExecuteMarketOrder(TradeType.Buy, SymbolName, volumeInUnits, "360BuyMarket", StopLoss, TakeProfit);
            PriceBeingCrossed=ShortEntry;
            SellExecuted=true;
            }   
        }*/
        private void ExecuteOrder(double quantity)
        {
            var volumeInUnits = Symbol.QuantityToVolumeInUnits(quantity);
            
            
                if(BuySignalSent==1)
                {     
                 ExecuteMarketOrder(TradeType.Sell, SymbolName, volumeInUnits, "360SellMarket", StopLoss, TakeProfit);
                 PlaceLimitOrder(TradeType.Sell, SymbolName, volumeInUnits,Close+(StopLoss/100), "360SellHedge", StopLoss, 800);
                }
                else if(SellSignalSent==1)
                {
                ExecuteMarketOrder(TradeType.Buy, SymbolName, volumeInUnits, "360BuyMarket", StopLoss, TakeProfit);
                PlaceLimitOrder(TradeType.Buy, SymbolName, volumeInUnits,Close-(StopLoss/100), "360BuyHedge", StopLoss, 800);
                }   
            
        }
        void PerformOnStartPreChecks()
        {
       
        for(int i =Bars.ClosePrices.Count-1; i>0; i--)
            {
            OnStartCrosses[i]=Bars.ClosePrices[i];
            PreviousClose = OnStartCrosses.Last(1);
            double Close = Bars.ClosePrices[i];
            double PClose= Bars.ClosePrices.LastValue;

                High=Bars.HighPrices[i-1];
                Low=Bars.LowPrices[i-1];
                OnStartHigh[i]=High;
                OnstartLow[i]=Low;
                //Print(" High: {0} Low: {1} :", High, Low);
                //Print("OnStartCrosses {2}: OnStartHigh: {0} OnstartLow: {1} :", OnStartHigh.LastValue, OnstartLow.LastValue,OnStartCrosses.LastValue);
                PSARResult = _PSAR.Result.LastValue; 
               // Print(PSARResult+" *");
                
                if(Functions.HasCrossedAbove(OnStartCrosses, OnStartHigh.LastValue, 100) || Functions.HasCrossedAbove(OnStartCrosses, OnstartLow.LastValue, 100))
                {
                //Print("{2} :Would be triggered. HClose: {0} LClose: {1}",High,Low,Bars.OpenTimes[i]);
                //PositionPlaced=true;
                }
                else
                {
                //WednesdayMarked=true;
                //OnBar();
                }
                //break;
            }
             // WriteOut(LongEntry,ShortEntry);  
        }
        void WriteOut(double High, double Low, int i, double previousClose)
            {
            var stringBuilder = new StringBuilder();
            double pips = (High-Low);
            stringBuilder.AppendLine(Bars.OpenTimes[i]+"Long Entry: " + High);
            stringBuilder.AppendLine("Short Entry: " + Low);
            stringBuilder.AppendLine("Pips: " + pips);
            stringBuilder.AppendLine("P Close: " + previousClose);
            
            Chart.DrawStaticText("Status", stringBuilder.ToString(), VerticalAlignment.Top, HorizontalAlignment.Right, Color.AliceBlue);  
            }
        
    }
}
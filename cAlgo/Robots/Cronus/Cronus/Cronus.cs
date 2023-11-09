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
    public class Cronus : Robot
    {
        //--Moving Average
        [Parameter("Turn On", DefaultValue = false,Group ="Moving Average")]
        public bool UseMa { get; set; }
        private MovingAverage _MA;
        [Parameter("Type", DefaultValue = MovingAverageType.Weighted,Group ="Moving Average")]
        public MovingAverageType MaType { get; set; }
        [Parameter("Periods", DefaultValue = 14,Group ="Moving Average")]
        public int MaPeriods { get; set; }
        
        public double MaResult { get; set; }
       //--Linear Regression Forecast
        [Parameter("Turn On", DefaultValue = false,Group ="LRF")]
        public bool UseLRF { get; set; }
        private LinearRegressionForecast _LRF;
        [Parameter("Period", DefaultValue = 14,Group ="LRF")]
        public int LRFPeriod { get; set; }
        
        public double LRFResult { get; set; }
       //--Parabolic SAR
        [Parameter("Turn On", DefaultValue = false,Group ="Prabolic SAR")]
        public bool UseSAR { get; set; }
        private ParabolicSAR _PSAR;
        [Parameter("Min AF", DefaultValue = 0.02,Group ="Prabolic SAR")]
        public double Minaf { get; set; }
        [Parameter("Max AF", DefaultValue = 0.2,Group ="Prabolic SAR")]
        public double Maxaf { get; set; }
        [Parameter("Shift",DefaultValue =0,Group ="Prabolic SAR")]
        public int PSARshift {get;set;}
        
        public double PSARResult { get; set; }
       //--Stochastic Oscillator
        [Parameter("Turn On", DefaultValue = false,Group ="Stochastic Oscillator")]
        public bool UseStoch { get; set; }
        private StochasticOscillator Stoch;
        [Parameter("%k Periods",DefaultValue =24,Group ="Stochastic Oscillator")]
        public int kPeriods{get;set;}
        [Parameter("%k Slowing",DefaultValue =5,Group ="Stochastic Oscillator")]
        public int kSlowing{get;set;}
        [Parameter("%d Periods",DefaultValue =9,Group ="Stochastic Oscillator")]
        public int dPeriods{get;set;}
        [Parameter("MA Type", DefaultValue = MovingAverageType.Simple,Group ="Stochastic Oscillator")]
        public MovingAverageType StochMaType { get; set; }
       //--stoch output?
        public IndicatorDataSeries StochResultK;
        public IndicatorDataSeries StochResultD;
       //--Signals
        
        public double SellSignal{get;set;}
        
        public double BuySignal{get;set;}
        [Parameter("Signal Space", DefaultValue =0.01,Group ="Signals")]
        public double SignalSpace{get;set;}
        
        
        public double High { get; set; }
        public double Low { get; set; }
        public double Open { get; set; }
        double Close;

        public double Bearhigh { get; set; }
        public double Bearlow { get; set; }
        public double Bullhigh { get; set; }
        public double Bulllow { get; set; }

        public double PrevDayClose { get; set; }
        public double PrevDayOpen { get; set; }
        public double PrevDayHigh { get; set; }
        public double PrevDayLow { get; set; }
        public string BarType ="";

        public double bullHighs;
        public double bearHighs;
        public double bearLows;
        public double bullLows;
        public Bars _Series;
        public double ShortEntry{get;set;}
        public double LongEntry{get;set;}
        //Entry Prices and Stop Losses , May be deleted.
        
        public double ShortEntrySeries{get;set;}
        
        public double LongEntrySeries{get;set;}
        
        public double ShortStoplossSeries{get;set;} 
        
        public double LongStoplossSeries{get;set;}
        
        public List<int> BearIndexes=new List <int>{};
        public int[] BearIndexesArr = new int[]{};
        
        public List<int> BullIndexes=new List <int>{};
        public int[] BullIndexesArr = new int[]{};
        
        bool BearSignalNotSent=true;
        bool BullSignalNotSent=true;
        
        bool bullishBar ;
        bool bearishBar;
        double previousOpen ;
        double previousClose ;
        public int LastCalculatedIndex =-1;
        
        public bool BearCrossingBar;
        public bool BullCrossingBar ; 
        
        public bool BearMarketCross ;
        public bool BullMarketCross ;
        public bool StochAboveThreshold ; 
        public bool StochBelowThreshold ;
        public bool priceAboveSAR ;
        public bool priceBelowSAR ;
        //[Output("Sell Signal Sent",PlotType =PlotType.Points)]
        public double SellSignalSent{get;set;}
        //[Output("Buy Signal Sent",PlotType =PlotType.Points)]
        public double BuySignalSent{get;set;}
        
        [Parameter("Place Orders",DefaultValue =false, Group = "Order Type")]
        public bool PlaceOrders {get;set;}
        [Parameter("Execute Orders",DefaultValue =false, Group = "Order Type")]
        public bool ExecuteOrders {get;set;}
        
        public bool SellLimitNotPlaced = true;
        public bool BuyLimitNotPlaced=true;
        public double  BuySignalRecieved ;
        public double SellSignalRecieved;
        [Parameter("Initial Quantity (Lots)", Group = "Volume", DefaultValue = 1, MinValue = 0.01, Step = 0.01)]
        public double InitialQuantity { get; set; }

        [Parameter("Stop Loss", Group = "Protection", DefaultValue = 40)]
        public int StopLoss { get; set; }

        [Parameter("Take Profit", Group = "Protection", DefaultValue = 40)]
        public int TakeProfit { get; set; }
        
        double ClosePrice;
        double SellEntry;
        double SellStoploss;
        double BuyEntry;
        double BuyStoploss;
        
        [Parameter("Stack Orders", Group = "Order Type", DefaultValue = true)]
        public bool StackOrders { get; set; }
        protected override void OnStart()
        {
            _Series=MarketData.GetBars(TimeFrame);
            _PSAR = Indicators.ParabolicSAR(Minaf, Maxaf);
            _MA = Indicators.MovingAverage(_Series.ClosePrices,MaPeriods,MaType);
            _LRF = Indicators.LinearRegressionForecast(_Series.ClosePrices, LRFPeriod);
            Stoch = Indicators.StochasticOscillator(_Series, kPeriods, kSlowing, dPeriods, StochMaType); 
            
            //bullLows = CreateDataSeries();
            //bullHighs = CreateDataSeries();
            //bearLows = CreateDataSeries();
            //bearHighs = CreateDataSeries();
            
            //BuySignalSent=CreateDataSeries();
            //SellSignalSent=CreateDataSeries();
            High = _Series.HighPrices.LastValue;
            Low = _Series.LowPrices.LastValue;

            previousOpen = _Series.OpenPrices.Last(1);
            previousClose = _Series.ClosePrices.Last(1);
            Close=_Series.ClosePrices.Last(1);//Did not want to change to previousClose 36+ times
            Open = _Series.OpenPrices.Last(1);
            
            bullishBar = previousClose > previousOpen;
            bearishBar = previousClose < previousOpen;
            
            BuySignalRecieved   = BuySignalSent;
            SellSignalRecieved  = SellSignalSent;
            
            ClosePrice      =  Bars.ClosePrices.LastValue;
             
            SellEntry       = ShortEntrySeries;
            SellStoploss    = ShortStoplossSeries;
             
            BuyEntry        = LongEntrySeries;
            BuyStoploss     = LongStoplossSeries;
             
            if (bearishBar)
                {//Print("bearish");
                    BearIndexes.Add(1);
                    //Print("added");
                    Bearhigh = Math.Max(Bearhigh, _Series.HighPrices.LastValue);
                    Bearlow = Math.Min(Bearlow, _Series.LowPrices.LastValue);
                    bearLows= Math.Min(Bearlow, _Series.LowPrices.LastValue);   
                    bearHighs = Math.Max(Bearhigh, _Series.HighPrices.LastValue);
LongEntry=_Series.HighPrices.Last(1);
                    BearIndexesArr = BearIndexes.ToArray();  
                }
           if (bullishBar)
                {//Print("bullish");
                
                    BullIndexes.Add(1);
                    //Print("added");
                    Bullhigh = Math.Max(Bullhigh, _Series.HighPrices.LastValue);
                    Bulllow = Math.Max(Bulllow, _Series.LowPrices.LastValue);
                    bullHighs = Math.Max(Bullhigh, _Series.HighPrices.LastValue);
                    bullLows = Math.Max(Bulllow, _Series.LowPrices.LastValue);
                    
                    ShortEntry=_Series.LowPrices.Last(1);
                    
//Print("bullish "+ShortEntry);
                    BullIndexesArr = BullIndexes.ToArray();   
                }
            
        }

        protected override void OnBar()
        {
            // Handle price updates here
            int index = Bars.ClosePrices.Count-1;
            PSARResult = _PSAR.Result.LastValue;
            MaResult=_MA.Result.LastValue;
            LRFResult= _LRF.Result.LastValue;
                        
            High = _Series.HighPrices.LastValue;
            Low = _Series.LowPrices.LastValue;

            previousOpen = _Series.OpenPrices.Last(1);
            previousClose = _Series.ClosePrices.Last(1);
            Close=_Series.ClosePrices.Last(1);//Did not want to change to previousClose 36+ times
            Open = _Series.OpenPrices.Last(1);
            
            bullishBar = previousClose > previousOpen;
            bearishBar = previousClose < previousOpen;
            
            BuySignalRecieved   = BuySignalSent;
            SellSignalRecieved  = SellSignalSent;
            
            ClosePrice      =  Bars.ClosePrices.LastValue;
             
            SellEntry       = ShortEntrySeries;
            SellStoploss    = ShortStoplossSeries;
             
            BuyEntry        = LongEntrySeries;
            BuyStoploss     = LongStoplossSeries;
             
            if (bearishBar)
                {//Print("bearish");
                    BearIndexes.Add(index-1);
                    //Print("added");
                    Bearhigh = Math.Max(Bearhigh, _Series.HighPrices.Last(1));
                    Bearlow = Math.Min(Bearlow, _Series.LowPrices.Last(1));
                    bearLows= Math.Min(Bearlow, _Series.LowPrices.Last(1));   
                    bearHighs = Math.Max(Bearhigh, _Series.HighPrices.Last(1));

                    BearIndexesArr = BearIndexes.ToArray();
                    
                    OnBar(UseMa,UseLRF,UseStoch,UseSAR,index);
                }
           if (bullishBar)
                {//Print("bullish");
                    BullIndexes.Add(index-1);
                    //Print("added");
                    //ShortEntrySeries = ShortEntry;
                    Bullhigh = Math.Max(Bullhigh, _Series.HighPrices.Last(1));
                    Bulllow = Math.Max(Bulllow, _Series.LowPrices.Last(1));
                    bullHighs = Math.Max(Bullhigh, _Series.HighPrices.Last(1));
                    bullLows = Math.Max(Bulllow, _Series.LowPrices.Last(1));

                    BullIndexesArr = BullIndexes.ToArray();
                    
                    OnBar(UseMa,UseLRF,UseStoch,UseSAR,index);
                }
            
        }
        void OnBar(bool A,bool B,bool C,bool D,int index)
        {
            double StochResultK = Stoch.PercentK.LastValue;
            double StochResultD = Stoch.PercentD.LastValue;
            
            bool BearMarketCross = LRFResult>MaResult;
            bool BullMarketCross = LRFResult<MaResult;
            bool StochAboveThreshold = StochResultK > 50 & StochResultD > 50; 
            bool StochBelowThreshold = StochResultK < 50 & StochResultD < 50;
            bool priceAboveSAR = Close>PSARResult;
            bool priceBelowSAR = Close<PSARResult;
            
            bool BearCrossingBar = Open>MaResult & Close<MaResult;
            bool BullCrossingBar = Open<MaResult & Close>MaResult;
            
            Bearhigh = _Series.HighPrices.LastValue;
            Bearlow = _Series.LowPrices.LastValue;
            Bullhigh = _Series.HighPrices.LastValue;
            Bulllow = _Series.LowPrices.LastValue;
            
            
            
                if (bearishBar)
                    {//LongStoplossSeries=bearLows.lastvalue;
                    //Print("the bar behind me red af");
                        //if (BearIndexesArr.Length>0)
                        //{
                        LongEntry = bearHighs;
                        LongEntrySeries= LongEntry;
                        
                        BearSignalNotSent=true;
                        BuySignalSent=0;
                        //}
                        
                        //Indicator Options
                        if (A & B & C & D)
                        {
                            if(Close<ShortEntry & BullSignalNotSent & BearCrossingBar  & BearMarketCross & StochAboveThreshold & priceBelowSAR)
                            {
                            SellSignal=High+SignalSpace;
                            BullSignalNotSent=false;
                            SellSignalSent=1;
                            }
                        }
                        else if(A & B & C & !D)
                        {
                            if(Close<ShortEntry & BullSignalNotSent & BearCrossingBar  & BearMarketCross & StochAboveThreshold)
                            {
                            SellSignal=High+SignalSpace;
                            BullSignalNotSent=false;
                            SellSignalSent=1;
                            }
                        }
                        else if(A & B & !C & D)
                        {
                            if(Close<ShortEntry & BullSignalNotSent & BearCrossingBar  & BearMarketCross & priceBelowSAR)
                            {
                            SellSignal=High+SignalSpace;
                            BullSignalNotSent=false;
                            SellSignalSent=1;
                            }
                        }
                        else if(A & !B & C & D)
                        {
                            if(Close<ShortEntry & BullSignalNotSent & BearCrossingBar  & StochAboveThreshold & priceBelowSAR)
                            {
                            SellSignal=High+SignalSpace;
                            BullSignalNotSent=false;
                            SellSignalSent=1;
                            }
                        }
                        else if(!A & B & C & D)
                        {
                            if(Close<ShortEntry & BullSignalNotSent & BearMarketCross & StochAboveThreshold & priceBelowSAR)
                            {
                            SellSignal=High+SignalSpace;
                            BullSignalNotSent=false;
                            SellSignalSent=1;
                            }
                        }//2
                        else if(A & B & !C & !D)
                        {
                            if(Close<ShortEntry & BullSignalNotSent & BearCrossingBar & BearMarketCross)
                            {
                            SellSignal=High+SignalSpace;
                            BullSignalNotSent=false;
                            SellSignalSent=1;
                            }
                        }
                        else if(A & !B & !C & D)
                        {
                            if(Close<ShortEntry & BullSignalNotSent & BearCrossingBar & priceBelowSAR)
                            {
                            SellSignal=High+SignalSpace;
                            BullSignalNotSent=false;
                            SellSignalSent=1;
                            }
                        }
                        else if(!A & !B & C & D)
                        {
                            if(Close<ShortEntry & BullSignalNotSent & StochAboveThreshold & priceBelowSAR)
                            {
                            SellSignal=High+SignalSpace;
                            BullSignalNotSent=false;
                            SellSignalSent=1;
                            }
                        }
                        else if(!A & B & C & !D)
                        {
                            if(Close<ShortEntry & BullSignalNotSent & BearMarketCross & StochAboveThreshold)
                            {
                            SellSignal=High+SignalSpace;
                            BullSignalNotSent=false;
                            SellSignalSent=1;
                            }
                        }//2 skip 1
                        else if(A & !B & C & !D)
                        {
                            if(Close<ShortEntry & BullSignalNotSent & BearCrossingBar & StochAboveThreshold)
                            {
                            SellSignal=High+SignalSpace;
                            BullSignalNotSent=false;
                            SellSignalSent=1;
                            }
                        }
                        else if(!A & B & !C & D)
                        {
                            if(Close<ShortEntry & BullSignalNotSent  & BearMarketCross & priceBelowSAR)
                            {
                            SellSignal=High+SignalSpace;
                            BullSignalNotSent=false;
                            SellSignalSent=1;
                            }
                        }//2 skip 2
                        else if(!A & B & C & !D)
                        {
                            if(Close<ShortEntry & BullSignalNotSent  & BearCrossingBar & priceBelowSAR)
                            {
                            SellSignal=High+SignalSpace;
                            BullSignalNotSent=false;
                            SellSignalSent=1;
                            }
                        }//3
                        else if(A & !B & !C & !D)
                        {
                            if(Close<ShortEntry & BullSignalNotSent  & BearCrossingBar)
                            {
                            SellSignal=High+SignalSpace;
                            BullSignalNotSent=false;
                            SellSignalSent=1;
                            }
                        }
                        else if(!A & !B & !C & D)
                        {
                            if(Close<ShortEntry & BullSignalNotSent  & priceBelowSAR)
                            {
                            SellSignal=High+SignalSpace;
                            BullSignalNotSent=false;
                            SellSignalSent=1;
                            }
                        }
                        else if(!A & !B & C & !D)
                        {
                            if(Close<ShortEntry & BullSignalNotSent  & StochAboveThreshold)
                            {
                            SellSignal=High+SignalSpace;
                            BullSignalNotSent=false;
                            SellSignalSent=1;
                            }
                        }
                        else if(!A & B & !C & !D)
                        {
                            if(Close<ShortEntry & BullSignalNotSent  & BearMarketCross)
                            {
                            SellSignal=High+SignalSpace;
                            BullSignalNotSent=false;
                            SellSignalSent=1;
                            }
                        }
                        else 
                        {//all off
                        //Print("I would like sell now");
                        //Print("Close: "+Close+"Short Entry: "+ShortEntry+" Sell Entry: "+SellEntry);
                            if(Close<ShortEntry & BullSignalNotSent)
                            {
                            SellSignalSent=1;
                            SellSignal=High+SignalSpace;
                            BullSignalNotSent=false;
                            BuySignalSent=0;
                            LongStoplossSeries=bearLows;
                            PlaceOrder(InitialQuantity);
                            ExecuteOrder(InitialQuantity);
                            }
                        }
                    }
        
               if (bullishBar)
                    {
                        //if (BullIndexesArr.Length>0 )
                        //{
                        ShortEntry = bullLows;
                        ShortEntrySeries = ShortEntry;
                        ShortStoplossSeries = bullHighs;
                        BullSignalNotSent=true;
                        SellSignalSent=0;
                        //}
                        //Indicator Options
                        if (A & B & C & D)
                        {
                            if(Close>LongEntry & BearSignalNotSent & BullCrossingBar  & BullMarketCross & StochBelowThreshold & priceAboveSAR)
                            {
                            ;
                            BuySignal=Low-SignalSpace;
                            BearSignalNotSent=false;
                            BuySignalSent=1;
                            }
                            
                        }
                        else if(A & B & C & !D)
                        {
                            if(Close>LongEntry & BearSignalNotSent & BullCrossingBar  & BullMarketCross & StochBelowThreshold)
                            {
                            BuySignalSent=1;
                            BuySignal=Low-SignalSpace;
                            BearSignalNotSent=false;
                            
                            }
                        }
                        else if(A & B & !C & D)
                        {
                            if(Close>LongEntry & BearSignalNotSent & BullCrossingBar  & BullMarketCross & priceAboveSAR)
                            {
                            BuySignalSent=1;
                            BuySignal=Low-SignalSpace;
                            BearSignalNotSent=false;
                            
                            }
                        }
                        else if(A & !B & C & D)
                        {
                            if(Close>LongEntry & BearSignalNotSent & BullCrossingBar  & StochBelowThreshold & priceAboveSAR)
                            {
                            BuySignalSent=1;
                            BuySignal=Low-SignalSpace;
                            BearSignalNotSent=false;
                            
                            }
                        }
                        else if(!A & B & C & D)
                        {
                            if(Close>LongEntry & BearSignalNotSent & BullMarketCross & StochBelowThreshold & priceAboveSAR)
                            {
                            BuySignalSent=1;
                            BuySignal=Low-SignalSpace;
                            BearSignalNotSent=false;
                            
                            }
                        }//2
                        else if(A & B & !C & !D)
                        {
                            if(Close>LongEntry & BearSignalNotSent & BullCrossingBar & BullMarketCross)
                            {
                            BuySignalSent=1;
                            BuySignal=Low-SignalSpace;
                            BearSignalNotSent=false;
                            
                            }
                        }
                        else if(A & !B & !C & D)
                        {
                            if(Close>LongEntry & BearSignalNotSent & BullCrossingBar & priceAboveSAR)
                            {
                            BuySignalSent=1;
                            BuySignal=Low-SignalSpace;
                            BearSignalNotSent=false;
                            
                            }
                        }
                        else if(!A & !B & C & D)
                        {
                            if(Close>LongEntry & BearSignalNotSent & StochBelowThreshold & priceAboveSAR)
                            {
                            BuySignalSent=1;
                            BuySignal=Low-SignalSpace;
                            BearSignalNotSent=false;
                            
                            }
                        }
                        else if(!A & B & C & !D)
                        {
                            if(Close>LongEntry & BearSignalNotSent & BullMarketCross & StochBelowThreshold)
                            {
                            BuySignalSent=1;
                            BuySignal=Low-SignalSpace;
                            BearSignalNotSent=false;
                            
                            }
                        }//2 skip 1
                        else if(A & !B & C & !D)
                        {
                            if(Close>LongEntry & BearSignalNotSent & BullCrossingBar & StochBelowThreshold)
                            {
                            BuySignalSent=1;
                            BuySignal=Low-SignalSpace;
                            BearSignalNotSent=false;
                            
                            }
                        }
                        else if(!A & B & !C & D)
                        {
                            if(Close>LongEntry & BearSignalNotSent  & BullMarketCross & priceAboveSAR)
                            {
                            BuySignalSent=1;
                            BuySignal=Low-SignalSpace;
                            BearSignalNotSent=false;
                            
                            }
                        }//2 skip 2
                        else if(!A & B & C & !D)
                        {
                            if(Close>LongEntry & BearSignalNotSent  & BullCrossingBar & priceAboveSAR)
                            {
                            BuySignalSent=1;
                            BuySignal=Low-SignalSpace;
                            BearSignalNotSent=false;
                            
                            }
                        }//3
                        else if(A & !B & !C & !D)
                        {
                            if(Close>LongEntry & BearSignalNotSent  & BullCrossingBar)
                            {
                            BuySignalSent=1;
                            BuySignal=Low-SignalSpace;
                            BearSignalNotSent=false;
                            
                            }
                        }
                        else if(!A & !B & !C & D)
                        {
                            if(Close>LongEntry & BearSignalNotSent  & priceAboveSAR)
                            {
                            BuySignalSent=1;
                            BuySignal=Low-SignalSpace;
                            BearSignalNotSent=false;
                            
                            }
                        }
                        else if(!A & !B & C & !D)
                        {
                            if(Close>LongEntry & BearSignalNotSent  & StochBelowThreshold)
                            {
                            BuySignalSent=1;
                            BuySignal=Low-SignalSpace;
                            BearSignalNotSent=false;
                            
                            }
                        }
                        else if(!A & B & !C & !D)
                        {
                            if(Close>LongEntry & BearSignalNotSent  & BullMarketCross)
                            {
                            BuySignalSent=1;
                            BuySignal=Low-SignalSpace;
                            BearSignalNotSent=false;
                            
                            }
                        }
                        else 
                        {
                            if(Close>LongEntry & BearSignalNotSent )
                            {
                            BuySignalSent=1;
                            BuySignal=Low-SignalSpace;
                            BearSignalNotSent=false;
                            SellSignalSent=0;
                            ShortStoplossSeries= bullHighs;
                            PlaceOrder(InitialQuantity);
                            ExecuteOrder(InitialQuantity);
                            }
                        }
                    }
                
            }
        
        void PlaceOrder(double quantity)
        {
        var volumeInUnits = Symbol.QuantityToVolumeInUnits(quantity);
            //Print("Buy signal: "+BuySignal+" | Sell Signal "+SellSignal);
            //Print("Hello from place orders");
            if(PlaceOrders)
            {//Print(SellSignalRecieved +"|inside|"+Bars.HighPrices.Last(1));
                if(SellSignalSent==1)
                {//Print(ClosePrice+" |Short Entry| "+ShortEntry);
                    SellLimitNotPlaced = true;
                    if(StackOrders)
                    {
                        if(ClosePrice<ShortEntry)// && BuyLimitNotPlaced)
                        {
                        double Stoploss = (SellStoploss-SellEntry)*10000;//Highs and lows , would need to configure for different instruments
                            //if (SellPotentialStopLossPips>TakeProfit ){SellPotentialStopLossPips=TakeProfit;}
                      PlaceLimitOrder(TradeType.Sell,SymbolName,volumeInUnits,ShortEntry,"SellStop",StopLoss,TakeProfit);
                            BuyLimitNotPlaced = false;
                        }
                    }
                    else if(!StackOrders)
                    {
                       if(ClosePrice<ShortEntry && BuyLimitNotPlaced)
                        {
                        double Stoploss = (SellStoploss-SellEntry)*10000;//Highs and lows , would need to configure for different instruments
                            //if (SellPotentialStopLossPips>TakeProfit ){SellPotentialStopLossPips=TakeProfit;}
                      PlaceLimitOrder(TradeType.Sell,SymbolName,volumeInUnits,ShortEntry,"SellStop",StopLoss,TakeProfit);
                            BuyLimitNotPlaced = false;
                        } 
                    }
                }
                if(BuySignalSent==1)
                {//Print(ClosePrice +" |buy Entry| "+LongEntry);
                    BuyLimitNotPlaced=true;
                    if(StackOrders)
                    {
                        if(ClosePrice>LongEntry )//&& SellLimitNotPlaced)
                        {
                        double Stoploss = (BuyEntry-BuyStoploss)*10000;
                            //if (BuyPotentialStopLossPips>TakeProfit ){BuyPotentialStopLossPips=TakeProfit;}
                      PlaceLimitOrder(TradeType.Buy,SymbolName,volumeInUnits,LongEntry,"BuyStop",StopLoss,TakeProfit);
                            SellLimitNotPlaced = false;
                        }
                    }
                    else if(!StackOrders)
                    {
                       if(ClosePrice>LongEntry && SellLimitNotPlaced)
                        {
                        double Stoploss = (BuyEntry-BuyStoploss)*10000;
                            //if (BuyPotentialStopLossPips>TakeProfit ){BuyPotentialStopLossPips=TakeProfit;}
                      PlaceLimitOrder(TradeType.Buy,SymbolName,volumeInUnits,LongEntry,"BuyStop",StopLoss,TakeProfit);
                            SellLimitNotPlaced = false;
                        } 
                    }
                }
            }
        }
        private void ExecuteOrder(double quantity)
        {
            var volumeInUnits = Symbol.QuantityToVolumeInUnits(quantity);
            
            if(ExecuteOrders)
            {
                if(BuySignalSent==1)
                {     
                double Stoploss = (BuyEntry-BuyStoploss)*10000;//Highs and lows , would need to configure for different instruments
                ExecuteMarketOrder(TradeType.Buy, SymbolName, volumeInUnits, "Buy Market", StopLoss, TakeProfit);
                }
                else if(SellSignalSent==1)
                {
                double Stoploss = (SellEntry+SellStoploss)*10000;
                ExecuteMarketOrder(TradeType.Sell, SymbolName, volumeInUnits, "Sell Market", StopLoss, TakeProfit);
                }   
            }
        }
        protected override void OnStop()
        {
            // Handle cBot stop here
        }
    }
}
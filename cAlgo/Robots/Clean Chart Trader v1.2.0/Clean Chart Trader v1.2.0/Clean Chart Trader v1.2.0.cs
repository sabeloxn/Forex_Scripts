//FIXED STOP LOSS
//First attempt to load bars and place positions onstart
//on hold
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
    
    public class CleanChartTraderv120 : Robot
   {
        public double PrevDayClose { get; set; }
        public double PrevDayOpen { get; set; }
        public double PrevDayHigh { get; set; }
        public double PrevDayLow { get; set; }
        

        public bool RedBar;
        public bool GreenBar ;
        public string BarType="";
        public List<double> BearHighsList=new List <double>{};
        public double[] BearHighsArr =new double[]{};
        public List<double> BullLowsList=new List <double>{};
        public double[] BullLowsArr = new double[]{};
        public List<double> BearLowsList=new List <double>{};
        public double[] BearLowsArr =new double[]{};
        public List<double> BullHighsList=new List <double>{};
        public double[] BullHighsArr = new double[]{};
        double SellPotentialEntry;
        double SellPotentialStopLoss;
        double BuyPotentialEntry;
        double BuyPotentialStopLoss;

        public bool SellLimitNotPlaced = true;
        public bool BuyLimitNotPlaced=true;
        public double SellPotentialStopLossPips;
        public double BuyPotentialStopLossPips;
        
        private ParabolicSAR PSAR;
        [Parameter("Lot",DefaultValue =0.1)]
        public double LotSize {get;set;}
        public double LotsInUnits;
        [Parameter("Take Profit",DefaultValue =100)]
        public int TakeProfit{get;set;}
        public Bars dailySeries;
        [Parameter("Timeframe", DefaultValue = "Daily")]
        public TimeFrame iTimeFrame { get; set; }
        IndicatorDataSeries MyClosePrices;
        
        public IndicatorDataSeries bullHighs;
        public IndicatorDataSeries bearHighs;
        public IndicatorDataSeries bearLows;
        public IndicatorDataSeries bullLows;
        
        public double High { get; set; }
        public double Low { get; set; }
        public double Open { get; set; }
        public double Close { get; set; }

        public double Bearhigh { get; set; }
        public double Bearlow { get; set; }
        public double Bullhigh { get; set; }
        public double Bulllow { get; set; }
        
        public int lastCalculatedIdx=-1;
        
        public double ShortEntry;
        public double LongEntry;
        
        public List<int> BearIndexes=new List <int>{};
        public int[] BearIndexesArr = new int[]{};
        
        public List<int> BullIndexes=new List <int>{};
        public int[] BullIndexesArr = new int[]{};
        
        bool BearSignalNotSent=true;
        bool BullSignalNotSent=true;
        
        private ParabolicSAR _parabolic;
        public IndicatorDataSeries PSARResult { get; set; }
        protected override void OnStart()
        {
        
         
            GetOHLCdata();
            GetBarType();
            StoreOHLCdata();
            PSAR = Indicators.ParabolicSAR(0.02,0.2);
            //Print("PrevOHLC: "+PrevDayOpen+" "+PrevDayHigh+" "+PrevDayLow+" "+PrevDayClose+" "+BarType);
             
             dailySeries = MarketData.GetBars(iTimeFrame);
             //Bars_HistoryLoaded(dailySeries);
             Initialize();
             int index = Bars.ClosePrices.Count-1;
             
             Calculate(index);
             //index-=1;
             
             Print("Leaving OnStarrt" +index);
        }
        
        protected override void OnBar()
        {
        double ClosePrice = Bars.ClosePrices.LastValue;
        bool AbovePSAR = ClosePrice>PSAR.Result.LastValue;
        bool BelowPSAR = ClosePrice<PSAR.Result.LastValue;
        
        GetOHLCdata();
        GetBarType();
        StoreOHLCdata();
        SetTradeParameters();
        
        if(BarType=="Red Bar")
        {
            SellLimitNotPlaced = true;
            if(ClosePrice<SellPotentialEntry && BuyLimitNotPlaced && BelowPSAR ) //&SellPotentialStopLossPips<=TakeProfit 
            {
                if (SellPotentialStopLossPips>TakeProfit ){SellPotentialStopLossPips=TakeProfit;}
                PlaceLimitOrder(TradeType.Sell,SymbolName,LotsInUnits,SellPotentialEntry-0.3,"SellStop",SellPotentialStopLossPips,TakeProfit);
                BuyLimitNotPlaced = false;
            }
        }
        else
        {
            BuyLimitNotPlaced=true;
            if(ClosePrice>BuyPotentialEntry && SellLimitNotPlaced && AbovePSAR ) //&BuyPotentialStopLossPips<=TakeProfit
            {
                if (BuyPotentialStopLossPips>TakeProfit ){BuyPotentialStopLossPips=TakeProfit;}
                PlaceLimitOrder(TradeType.Buy,SymbolName,LotsInUnits,BuyPotentialEntry+0.3,"BuyStop",BuyPotentialStopLossPips,TakeProfit);
                SellLimitNotPlaced = false;
            }  
        }
        }

        protected override void OnStop()
        {
            // Handle cBot stop here
        }
        
        void GetOHLCdata(){
            PrevDayOpen=Bars.OpenPrices.Last(1);
            PrevDayHigh=Bars.HighPrices.Last(1);
            PrevDayLow = Bars.LowPrices.Last(1);
            PrevDayClose= Bars.ClosePrices.Last(1);
        }
        string GetBarType(){
            RedBar=PrevDayClose<PrevDayOpen;
            GreenBar = PrevDayClose>PrevDayOpen;
            
            if(RedBar){
            BarType="Red Bar";}
            else{
            BarType="Green Bar";};

            return BarType;
        }
        void StoreOHLCdata(){
            if(BarType=="Red Bar"){
            BearHighsList.Add(PrevDayHigh);
            BearHighsArr = BearHighsList.ToArray(); 
            
            BearLowsList.Add(PrevDayLow);
            BearLowsArr=BearLowsList.ToArray();
            
            BuyPotentialEntry= BearHighsArr[BearHighsArr.Length-1];
            BuyPotentialStopLoss= BearLowsArr[BearLowsArr.Length-1]; 
            //BuyPotentialStopLossPips = (BuyPotentialEntry-BuyPotentialStopLoss) * (Symbol.LotSize);

            }
            else{
            BullLowsList.Add(PrevDayLow);
            BullHighsList.Add(PrevDayHigh);
            BullLowsArr = BullLowsList.ToArray();
            BullHighsArr = BullHighsList.ToArray();
            
            SellPotentialEntry= BullLowsArr[BullLowsArr.Length-1];
            SellPotentialStopLoss = BullHighsArr[BullHighsArr.Length-1];
            //SellPotentialStopLossPips = (SellPotentialStopLoss-SellPotentialEntry) * (Symbol.LotSize);

            }
        
        
        }
        void SetTradeParameters(){
            int SymbolDigits=Symbol.Digits;
            if (SymbolDigits == 2){
                SellPotentialStopLossPips = (SellPotentialStopLoss-SellPotentialEntry) * (Symbol.LotSize);
                BuyPotentialStopLossPips = (BuyPotentialEntry-BuyPotentialStopLoss) * (Symbol.LotSize);
                TakeProfit = Convert.ToInt32(( Symbol.LotSize/TakeProfit)*100);
            }
            else{
                SellPotentialStopLossPips = (SellPotentialStopLoss-SellPotentialEntry) * (Symbol.LotSize/10);
                BuyPotentialStopLossPips = (BuyPotentialEntry-BuyPotentialStopLoss) * (Symbol.LotSize/10);
                TakeProfit = Convert.ToInt32((Symbol.LotSize/TakeProfit )/10);
            }
            LotsInUnits=Symbol.LotSize*LotSize;
            
        }
         void Initialize()
        {
            bullLows = CreateDataSeries();
            bullHighs = CreateDataSeries();
            bearLows = CreateDataSeries();
            bearHighs = CreateDataSeries();
            _parabolic = Indicators.ParabolicSAR(0.02, 0.2);
        }
        void Calculate(int index)
        {
            //PSARResult=PSAR.Result.LastValue; //PSARResult[index+PSARshift] 
            
            DateTime today = dailySeries.OpenTimes[index].Date;
            DateTime todayCheck = dailySeries.OpenTimes[index].Date;
            DateTime tomorrow = today.AddDays(1);
    
            High = dailySeries.HighPrices.LastValue;
            Low = dailySeries.LowPrices.LastValue;
            Open = dailySeries.OpenPrices.LastValue;
            Close = dailySeries.ClosePrices.LastValue;
            Print(High, Low,Open, Close);
            Bearhigh = dailySeries.HighPrices.LastValue;
            Bearlow = dailySeries.LowPrices.LastValue;
            Bullhigh = dailySeries.HighPrices.LastValue;
            Bulllow = dailySeries.LowPrices.LastValue;
            
            bool priceAboveSAR = Close>PSAR.Result.LastValue;
            bool priceBelowSAR = Close<PSAR.Result.LastValue;

            bool bullishBar = Close > Open;
            bool bearishBar = Close < Open;
            var previousLow = Bars.LowPrices.Last(1);
            var previousHigh = Bars.HighPrices.Last(1);

            for (int i = dailySeries.ClosePrices.Count - 1; i > 0; i--)
            {
            
            bool gotANewIdxSoANewBar = index > lastCalculatedIdx;

            if (gotANewIdxSoANewBar)
            {
                
                lastCalculatedIdx = index;
                //if (dailySeries.OpenTimes[i].Date < todayCheck)
                    //break;
                High = Math.Max(High, dailySeries.HighPrices[i]);
                Low = Math.Min(Low, dailySeries.LowPrices[i]);
                Open = dailySeries.OpenPrices[i];
                Close = dailySeries.ClosePrices[i];
                
                double previousOpen = dailySeries.OpenPrices[i+1];
                double previousClose = dailySeries.ClosePrices[i+1];
                
                if (bearishBar)
                {
Print("bear");
                    BearIndexes.Add(index);
                    
                    Bearhigh = Math.Max(Bearhigh, dailySeries.HighPrices[i]);
                    Bearlow = Math.Min(Bearlow, dailySeries.LowPrices[i]);
                    bearLows[index] = Math.Min(Bearlow, dailySeries.LowPrices[i]);   
                    bearHighs[index] = Math.Max(Bearhigh, dailySeries.HighPrices[i]);
                    
                    BearIndexesArr = BearIndexes.ToArray();
                    
                    if (BearIndexesArr.Length>0 & previousClose>previousOpen)
                    {
                    LongEntry = bearHighs.LastValue;
                    BearSignalNotSent=true;
                    }
                    if(Close<ShortEntry & BullSignalNotSent & priceBelowSAR)
                    {
                    Chart.DrawIcon("Sell Signal"+index, ChartIconType.DownTriangle, index, High+0.00, Color.Crimson);
                    BullSignalNotSent=false;
                    }

                }
                    
                    
                
                if (bullishBar)
                {
                Print("bull");
                    BullIndexes.Add(index);
                    
                    Bullhigh = Math.Max(Bullhigh, dailySeries.HighPrices[i]);
                    Bulllow = Math.Max(Bulllow, dailySeries.LowPrices[i]);
                    bullHighs[index] = Math.Max(Bullhigh, dailySeries.HighPrices[i]);
                    bullLows[index] = Math.Max(Bulllow, dailySeries.LowPrices[i]);
                    
                    BullIndexesArr = BullIndexes.ToArray();
                    
                    if (BullIndexesArr.Length>0 & previousClose<previousOpen)
                    {
                    ShortEntry = bullLows.LastValue;
                    BullSignalNotSent=true;
                    }
                    if(Close>LongEntry & BearSignalNotSent & priceAboveSAR)
                    {
                    Chart.DrawIcon("Buy Signal"+index, ChartIconType.UpTriangle, index, Low, Color.DodgerBlue);
                    BearSignalNotSent=false;
                    }
                }
            }
            }
            
        }
        
        private void Bars_HistoryLoaded(BarsHistoryLoadedEventArgs obj)
         {
             Print("Loaded Bars Count: {0}", obj.Count);
         }
        
//--
    }
}
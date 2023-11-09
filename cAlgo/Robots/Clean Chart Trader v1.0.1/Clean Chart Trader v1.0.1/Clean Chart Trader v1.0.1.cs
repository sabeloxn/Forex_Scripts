//FIXED STOP LOSS
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
    
    public class CleanChartTraderv101 : Robot
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
        [Parameter("Lot(s)",DefaultValue =1)]
        public double LotSize {get;set;}
        public double LotsInUnits;
        [Parameter("Take Profit",DefaultValue =100)]
        public int TakeProfit{get;set;}
        [Parameter("Max. Stop Loss",DefaultValue =100)]
        public int MaxStopLoss{get;set;}
        
        protected override void OnStart()
        {
            PSAR = Indicators.ParabolicSAR(0.02,0.2);
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
            if(ClosePrice<SellPotentialEntry && BuyLimitNotPlaced && BelowPSAR )
            {
                if (SellPotentialStopLossPips>TakeProfit ){SellPotentialStopLossPips=MaxStopLoss;}
                PlaceLimitOrder(TradeType.Sell,SymbolName,LotsInUnits,SellPotentialEntry-0.3,"SellStop",SellPotentialStopLossPips,TakeProfit);
                BuyLimitNotPlaced = false;
            }
        }
        else
        {
            BuyLimitNotPlaced=true;
            if(ClosePrice>BuyPotentialEntry && SellLimitNotPlaced && AbovePSAR )
            {
                if (BuyPotentialStopLossPips>TakeProfit ){BuyPotentialStopLossPips=MaxStopLoss;}
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
        
//--
    }
}
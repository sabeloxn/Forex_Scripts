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
        
        protected override void OnStart()
        {
            GetOHLCdata();
            GetBarType();
            StoreOHLCdata();//to bartype and get potential entries
            
        }
        
        protected override void OnBar()
        {
        double ClosePrice = Bars.ClosePrices.LastValue;
        GetOHLCdata();
        GetBarType();
        StoreOHLCdata();
        
        if(BarType=="Red Bar")
        {
            SellLimitNotPlaced = true;
            if(ClosePrice<SellPotentialEntry && BuyLimitNotPlaced)
            {
                //Print("Sell Conditions Met. Entry: "+SellPotentialEntry+" S/L: "+SellPotentialStopLoss );
                PlaceLimitOrder(TradeType.Sell,SymbolName,10000,SellPotentialEntry,"SellStop",SellPotentialStopLossPips,100);
                BuyLimitNotPlaced = false;
            }
        }
        else
        {
            BuyLimitNotPlaced=true;
            if(ClosePrice>BuyPotentialEntry && SellLimitNotPlaced)
            {
                //Print("Buy Conditions Met. Entry: "+BuyPotentialEntry+" S/L: "+BuyPotentialStopLoss );
                PlaceLimitOrder(TradeType.Buy,SymbolName,10000,BuyPotentialEntry,"BuyStop",BuyPotentialStopLossPips,100);
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
            //Print("PrevOHLC: "+PrevDayOpen+" "+PrevDayHigh+" "+PrevDayLow+" "+PrevDayClose+" "+BarType);
            return BarType;
        }
        void StoreOHLCdata(){
            if(BarType=="Red Bar"){
            BearHighsList.Add(PrevDayHigh);
            BearHighsArr = BearHighsList.ToArray(); 
            
            BuyPotentialEntry= BearHighsArr[BearHighsArr.Length-1];
            BuyPotentialStopLoss= BullLowsArr[BullLowsArr.Length-1];
            BuyPotentialStopLossPips = (BuyPotentialEntry-BuyPotentialStopLoss) * 10000;
            //Print("BUY Potential Entry: "+ BuyPotentialEntry);
            }
            else{
            BullLowsList.Add(PrevDayLow);
            BullHighsList.Add(PrevDayHigh);
            BullLowsArr = BullLowsList.ToArray();
            BullHighsArr = BullHighsList.ToArray();
            
            SellPotentialEntry= BullLowsArr[BullLowsArr.Length-1];
            SellPotentialStopLoss = BullHighsArr[BullHighsArr.Length-1];
            SellPotentialStopLossPips = (SellPotentialStopLoss-SellPotentialEntry) * 10000;
            //Print("SELL Potential Entry: "+ SellPotentialEntry);
            }

        }
    }
}
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
    
    public class CleanChartTraderv200 : Robot
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
        [Parameter("Stop Loss ",DefaultValue =100)]
        public int StopLoss{get;set;}
        protected override void OnStart()
        {
        //GetOHLCdata();
        //GetBarType();
        //StoreOHLCdata();
        PSAR = Indicators.ParabolicSAR(0.02,0.2);
        //Print("PrevOHLC: "+PrevDayOpen+" "+PrevDayHigh+" "+PrevDayLow+" "+PrevDayClose+" "+BarType);
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
            
            if(ClosePrice<SellPotentialEntry && BuyLimitNotPlaced && BelowPSAR )
            
            {//Print("#######"+SellPotentialStopLossPips+"SPEntry: "+SellPotentialEntry+"PC: "+PrevDayClose+"SPSL: "+SellPotentialStopLoss);
                if (SellPotentialStopLossPips>TakeProfit ){SellPotentialStopLossPips=StopLoss;}
                
                ExecuteMarketOrder(TradeType.Sell,SymbolName,LotsInUnits,"Sell",SellPotentialStopLossPips,TakeProfit);
                BuyLimitNotPlaced = false;
                SellLimitNotPlaced = true;
            }
        }
        else
        {
            
            if(ClosePrice>BuyPotentialEntry && SellLimitNotPlaced && AbovePSAR )
            {//Print("#######"+BuyPotentialStopLossPips+"SPEntry: "+BuyPotentialEntry+"PC: "+PrevDayClose+"SPSL: "+BuyPotentialStopLoss);
                if (BuyPotentialStopLossPips>TakeProfit ){BuyPotentialStopLossPips=StopLoss;}
                ExecuteMarketOrder(TradeType.Buy,SymbolName,LotsInUnits,"Buy",BuyPotentialStopLossPips,TakeProfit);
                SellLimitNotPlaced = false;
                BuyLimitNotPlaced=true;
            }  
        }
        }
        
        protected override void OnTick()
        {
            double myClose = Bars.ClosePrices.LastValue;
            var ShortPosition = Positions.Find("Sell", SymbolName,TradeType.Sell);
            var LongPosition = Positions.Find("Buy", SymbolName,TradeType.Buy);
            
            if (ShortPosition == null & LongPosition == null)
            { 
                //Stop();
            }
            else
            {
                double newShortStopLoss = SellPotentialEntry;
                //bool SellisProtected = ShortPosition.StopLoss.HasValue;
                
                double newLongStopLoss = BuyPotentialEntry;
                //bool BuyisProtected = LongPosition.StopLoss.HasValue;
                
                if (LongPosition != null )//&& BuyisProtected)
                {
                    if (newLongStopLoss > Bid)
                        return;
                    if (newLongStopLoss - LongPosition.StopLoss < Symbol.TickSize)
                        return;
                    if(myClose>BuyPotentialEntry+0.3){
                    ModifyPosition(LongPosition, LongPosition.EntryPrice, null);
                    }
                    
                }

                if (ShortPosition != null)// && SellisProtected)
                {
                    if (newShortStopLoss < Bid)
                        return;
                    if (ShortPosition.StopLoss - newShortStopLoss < Symbol.TickSize)
                        return;
                   if(myClose<SellPotentialEntry-0.3){
                   ModifyPosition(ShortPosition, ShortPosition.EntryPrice, null);
                   }
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
                //TakeProfit = Convert.ToInt32(( Symbol.LotSize/TakeProfit)*100);
            }
            else if (SymbolDigits == 3){
                SellPotentialStopLossPips = (SellPotentialStopLoss-PrevDayClose)  * (Symbol.LotSize/10);
                BuyPotentialStopLossPips = (PrevDayClose-BuyPotentialStopLoss) * (Symbol.LotSize/10);
                //TakeProfit = Convert.ToInt32(( Symbol.LotSize/TakeProfit)*100);
            }
            else{
                SellPotentialStopLossPips = (SellPotentialStopLoss-SellPotentialEntry) * (Symbol.LotSize/10);
                BuyPotentialStopLossPips = (BuyPotentialEntry-BuyPotentialStopLoss) * (Symbol.LotSize/10);
                
                //TakeProfit = Convert.ToInt32((Symbol.LotSize/TakeProfit )/10);
            }
            LotsInUnits=Symbol.LotSize*LotSize;
            
        }
        
//--
    }
}
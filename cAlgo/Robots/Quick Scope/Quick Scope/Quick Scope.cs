using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class QuickScope : Robot
    {
        private AimAssistv110 _aimAssist;
        public bool UseMa { get; set; } public MovingAverageType MaType { get; set; }public bool UseLRF { get; set; }public int LRFPeriod { get; set; }public bool UseSAR { get; set; }public double Minaf { get; set; }public double Maxaf { get; set; }public int PSARshift {get;set;}public int MaPeriods { get; set; }public bool UseStoch { get; set; }public int kPeriods{get;set;}public int kSlowing{get;set;}public int dPeriods{get;set;}public MovingAverageType StochMaType { get; set; }public double SignalSpace{get;set;} public bool ShowPeriod {get; set;}
        [Parameter("Place Orders",DefaultValue =false, Group = "Order Type")]
        public bool PlaceOrders {get;set;}
        [Parameter("Execute Orders",DefaultValue =false, Group = "Order Type")]
        public bool ExecuteOrders {get;set;}
        
        public bool RedBar;
        public bool GreenBar ;
        public string BarType="";
        public double PrevDayClose { get; set; }
        public double PrevDayOpen { get; set; }
        public double PrevDayHigh { get; set; }
        public double PrevDayLow { get; set; }
        
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
        
        protected override void OnStart()
        {
            _aimAssist = Indicators.GetIndicator<AimAssistv110>(UseMa,MaType,MaPeriods,UseLRF,LRFPeriod,UseSAR,Minaf,Maxaf,PSARshift,UseStoch,kPeriods,kSlowing,dPeriods,StochMaType,SignalSpace,ShowPeriod);     
        }
        
        protected override void OnBar()
        {
            // Handle price updates here
             BuySignalRecieved   = _aimAssist.BuySignal.LastValue;
             SellSignalRecieved  = _aimAssist.SellSignal.LastValue;
        
            //Print("Buy Signal: "+BuySignalRecieved+" |****| "+" Sell Signal: "+SellSignalRecieved);}
             ClosePrice      =  Bars.ClosePrices.LastValue;
             
             SellEntry       = _aimAssist.ShortEntrySeries.LastValue;
             SellStoploss    = _aimAssist.ShortStoplossSeries.LastValue;
             
             BuyEntry        = _aimAssist.LongEntrySeries.LastValue;
             BuyStoploss     = _aimAssist.LongStoplossSeries.LastValue;
             
             ExecuteOrder(InitialQuantity);
             PlaceOrder(InitialQuantity);
        }
        private void ExecuteOrder(double quantity)
        {
            var volumeInUnits = Symbol.QuantityToVolumeInUnits(quantity);
            
            if(ExecuteOrders)
            {
                if(BuySignalRecieved==Bars.LowPrices.Last(1))
                {     
                double Stoploss = (BuyEntry-BuyStoploss)*10000;//Highs and lows , would need to configure for different instruments
                ExecuteMarketOrder(TradeType.Buy, SymbolName, volumeInUnits, "Buy Market", StopLoss, TakeProfit);
                }
                else if(SellSignalRecieved==Bars.HighPrices.Last(1))
                {
                double Stoploss = (SellEntry+SellStoploss)*10000;
                ExecuteMarketOrder(TradeType.Sell, SymbolName, volumeInUnits, "Sell Market", StopLoss, TakeProfit);
                }   
            }
        }
        
        void PlaceOrder(double quantity)
        {
        var volumeInUnits = Symbol.QuantityToVolumeInUnits(quantity);
            
            if(PlaceOrders)
            {
                if(SellSignalRecieved==Bars.HighPrices.Last(1))
                {//Print(SellSignalRecieved +"||"+Bars.HighPrices.Last(1));
                    SellLimitNotPlaced = true;
                    if(ClosePrice<SellEntry && BuyLimitNotPlaced)
                    {
                    double Stoploss = (SellStoploss-SellEntry)*10000;//Highs and lows , would need to configure for different instruments
                        //if (SellPotentialStopLossPips>TakeProfit ){SellPotentialStopLossPips=TakeProfit;}
                  PlaceLimitOrder(TradeType.Sell,SymbolName,volumeInUnits,SellEntry,"SellStop",StopLoss,TakeProfit);
                        BuyLimitNotPlaced = false;
                    }
                }
                else if(BuySignalRecieved==Bars.LowPrices.Last(1))
                {
                    BuyLimitNotPlaced=true;
                    if(ClosePrice>BuyEntry && SellLimitNotPlaced)
                    {
                    double Stoploss = (BuyEntry-BuyStoploss)*10000;
                        //if (BuyPotentialStopLossPips>TakeProfit ){BuyPotentialStopLossPips=TakeProfit;}
                  PlaceLimitOrder(TradeType.Buy,SymbolName,volumeInUnits,BuyEntry,"BuyStop",StopLoss,TakeProfit);
                        SellLimitNotPlaced = false;
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
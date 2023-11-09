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
    public class EURUSDDayTrader : Robot
    {
        [Parameter("Lot(s)",DefaultValue = 1)]
        public double Quantity { get; set; }
        
        [Parameter("Take Profit",DefaultValue = 100)]
        public double TakeProfit { get; set; }
        [Parameter("Normalizer",DefaultValue = 1000)]
        public double Normalizer { get; set; }

        bool DaybullishBar ;
        bool DaybearishBar;

        double previousDayOpen ;
        double previousDayClose ;
        public int LastCalculatedIndex =-1;
//Weekdays
        public Bars Series;
        public bool MondayMarked;
        public bool WednesdayMarked;
        public bool Monday;
        public bool Wednesday;
        DateTime todayCheck ;
        DateTime WeekDay;
        public double PreviousClose;
        public double High;
        public double Low ;
        public bool PositionPlaced;
        
        public double[] ShortEntry = new double []{};
        public double[] LongEntry  = new double []{};
        public double SellLevel;
        public double BuyLevel;
        
        public double HighestClose;
        public double LowestClose;
        
        public IndicatorDataSeries OnStartCrosses ;
        public IndicatorDataSeries OnStartHigh;
        public IndicatorDataSeries OnstartLow;
        
        protected override void OnStart()
        {
        Series=MarketData.GetBars(TimeFrame.Daily); 
        OnStartCrosses=CreateDataSeries();
        OnStartHigh=CreateDataSeries();
        OnstartLow=CreateDataSeries();
        GetLastTradeDay();
        Print(Symbol.Digits);
        }

        protected override void OnTick()
        {
        int index=Bars.ClosePrices.Count-1;
        previousDayOpen     = Series.OpenPrices.Last(1);
        
        previousDayClose    = Series.ClosePrices.Last(1);
        
        
        DaybullishBar = previousDayClose > previousDayOpen;
        DaybearishBar = previousDayClose < previousDayOpen;
        // Handle price updates here
        todayCheck = Bars.OpenTimes[index].Date;
        WeekDay= Convert.ToDateTime(todayCheck);
        string WeekDayName =Convert.ToString(WeekDay.DayOfWeek);
        Monday=WeekDayName=="Monday";
        Wednesday=WeekDayName=="Wednesday";
        
        //PreviousClose = Bars.ClosePrices.Last(1);
            if(Monday && !MondayMarked)
                {
                //PositionPlaced=false;
                //WednesdayMarked=false;
                    //if(DaybearishBar)
                    //{
                    //SetHighLow();
                MondayMarked=true;  
                    //}
                    ///else if(DaybullishBar)
                    //{
                    //SetHighLow();
                    //MondayMarked=true;  
                    //}
                }
                
            if(Wednesday)
                {
                WednesdayMarked=true;
                //Print("In wednesday HIGH: {0} LOW: {1}", High,Low);
                OnBar();
                PositionPlaced=false;
                MondayMarked=false;
                    //if(DaybearishBar)
                    //{
                    //SetHighLow();
                    //WednesdayMarked=true;
                    //}
                    //else if(DaybullishBar)
                    //{
                    
                    SetHighLow();
                   
                    //}
                }
        }
        protected override void OnBar()
        {
            double StopLoss = (High-Low)*Normalizer;
            var volumeInUnits = Symbol.QuantityToVolumeInUnits(Quantity);
            if(PreviousClose<Low && WednesdayMarked &&!PositionPlaced)
            {Print("Selling|Previous Close: {0} Low: {1}",PreviousClose,Low);
            PlaceLimitOrder(TradeType.Sell,SymbolName,volumeInUnits,Low,"SellLimit",StopLoss,TakeProfit);
            PositionPlaced=true;
            }
            if(PreviousClose>High && WednesdayMarked&&!PositionPlaced)
            {
            PlaceLimitOrder(TradeType.Buy,SymbolName,volumeInUnits,High,"BuyLimit",StopLoss,TakeProfit);
            PositionPlaced=true;
            }
        }
        void SetHighLow()
        {
         High=Series.HighPrices.Last(1);
         Low =Series.LowPrices.Last(1);
         ShortEntry.Append(Low);
         LongEntry.Append(High);
         BuyLevel = LongEntry.Length-1;
         SellLevel = ShortEntry.Length-1;
        }
        void GetLastTradeDay()
        {
        DateTime StartDay =DateTime.Today;//when not backtesting
        for(int i =Bars.ClosePrices.Count-1; i>0; i--)
            {
            OnStartCrosses[i]=Bars.ClosePrices[i];
            PreviousClose = OnStartCrosses.Last(1);
            double Close = Bars.ClosePrices[i];
            double PClose= Bars.ClosePrices.LastValue;
            DateTime today = Bars.OpenTimes[i].Date;
            DateTime thisWeekDay= Convert.ToDateTime(today);
            string WeekDayName =Convert.ToString(thisWeekDay.DayOfWeek);
            
            bool WednesdayCheck=WeekDayName=="Wednesday";
            
            
                if(WednesdayCheck)
                {
                TimeSpan Period = StartDay-today;
                //Print(Period.Days);
                High=Bars.HighPrices[i-1];
                Low=Bars.LowPrices[i-1];
                OnStartHigh[i]=High;
                OnstartLow[i]=Low;
                //Print("Wdnesday {2}: High: {0} Low: {1} :", High, Low,today);
                //Print("OnStartCrosses {2}: OnStartHigh: {0} OnstartLow: {1} :", OnStartHigh.LastValue, OnstartLow.LastValue,OnStartCrosses.LastValue);
                if(Functions.HasCrossedAbove(OnStartCrosses, OnStartHigh.LastValue, Period.Days) || Functions.HasCrossedAbove(OnStartCrosses, OnstartLow.LastValue, Period.Days))
                {
                //Print("Would be triggered. HClose: {0} LClose: {1}",High,Low);
                PositionPlaced=true;
                }
                else
                {
                WednesdayMarked=true;
                OnBar();
                }
                break;
                }
                
            }

        }
        
        protected override void OnStop()
        {
            // Handle cBot stop here
            Stop();
        }
    }
}
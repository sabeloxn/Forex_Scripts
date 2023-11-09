//onBar and send signals to bot
//Developed method to read signals from bot, no need to send
using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;
using System.Collections.Generic;
using System.Net;
using System.Text;
namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = true, AccessRights = AccessRights.None)]
    public class AimAssistv110 : Indicator
    {
        
       //--Moving Average
        [Parameter("Turn On", DefaultValue = false,Group ="Moving Average")]
        public bool UseMa { get; set; }
        private MovingAverage _MA;
        [Parameter("Type", DefaultValue = MovingAverageType.Weighted,Group ="Moving Average")]
        public MovingAverageType MaType { get; set; }
        [Parameter("Periods", DefaultValue = 14,Group ="Moving Average")]
        public int MaPeriods { get; set; }
        [Output("MA")]
        public IndicatorDataSeries MaResult { get; set; }
       //--Linear Regression Forecast
        [Parameter("Turn On", DefaultValue = false,Group ="LRF")]
        public bool UseLRF { get; set; }
        private LinearRegressionForecast _LRF;
        [Parameter("Period", DefaultValue = 14,Group ="LRF")]
        public int LRFPeriod { get; set; }
        [Output("LRF Main",LineColor = "#FFFED966", LineStyle = LineStyle.DotsVeryRare, Thickness = 5)]
        public IndicatorDataSeries LRFResult { get; set; }
       //--Parabolic SAR
        [Parameter("Turn On", DefaultValue = true,Group ="Prabolic SAR")]
        public bool UseSAR { get; set; }
        private ParabolicSAR _PSAR;
        [Parameter("Min AF", DefaultValue = 0.02,Group ="Prabolic SAR")]
        public double Minaf { get; set; }
        [Parameter("Max AF", DefaultValue = 0.2,Group ="Prabolic SAR")]
        public double Maxaf { get; set; }
        [Parameter("Shift",DefaultValue =0,Group ="Prabolic SAR")]
        public int PSARshift {get;set;}
        [Output("PSAR Main",LineColor = "#66FFE699",PlotType =PlotType.Points, Thickness = 5)]
        public IndicatorDataSeries PSARResult { get; set; }
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
        [Output("Short Signal",LineColor = "#FFFF999A",PlotType =PlotType.Points, Thickness = 5)]
        public IndicatorDataSeries SellSignal{get;set;}
        [Output("Long Signal",LineColor = "#FF9CFFF7",PlotType =PlotType.Points, Thickness = 5)]
        public IndicatorDataSeries BuySignal{get;set;}
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

        public IndicatorDataSeries bullHighs;
        public IndicatorDataSeries bearHighs;
        public IndicatorDataSeries bearLows;
        public IndicatorDataSeries bullLows;
        public Bars _Series;
        
        public double ShortEntry{get;set;}
        public double LongEntry;
       //Entry Prices and Stop Losses , May be deleted.
        [Output("ShortEntrySeries",PlotType =PlotType.Points)]
        public IndicatorDataSeries ShortEntrySeries{get;set;}
        [Output("LongEntrySeries",PlotType =PlotType.Points)]
        public IndicatorDataSeries LongEntrySeries{get;set;}
        [Output("ShortStoplossSeries",PlotType =PlotType.Points)]
        public IndicatorDataSeries ShortStoplossSeries{get;set;} 
        [Output("LongStoplossSeries",PlotType =PlotType.Points)]
        public IndicatorDataSeries LongStoplossSeries{get;set;}
        
        public List<int> BearIndexes=new List <int>{};
        public int[] BearIndexesArr = new int[]{};
        
        public List<int> BullIndexes=new List <int>{};
        public int[] BullIndexesArr = new int[]{};
        
        bool BearSignalNotSent=true;
        bool BullSignalNotSent=true;
        
        bool bullishBar ;
        bool bearishBar;
        bool DaybullishBar ;
        bool DaybearishBar;
        double previousOpen ;
        double previousClose ;
        double previousDayOpen ;
        double previousDayClose ;
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
        public IndicatorDataSeries SellSignalSent{get;set;}
        //[Output("Buy Signal Sent",PlotType =PlotType.Points)]
        public IndicatorDataSeries BuySignalSent{get;set;}
        
        //[Output("Sell Signal Sent to bot")]
        public IndicatorDataSeries SellSignalSentToBot{get;set;}
        //output[Output("Buy Signal Sent to bot")]
        public IndicatorDataSeries BuySignalSentToBot{get;set;}
        //Weekdays
        public Bars Series;
        public bool MondayMarked;
        public bool WednesdayMarked;
        public bool Monday;
        public bool Wednesday;
        DateTime todayCheck ;
        DateTime WeekDay;
        [Parameter("Show Period",DefaultValue =true)]
        public bool ShowPeriod{get;set;}
        
        protected override void Initialize()
        {
            _Series = MarketData.GetBars(TimeFrame);
            //Daily Series for weekdays
            Series=MarketData.GetBars(TimeFrame.Daily);
            bullLows = CreateDataSeries();
            bullHighs = CreateDataSeries();
            bearLows = CreateDataSeries();
            bearHighs = CreateDataSeries();
            //ShortEntrySeries=CreateDataSeries();
            //LongEntrySeries=CreateDataSeries();
            //ShortStoplossSeries=CreateDataSeries();
            //LongStoplossSeries=CreateDataSeries();
            BuySignalSent=CreateDataSeries();
            SellSignalSent=CreateDataSeries();
            
            _PSAR = Indicators.ParabolicSAR(Minaf, Maxaf);
            _MA = Indicators.MovingAverage(_Series.ClosePrices,MaPeriods,MaType);
            _LRF = Indicators.LinearRegressionForecast(_Series.ClosePrices, LRFPeriod);
            Stoch = Indicators.StochasticOscillator(_Series, kPeriods, kSlowing, dPeriods, StochMaType);
        }
        public override void Calculate(int index)
        {
            PSARResult[index+PSARshift] = _PSAR.Result[index];
            MaResult[index]=_MA.Result.LastValue;//////////////why different
            LRFResult[index] = _LRF.Result[index];
                        
            High = _Series.HighPrices[index-1];
            Low = _Series.LowPrices[index-1];

            previousOpen        = _Series.OpenPrices[index-1];
            previousDayOpen     = Series.OpenPrices[index-1];
            previousClose       = _Series.ClosePrices[index-1];
            previousDayClose    = Series.ClosePrices[index-1];
            Close=_Series.ClosePrices[index-1];//Did not want to change to previousClose 36+ times
            Open = _Series.OpenPrices[index-1];
            bullishBar = previousClose > previousOpen;
            bearishBar = previousClose < previousOpen;
            DaybullishBar = previousDayClose > previousDayOpen;
            DaybearishBar = previousDayClose < previousDayOpen;
            
            if (bearishBar)
                {
                    BearIndexes.Add(index-1);
                    
                    Bearhigh = Math.Max(Bearhigh, _Series.HighPrices[index-1]);
                    Bearlow = Math.Min(Bearlow, _Series.LowPrices[index-1]);
                    bearLows[index] = Math.Min(Bearlow, _Series.LowPrices[index-1]);   
                    bearHighs[index] = Math.Max(Bearhigh, _Series.HighPrices[index-1]);

                    BearIndexesArr = BearIndexes.ToArray();
                    
                    OnBar(UseMa,UseLRF,UseStoch,UseSAR,index);
                }
           if (bullishBar)
                {

                    BullIndexes.Add(index-1);
                    
                    Bullhigh = Math.Max(Bullhigh, _Series.HighPrices[index-1]);
                    Bulllow = Math.Max(Bulllow, _Series.LowPrices[index-1]);
                    bullHighs[index] = Math.Max(Bullhigh, _Series.HighPrices[index-1]);
                    bullLows[index] = Math.Max(Bulllow, _Series.LowPrices[index-1]);

                    BullIndexesArr = BullIndexes.ToArray();
                    
                    OnBar(UseMa,UseLRF,UseStoch,UseSAR,index);
                }
        //Weekdays
        if(ShowPeriod)
        {
        todayCheck = Bars.OpenTimes[index].Date;
        WeekDay= Convert.ToDateTime(todayCheck);
        string WeekDayName =Convert.ToString(WeekDay.DayOfWeek);
        Monday=WeekDayName=="Monday";
        Wednesday=WeekDayName=="Wednesday";
        
                    if(Monday && !MondayMarked)
                        {
                        WednesdayMarked=false;
                            if(DaybearishBar)
                            {
                            //Chart.DrawVerticalLine("MondayBear"+todayCheck,todayCheck,Color.Crimson,1,LineStyle.Lines);
                            DrawHighLow( index);
                            MondayMarked=true;  
                            }
                            else if(DaybullishBar)
                            {
                            //Chart.DrawVerticalLine("MondayBull"+todayCheck,todayCheck,Color.DarkGreen,1,LineStyle.Lines);
                            DrawHighLow( index);
                            MondayMarked=true;  
                            }
                        }
                        
                    if(Wednesday && !WednesdayMarked)
                        {
                        WriteOut(High,Low);
                        MondayMarked=false;
                            if(DaybearishBar)
                            {
                            Chart.DrawVerticalLine("WednesdayBear"+todayCheck,todayCheck,Color.Crimson,1,LineStyle.Lines);
                            DrawHighLow( index);
                            WednesdayMarked=true;
                            }
                            else if(DaybullishBar)
                            {
                            Chart.DrawVerticalLine("WednesdayBull"+todayCheck,todayCheck,Color.DarkGreen,1,LineStyle.Lines);
                            DrawHighLow( index);
                            WednesdayMarked=true;
                            }
                        }
                  }
                  
                
        }
        void DrawHighLow(int index)
    {
    double High=Series.HighPrices.Last(1);
    double Low =Series.LowPrices.Last(1);
        if(Monday)
        {
        //Chart.DrawTrendLine("MondayHigh"+index,todayCheck,High,todayCheck.AddDays(2),High,Color.GhostWhite);
        //Chart.DrawTrendLine("MondayLow"+index,todayCheck,Low,todayCheck.AddDays(2),Low,Color.GhostWhite);
        
        }
        if(Wednesday)
        {
        Chart.DrawTrendLine("WednesdayHigh"+index,todayCheck,High,todayCheck.AddDays(2),High,Color.Yellow);
        Chart.DrawTrendLine("WednesdayLow"+index,todayCheck,Low,todayCheck.AddDays(2),Low,Color.Yellow);
        
        }
    }
        void OnBar(bool A,bool B,bool C,bool D,int index)
        {
            double StochResultK = Stoch.PercentK[index-1];
            double StochResultD = Stoch.PercentD[index-1];
            
            bool BearMarketCross = LRFResult.LastValue>MaResult.LastValue;
            bool BullMarketCross = LRFResult.LastValue<MaResult.LastValue;
            bool StochAboveThreshold = StochResultK > 50 & StochResultD > 50; 
            bool StochBelowThreshold = StochResultK < 50 & StochResultD < 50;
            bool priceAboveSAR = Close>PSARResult.LastValue;
            bool priceBelowSAR = Close<PSARResult.LastValue;
            
            bool BearCrossingBar = Open>MaResult.LastValue & Close<MaResult.LastValue;
            bool BullCrossingBar = Open<MaResult.LastValue & Close>MaResult.LastValue;
            
            Bearhigh = _Series.HighPrices.LastValue;
            Bearlow = _Series.LowPrices.LastValue;
            Bullhigh = _Series.HighPrices.LastValue;
            Bulllow = _Series.LowPrices.LastValue;
            
            bool NewBar = index > LastCalculatedIndex;

                if (NewBar)
                {
                    LastCalculatedIndex = index;
                        
                if (bearishBar)
                    {LongStoplossSeries[index-1]=bearLows.LastValue;
                        if (BearIndexesArr.Length>0)
                        {
                        LongEntry = bearHighs.LastValue;
                        LongEntrySeries[index-1] = LongEntry;
                        
                        BearSignalNotSent=true;
                        BuySignalSent[index-1]=0;
                        }
                        
                        //Indicator Options
                        if (A & B & C & D)
                        {
                            if(Close<ShortEntry & BullSignalNotSent & BearCrossingBar  & BearMarketCross & StochAboveThreshold & priceBelowSAR)
                            {
                            
                            SellSignal[index-1]=High+SignalSpace;
                            BullSignalNotSent=false;
                            SellSignalSent[index-1]=1;
                            }
                        }
                        else if(A & B & C & !D)
                        {
                            if(Close<ShortEntry & BullSignalNotSent & BearCrossingBar  & BearMarketCross & StochAboveThreshold)
                            {
                            SellSignal[index-1]=High+SignalSpace;
                            BullSignalNotSent=false;
                            SellSignalSent[index-1]=1;
                            }
                        }
                        else if(A & B & !C & D)
                        {
                            if(Close<ShortEntry & BullSignalNotSent & BearCrossingBar  & BearMarketCross & priceBelowSAR)
                            {
                            SellSignal[index-1]=High+SignalSpace;
                            BullSignalNotSent=false;
                            SellSignalSent[index-1]=1;
                            }
                        }
                        else if(A & !B & C & D)
                        {
                            if(Close<ShortEntry & BullSignalNotSent & BearCrossingBar  & StochAboveThreshold & priceBelowSAR)
                            {
                            SellSignal[index-1]=High+SignalSpace;
                            BullSignalNotSent=false;
                            SellSignalSent[index-1]=1;
                            }
                        }
                        else if(!A & B & C & D)
                        {
                            if(Close<ShortEntry & BullSignalNotSent & BearMarketCross & StochAboveThreshold & priceBelowSAR)
                            {
                            SellSignal[index-1]=High+SignalSpace;
                            BullSignalNotSent=false;
                            SellSignalSent[index-1]=1;
                            }
                        }//2
                        else if(A & B & !C & !D)
                        {
                            if(Close<ShortEntry & BullSignalNotSent & BearCrossingBar & BearMarketCross)
                            {
                            SellSignal[index-1]=High+SignalSpace;
                            BullSignalNotSent=false;
                            SellSignalSent[index-1]=1;
                            }
                        }
                        else if(A & !B & !C & D)
                        {
                            if(Close<ShortEntry & BullSignalNotSent & BearCrossingBar & priceBelowSAR)
                            {
                            SellSignal[index-1]=High+SignalSpace;
                            BullSignalNotSent=false;
                            SellSignalSent[index-1]=1;
                            }
                        }
                        else if(!A & !B & C & D)
                        {
                            if(Close<ShortEntry & BullSignalNotSent & StochAboveThreshold & priceBelowSAR)
                            {
                            SellSignal[index-1]=High+SignalSpace;
                            BullSignalNotSent=false;
                            SellSignalSent[index-1]=1;
                            }
                        }
                        else if(!A & B & C & !D)
                        {
                            if(Close<ShortEntry & BullSignalNotSent & BearMarketCross & StochAboveThreshold)
                            {
                            SellSignal[index-1]=High+SignalSpace;
                            BullSignalNotSent=false;
                            SellSignalSent[index-1]=1;
                            }
                        }//2 skip 1
                        else if(A & !B & C & !D)
                        {
                            if(Close<ShortEntry & BullSignalNotSent & BearCrossingBar & StochAboveThreshold)
                            {
                            SellSignal[index-1]=High+SignalSpace;
                            BullSignalNotSent=false;
                            SellSignalSent[index-1]=1;
                            }
                        }
                        else if(!A & B & !C & D)
                        {
                            if(Close<ShortEntry & BullSignalNotSent  & BearMarketCross & priceBelowSAR)
                            {
                            SellSignal[index-1]=High+SignalSpace;
                            BullSignalNotSent=false;
                            SellSignalSent[index-1]=1;
                            }
                        }//2 skip 2
                        else if(!A & B & C & !D)
                        {
                            if(Close<ShortEntry & BullSignalNotSent  & BearCrossingBar & priceBelowSAR)
                            {
                            SellSignal[index-1]=High+SignalSpace;
                            BullSignalNotSent=false;
                            SellSignalSent[index-1]=1;
                            }
                        }//3
                        else if(A & !B & !C & !D)
                        {
                            if(Close<ShortEntry & BullSignalNotSent  & BearCrossingBar)
                            {
                            SellSignal[index-1]=High+SignalSpace;
                            BullSignalNotSent=false;
                            SellSignalSent[index-1]=1;
                            }
                        }
                        else if(!A & !B & !C & D)
                        {
                            if(Close<ShortEntry & BullSignalNotSent  & priceBelowSAR)
                            {
                            SellSignal[index-1]=High+SignalSpace;
                            BullSignalNotSent=false;
                            SellSignalSent[index-1]=1;
                            }
                        }
                        else if(!A & !B & C & !D)
                        {
                            if(Close<ShortEntry & BullSignalNotSent  & StochAboveThreshold)
                            {
                            SellSignal[index-1]=High+SignalSpace;
                            BullSignalNotSent=false;
                            SellSignalSent[index-1]=1;
                            }
                        }
                        else if(!A & B & !C & !D)
                        {
                            if(Close<ShortEntry & BullSignalNotSent  & BearMarketCross)
                            {
                            SellSignal[index-1]=High+SignalSpace;
                            BullSignalNotSent=false;
                            SellSignalSent[index-1]=1;
                            }
                        }
                        else 
                        {//all off
                            if(Close<ShortEntry & BullSignalNotSent)
                            {
                            SellSignalSent[index-1]=1;
                            SellSignal[index-1]=High+SignalSpace;
                            BullSignalNotSent=false;
                            BuySignalSent[index-1]=0;
                            LongStoplossSeries[index-1]=bearLows.LastValue;
                            }
                        }
                    }
        
               if (bullishBar)
                    {
                        
                        
                        if (BullIndexesArr.Length>0 )
                        {
                        ShortEntry = bullLows.LastValue;
                        ShortEntrySeries[index-1] = ShortEntry;
                        ShortStoplossSeries[index-1] = bullHighs.LastValue;
                        BullSignalNotSent=true;
                        SellSignalSent[index-1]=0;
                        }
                        //Indicator Options
                        if (A & B & C & D)
                        {
                        
                            if(Close>LongEntry & BearSignalNotSent & BullCrossingBar  & BullMarketCross & StochBelowThreshold & priceAboveSAR)
                            {
                            ;
                            BuySignal[index-1]=Low-SignalSpace;
                            BearSignalNotSent=false;
                            BuySignalSent[index-1]=1;
                            }
                            
                        }
                        else if(A & B & C & !D)
                        {
                            if(Close>LongEntry & BearSignalNotSent & BullCrossingBar  & BullMarketCross & StochBelowThreshold)
                            {
                            //BuySignalSent[index]=1;
                            BuySignal[index-1]=Low-SignalSpace;
                            BearSignalNotSent=false;
                            
                            }
                        }
                        else if(A & B & !C & D)
                        {
                            if(Close>LongEntry & BearSignalNotSent & BullCrossingBar  & BullMarketCross & priceAboveSAR)
                            {
                            BuySignalSent[index-1]=1;
                            BuySignal[index-1]=Low-SignalSpace;
                            BearSignalNotSent=false;
                            
                            }
                        }
                        else if(A & !B & C & D)
                        {
                            if(Close>LongEntry & BearSignalNotSent & BullCrossingBar  & StochBelowThreshold & priceAboveSAR)
                            {
                            BuySignalSent[index-1]=1;
                            BuySignal[index-1]=Low-SignalSpace;
                            BearSignalNotSent=false;
                            
                            }
                        }
                        else if(!A & B & C & D)
                        {
                            if(Close>LongEntry & BearSignalNotSent & BullMarketCross & StochBelowThreshold & priceAboveSAR)
                            {
                            BuySignalSent[index-1]=1;
                            BuySignal[index-1]=Low-SignalSpace;
                            BearSignalNotSent=false;
                            
                            }
                        }//2
                        else if(A & B & !C & !D)
                        {
                            if(Close>LongEntry & BearSignalNotSent & BullCrossingBar & BullMarketCross)
                            {
                            BuySignalSent[index-1]=1;
                            BuySignal[index-1]=Low-SignalSpace;
                            BearSignalNotSent=false;
                            
                            }
                        }
                        else if(A & !B & !C & D)
                        {
                            if(Close>LongEntry & BearSignalNotSent & BullCrossingBar & priceAboveSAR)
                            {
                            BuySignalSent[index-1]=1;
                            BuySignal[index-1]=Low-SignalSpace;
                            BearSignalNotSent=false;
                            
                            }
                        }
                        else if(!A & !B & C & D)
                        {
                            if(Close>LongEntry & BearSignalNotSent & StochBelowThreshold & priceAboveSAR)
                            {
                            BuySignalSent[index-1]=1;
                            BuySignal[index-1]=Low-SignalSpace;
                            BearSignalNotSent=false;
                            
                            }
                        }
                        else if(!A & B & C & !D)
                        {
                            if(Close>LongEntry & BearSignalNotSent & BullMarketCross & StochBelowThreshold)
                            {
                            BuySignalSent[index-1]=1;
                            BuySignal[index-1]=Low-SignalSpace;
                            BearSignalNotSent=false;
                            
                            }
                        }//2 skip 1
                        else if(A & !B & C & !D)
                        {
                            if(Close>LongEntry & BearSignalNotSent & BullCrossingBar & StochBelowThreshold)
                            {
                            BuySignalSent[index-1]=1;
                            BuySignal[index-1]=Low-SignalSpace;
                            BearSignalNotSent=false;
                            
                            }
                        }
                        else if(!A & B & !C & D)
                        {
                            if(Close>LongEntry & BearSignalNotSent  & BullMarketCross & priceAboveSAR)
                            {
                            BuySignalSent[index-1]=1;
                            BuySignal[index-1]=Low-SignalSpace;
                            BearSignalNotSent=false;
                            
                            }
                        }//2 skip 2
                        else if(!A & B & C & !D)
                        {
                            if(Close>LongEntry & BearSignalNotSent  & BullCrossingBar & priceAboveSAR)
                            {
                            BuySignalSent[index-1]=1;
                            BuySignal[index-1]=Low-SignalSpace;
                            BearSignalNotSent=false;
                            
                            }
                        }//3
                        else if(A & !B & !C & !D)
                        {
                            if(Close>LongEntry & BearSignalNotSent  & BullCrossingBar)
                            {
                            BuySignalSent[index-1]=1;
                            BuySignal[index-1]=Low-SignalSpace;
                            BearSignalNotSent=false;
                            
                            }
                        }
                        else if(!A & !B & !C & D)
                        {
                            if(Close>LongEntry & BearSignalNotSent  & priceAboveSAR)
                            {
                            BuySignalSent[index-1]=1;
                            BuySignal[index-1]=Low-SignalSpace;
                            BearSignalNotSent=false;
                            
                            }
                        }
                        else if(!A & !B & C & !D)
                        {
                            if(Close>LongEntry & BearSignalNotSent  & StochBelowThreshold)
                            {
                            BuySignalSent[index-1]=1;
                            BuySignal[index-1]=Low-SignalSpace;
                            BearSignalNotSent=false;
                            
                            }
                        }
                        else if(!A & B & !C & !D)
                        {
                            if(Close>LongEntry & BearSignalNotSent  & BullMarketCross)
                            {
                            BuySignalSent[index-1]=1;
                            BuySignal[index-1]=Low-SignalSpace;
                            BearSignalNotSent=false;
                            
                            }
                        }
                        else 
                        {
                            if(Close>LongEntry & BearSignalNotSent )
                            {
                            BuySignalSent[index-1]=1;
                            BuySignal[index-1]=Low-SignalSpace;
                            BearSignalNotSent=false;
                            SellSignalSent[index-1]=0;
                            ShortStoplossSeries[index] = bullHighs.LastValue;
                            }
                        }
                    }
                }
            }
            
            void WriteOut(double High, double Low)
            {
            var stringBuilder = new StringBuilder();
            double pips = (High-Low);
            stringBuilder.AppendLine("High: " + High);
            stringBuilder.AppendLine("Low: " + Low);
            stringBuilder.AppendLine("Pips: " + pips);

            Chart.DrawStaticText("Status", stringBuilder.ToString(), VerticalAlignment.Top, HorizontalAlignment.Left, Color.BlanchedAlmond);  
            }
    }
}

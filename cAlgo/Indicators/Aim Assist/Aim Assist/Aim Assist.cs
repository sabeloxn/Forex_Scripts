using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;
using System.Collections.Generic;
using System.Net;
namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = true, AccessRights = AccessRights.None)]
    public class AimAssist : Indicator
    {
        
       //--Moving Average
        [Parameter("Turn On", DefaultValue = true,Group ="Moving Average")]
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
        [Parameter("Turn On", DefaultValue = false,Group ="Prabolic SAR")]
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
       //--Stocahstic Oscillator
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
        [Output("Long Sgnal",LineColor = "#709CFFF7",PlotType =PlotType.Points, Thickness = 5)]
        public IndicatorDataSeries BuySignal{get;set;}
        [Parameter("Signal Space", DefaultValue =0.01,Group ="Signals")]
        public double SignalSpace{get;set;}
        
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

        public IndicatorDataSeries bullHighs;
        public IndicatorDataSeries bearHighs;
        public IndicatorDataSeries bearLows;
        public IndicatorDataSeries bullLows;
        public Bars _Series;
        
        public double ShortEntry{get;set;}
        public double LongEntry;
       //Entry Prices and Stop Losses
        [Output("ShortEntrySeries")]
        public IndicatorDataSeries ShortEntrySeries{get;set;}
        [Output("LongEntrySeries")]
        public IndicatorDataSeries LongEntrySeries{get;set;}
        [Output("ShortStoplossSeries")]
        public IndicatorDataSeries ShortStoplossSeries{get;set;} 
        [Output("LongStoplossSeries")]
        public IndicatorDataSeries LongStoplossSeries{get;set;}
        
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
        
        [Output("Sell Signal Sent")]
        public IndicatorDataSeries SellSignalSent{get;set;}
        [Output("Buy Signal Sent")]
        public IndicatorDataSeries BuySignalSent{get;set;}
        
        protected override void Initialize()
        {
            _Series = MarketData.GetBars(TimeFrame);
            bullLows = CreateDataSeries();
            bullHighs = CreateDataSeries();
            bearLows = CreateDataSeries();
            bearHighs = CreateDataSeries();
            //ShortEntrySeries=CreateDataSeries();
            //LongEntrySeries=CreateDataSeries();
            //ShortStoplossSeries=CreateDataSeries();
            //LongStoplossSeries=CreateDataSeries();
            
            _PSAR = Indicators.ParabolicSAR(Minaf, Maxaf);
            _MA = Indicators.MovingAverage(_Series.ClosePrices,MaPeriods,MaType);
            _LRF = Indicators.LinearRegressionForecast(_Series.ClosePrices, LRFPeriod);
            Stoch = Indicators.StochasticOscillator(_Series, kPeriods, kSlowing, dPeriods, StochMaType);
        }
        public override void Calculate(int index)
        {
            PSARResult[index+PSARshift] = _PSAR.Result[index];
            MaResult[index]=_MA.Result.LastValue;
            LRFResult[index] = _LRF.Result[index];
            
            DateTime todayCheck = _Series.OpenTimes[index].Date;
    
            High = _Series.HighPrices.LastValue;
            Low = _Series.LowPrices.LastValue;
            Open = _Series.OpenPrices.LastValue;
            Close = _Series.ClosePrices.LastValue;

            Bearhigh = High;
            Bearlow = Low;
            Bullhigh = High;
            Bulllow = Low;

            bullishBar = Close > Open;
            bearishBar = Close < Open;

            for (int i = _Series.ClosePrices.Count - 1; i > 0; i--)
            {
                bool NewBar = index > LastCalculatedIndex;
                if (NewBar)
                {
                    LastCalculatedIndex=index;
                    if (_Series.OpenTimes[i].Date < todayCheck)
                        break;
                    //High = Math.Max(High, _Series.HighPrices[i]);
                    //Low = Math.Min(Low, _Series.LowPrices[i]);
                    //Open = _Series.OpenPrices[i];
                    //Close = _Series.ClosePrices[i];

                    previousOpen = _Series.OpenPrices[i+1];
                    previousClose = _Series.ClosePrices[i+1];
                    
                    GetAndStoreBars(index, i);
                    BareBones(UseMa,UseLRF,UseStoch,UseSAR,index);
                }
            }
            
        }
        void GetAndStoreBars(int index, int i)
        {
        if (bearishBar)
            {
                BearIndexes.Add(index);
                
                Bearhigh = Math.Max(Bearhigh, _Series.HighPrices[i]);
                Bearlow = Math.Min(Bearlow, _Series.LowPrices[i]);
                bearLows[index] = Math.Min(Bearlow, _Series.LowPrices[i]);   
                bearHighs[index] = Math.Max(Bearhigh, _Series.HighPrices[i]);
                
                BearIndexesArr = BearIndexes.ToArray();
                
            }
        if (bullishBar)
            {
                BullIndexes.Add(index);
                
                Bullhigh = Math.Max(Bullhigh, _Series.HighPrices[i]);
                Bulllow = Math.Max(Bulllow, _Series.LowPrices[i]);
                bullHighs[index] = Math.Max(Bullhigh, _Series.HighPrices[i]);
                bullLows[index] = Math.Max(Bulllow, _Series.LowPrices[i]);
                
                BullIndexesArr = BullIndexes.ToArray();
                
            }
        }
        void BareBones(bool A,bool B,bool C,bool D,int index)
        {
            double StochResultK = Stoch.PercentK[index];
            double StochResultD = Stoch.PercentD[index];
            
            bool BearMarketCross = LRFResult.LastValue>MaResult.LastValue;
            bool BullMarketCross = LRFResult.LastValue<MaResult.LastValue;
            bool StochAboveThreshold = StochResultK > 50 & StochResultD > 50; 
            bool StochBelowThreshold = StochResultK < 50 & StochResultD < 50;
            bool priceAboveSAR = Close>PSARResult.LastValue;
            bool priceBelowSAR = Close<PSARResult.LastValue;
            
            BearCrossingBar = Open>MaResult.LastValue & Close<MaResult.LastValue;
            BullCrossingBar = Open<MaResult.LastValue & Close>MaResult.LastValue;
            
        if (bearishBar)
            {
                if (BearIndexesArr.Length>0 & previousClose>previousOpen)
                {
                LongEntry = bearHighs.LastValue;
                LongEntrySeries[index] = LongEntry;
                LongStoplossSeries[index]=bearLows.LastValue;
                BearSignalNotSent=true;
                BuySignalSent[index]=0;
                }
                
                //Indicator Options
                if (A & B & C & D)
                {
                
                    if(Close<ShortEntry & BullSignalNotSent & BearCrossingBar  & BearMarketCross & StochAboveThreshold & priceBelowSAR)
                    {
                    SellSignalSent[index]=1;
                    SellSignal[index]=High+SignalSpace;
                    BullSignalNotSent=false;
                    SellSignalSent[index]=1;
                    }
                    
                }
                else if(A & B & C & !D)
                {
                    if(Close<ShortEntry & BullSignalNotSent & BearCrossingBar  & BearMarketCross & StochAboveThreshold)
                    {
                    SellSignal[index]=High+SignalSpace;
                    BullSignalNotSent=false;
                    SellSignalSent[index]=1;
                    }
                }
                else if(A & B & !C & D)
                {
                    if(Close<ShortEntry & BullSignalNotSent & BearCrossingBar  & BearMarketCross & priceBelowSAR)
                    {
                    SellSignal[index]=High+SignalSpace;
                    BullSignalNotSent=false;
                    SellSignalSent[index]=1;
                    }
                }
                else if(A & !B & C & D)
                {
                    if(Close<ShortEntry & BullSignalNotSent & BearCrossingBar  & StochAboveThreshold & priceBelowSAR)
                    {
                    SellSignal[index]=High+SignalSpace;
                    BullSignalNotSent=false;
                    SellSignalSent[index]=1;
                    }
                }
                else if(!A & B & C & D)
                {
                    if(Close<ShortEntry & BullSignalNotSent & BearMarketCross & StochAboveThreshold & priceBelowSAR)
                    {
                    SellSignal[index]=High+SignalSpace;
                    BullSignalNotSent=false;
                    SellSignalSent[index]=1;
                    }
                }//2
                else if(A & B & !C & !D)
                {
                    if(Close<ShortEntry & BullSignalNotSent & BearCrossingBar & BearMarketCross)
                    {
                    SellSignal[index]=High+SignalSpace;
                    BullSignalNotSent=false;
                    SellSignalSent[index]=1;
                    }
                }
                else if(A & !B & !C & D)
                {
                    if(Close<ShortEntry & BullSignalNotSent & BearCrossingBar & priceBelowSAR)
                    {
                    SellSignal[index]=High+SignalSpace;
                    BullSignalNotSent=false;
                    SellSignalSent[index]=1;
                    }
                }
                else if(!A & !B & C & D)
                {
                    if(Close<ShortEntry & BullSignalNotSent & StochAboveThreshold & priceBelowSAR)
                    {
                    SellSignal[index]=High+SignalSpace;
                    BullSignalNotSent=false;
                    SellSignalSent[index]=1;
                    }
                }
                else if(!A & B & C & !D)
                {
                    if(Close<ShortEntry & BullSignalNotSent & BearMarketCross & StochAboveThreshold)
                    {
                    SellSignal[index]=High+SignalSpace;
                    BullSignalNotSent=false;
                    SellSignalSent[index]=1;
                    }
                }//2 skip 1
                else if(A & !B & C & !D)
                {
                    if(Close<ShortEntry & BullSignalNotSent & BearCrossingBar & StochAboveThreshold)
                    {
                    SellSignal[index]=High+SignalSpace;
                    BullSignalNotSent=false;
                    SellSignalSent[index]=1;
                    }
                }
                else if(!A & B & !C & D)
                {
                    if(Close<ShortEntry & BullSignalNotSent  & BearMarketCross & priceBelowSAR)
                    {
                    SellSignal[index]=High+SignalSpace;
                    BullSignalNotSent=false;
                    SellSignalSent[index]=1;
                    }
                }//2 skip 2
                else if(!A & B & C & !D)
                {
                    if(Close<ShortEntry & BullSignalNotSent  & BearCrossingBar & priceBelowSAR)
                    {
                    SellSignal[index]=High+SignalSpace;
                    BullSignalNotSent=false;
                    SellSignalSent[index]=1;
                    }
                }//3
                else if(A & !B & !C & !D)
                {
                    if(Close<ShortEntry & BullSignalNotSent  & BearCrossingBar)
                    {
                    SellSignal[index]=High+SignalSpace;
                    BullSignalNotSent=false;
                    SellSignalSent[index]=1;
                    }
                }
                else if(!A & !B & !C & D)
                {
                    if(Close<ShortEntry & BullSignalNotSent  & priceBelowSAR)
                    {
                    SellSignal[index]=High+SignalSpace;
                    BullSignalNotSent=false;
                    SellSignalSent[index]=1;
                    }
                }
                else if(!A & !B & C & !D)
                {
                    if(Close<ShortEntry & BullSignalNotSent  & StochAboveThreshold)
                    {
                    SellSignal[index]=High+SignalSpace;
                    BullSignalNotSent=false;
                    SellSignalSent[index]=1;
                    }
                }
                else if(!A & B & !C & !D)
                {
                    if(Close<ShortEntry & BullSignalNotSent  & BearMarketCross)
                    {
                    SellSignal[index]=High+SignalSpace;
                    BullSignalNotSent=false;
                    SellSignalSent[index]=1;
                    }
                }
                else 
                {
                    if(Close<ShortEntry & BullSignalNotSent)
                    {
                    SellSignalSent[index]=0;
                    SellSignal[index]=High+SignalSpace;
                    BullSignalNotSent=false;
                    BuySignalSent[index]=0;
                    }
                }
            }

       if (bullishBar)
            {
                
                
                if (BullIndexesArr.Length>0 & previousClose<previousOpen)
                {
                ShortEntry = bullLows.LastValue;
                ShortEntrySeries[index] = ShortEntry;
                ShortStoplossSeries[index] = bullHighs.LastValue;
                BullSignalNotSent=true;
                SellSignalSent[index]=0;
                }
                //Indicator Options
                if (A & B & C & D)
                {
                
                    if(Close>LongEntry & BearSignalNotSent & BullCrossingBar  & BullMarketCross & StochBelowThreshold & priceAboveSAR)
                    {
                    ;
                    BuySignal[index]=Low-SignalSpace;
                    BearSignalNotSent=false;
                    BuySignalSent[index]=1;
                    }
                    
                }
                else if(A & B & C & !D)
                {
                    if(Close>LongEntry & BearSignalNotSent & BullCrossingBar  & BullMarketCross & StochBelowThreshold)
                    {
                    BuySignalSent[index]=1;
                    BuySignal[index]=Low-SignalSpace;
                    BearSignalNotSent=false;
                    
                    }
                }
                else if(A & B & !C & D)
                {
                    if(Close>LongEntry & BearSignalNotSent & BullCrossingBar  & BullMarketCross & priceAboveSAR)
                    {
                    BuySignalSent[index]=1;
                    BuySignal[index]=Low-SignalSpace;
                    BearSignalNotSent=false;
                    
                    }
                }
                else if(A & !B & C & D)
                {
                    if(Close>LongEntry & BearSignalNotSent & BullCrossingBar  & StochBelowThreshold & priceAboveSAR)
                    {
                    BuySignalSent[index]=1;
                    BuySignal[index]=Low-SignalSpace;
                    BearSignalNotSent=false;
                    
                    }
                }
                else if(!A & B & C & D)
                {
                    if(Close>LongEntry & BearSignalNotSent & BullMarketCross & StochBelowThreshold & priceAboveSAR)
                    {
                    BuySignalSent[index]=1;
                    BuySignal[index]=Low-SignalSpace;
                    BearSignalNotSent=false;
                    
                    }
                }//2
                else if(A & B & !C & !D)
                {
                    if(Close>LongEntry & BearSignalNotSent & BullCrossingBar & BullMarketCross)
                    {
                    BuySignalSent[index]=1;
                    BuySignal[index]=Low-SignalSpace;
                    BearSignalNotSent=false;
                    
                    }
                }
                else if(A & !B & !C & D)
                {
                    if(Close>LongEntry & BearSignalNotSent & BullCrossingBar & priceAboveSAR)
                    {
                    BuySignalSent[index]=1;
                    BuySignal[index]=Low-SignalSpace;
                    BearSignalNotSent=false;
                    
                    }
                }
                else if(!A & !B & C & D)
                {
                    if(Close>LongEntry & BearSignalNotSent & StochBelowThreshold & priceAboveSAR)
                    {
                    BuySignalSent[index]=1;
                    BuySignal[index]=Low-SignalSpace;
                    BearSignalNotSent=false;
                    
                    }
                }
                else if(!A & B & C & !D)
                {
                    if(Close>LongEntry & BearSignalNotSent & BullMarketCross & StochBelowThreshold)
                    {
                    BuySignalSent[index]=1;
                    BuySignal[index]=Low-SignalSpace;
                    BearSignalNotSent=false;
                    
                    }
                }//2 skip 1
                else if(A & !B & C & !D)
                {
                    if(Close>LongEntry & BearSignalNotSent & BullCrossingBar & StochBelowThreshold)
                    {
                    BuySignalSent[index]=1;
                    BuySignal[index]=Low-SignalSpace;
                    BearSignalNotSent=false;
                    
                    }
                }
                else if(!A & B & !C & D)
                {
                    if(Close>LongEntry & BearSignalNotSent  & BullMarketCross & priceAboveSAR)
                    {
                    BuySignalSent[index]=1;
                    BuySignal[index]=Low-SignalSpace;
                    BearSignalNotSent=false;
                    
                    }
                }//2 skip 2
                else if(!A & B & C & !D)
                {
                    if(Close>LongEntry & BearSignalNotSent  & BullCrossingBar & priceAboveSAR)
                    {
                    BuySignalSent[index]=1;
                    BuySignal[index]=Low-SignalSpace;
                    BearSignalNotSent=false;
                    
                    }
                }//3
                else if(A & !B & !C & !D)
                {
                    if(Close>LongEntry & BearSignalNotSent  & BullCrossingBar)
                    {
                    BuySignalSent[index]=1;
                    BuySignal[index]=Low-SignalSpace;
                    BearSignalNotSent=false;
                    
                    }
                }
                else if(!A & !B & !C & D)
                {
                    if(Close>LongEntry & BearSignalNotSent  & priceAboveSAR)
                    {
                    BuySignalSent[index]=1;
                    BuySignal[index]=Low-SignalSpace;
                    BearSignalNotSent=false;
                    
                    }
                }
                else if(!A & !B & C & !D)
                {
                    if(Close>LongEntry & BearSignalNotSent  & StochBelowThreshold)
                    {
                    BuySignalSent[index]=1;
                    BuySignal[index]=Low-SignalSpace;
                    BearSignalNotSent=false;
                    
                    }
                }
                else if(!A & B & !C & !D)
                {
                    if(Close>LongEntry & BearSignalNotSent  & BullMarketCross)
                    {
                    BuySignalSent[index]=1;
                    BuySignal[index]=Low-SignalSpace;
                    BearSignalNotSent=false;
                    
                    }
                }
                else 
                {
                    if(Close>LongEntry & BearSignalNotSent )
                    {
                    BuySignalSent[index]=0;//inspect
                    BuySignal[index]=Low-SignalSpace;
                    BearSignalNotSent=false;
                    
                    }
                }
            }
        }
        
    }
}

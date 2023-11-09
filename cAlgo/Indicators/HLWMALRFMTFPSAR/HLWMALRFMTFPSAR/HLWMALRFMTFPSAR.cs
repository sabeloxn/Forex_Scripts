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
    
    public class DailyHighLow : Indicator
    {

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
        //public string BarType ="";
        
        Color BullHighcolor = "Purple";
        Color BullLowcolor = "Purple";
        Color BearHighcolor = "White";
        Color BearLowcolor = "White";

        public IndicatorDataSeries bullHighs;
        public IndicatorDataSeries bearHighs;
        public IndicatorDataSeries bearLows;
        public IndicatorDataSeries bullLows;
        public Bars dailySeries;

        public double ShortEntry;
        public double LongEntry;
        
        public List<int> BearIndexes=new List <int>{};
        public int[] BearIndexesArr = new int[]{};
        
        public List<int> BullIndexes=new List <int>{};
        public int[] BullIndexesArr = new int[]{};
        
        bool BearSignalNotSent=true;
        bool BullSignalNotSent=true;
        
         private MovingAverage _ma;
         [Parameter("WMA Type", DefaultValue = MovingAverageType.Weighted)]
         public MovingAverageType MovingAverageType { get; set; }
         [Parameter("WMA Period", DefaultValue = 14)]
         public int MAPeriod { get; set; }
         [Output("WMA Main", LineColor = "#FF68D0F7", PlotType = PlotType.Points,Thickness = 5)]
         public IndicatorDataSeries Result { get; set; }
         
         private LinearRegressionForecast LRF;
        [Parameter("LRF Period", DefaultValue = 120)]
        public int LRFPeriod { get; set; }
        [Output("LRF Main",LineColor = "#FFFED966", LineStyle = LineStyle.DotsVeryRare, Thickness = 5)]
        public IndicatorDataSeries LRFResult { get; set; }
         
         private StochasticOscillator Stoch;
         public IndicatorDataSeries StochResultK;
         public IndicatorDataSeries StochResultD;
         
        [Parameter("Timeframe", DefaultValue = "Daily")]
        public TimeFrame iTimeFrame { get; set; }
        
        private ParabolicSAR _parabolic;
        [Parameter("Min AF", DefaultValue = 0.02)]
        public double minaf { get; set; }
        [Parameter("Max AF", DefaultValue = 0.2)]
        public double maxaf { get; set; }
        [Output("PSAR Main",LineColor = "#FFFED966", PlotType = PlotType.Points, Thickness = 3)]
        public IndicatorDataSeries PSARResult { get; set; }
        
        protected override void Initialize()
        {
            Chart.TryChangeTimeFrame(iTimeFrame);
            dailySeries = MarketData.GetBars(iTimeFrame);
            bullLows = CreateDataSeries();
            bullHighs = CreateDataSeries();
            bearLows = CreateDataSeries();
            bearHighs = CreateDataSeries();
            
            _ma = Indicators.MovingAverage(Bars.ClosePrices, MAPeriod, MovingAverageType);
            LRF = Indicators.LinearRegressionForecast(Bars.ClosePrices, LRFPeriod);
            Stoch = Indicators.StochasticOscillator(dailySeries, 24, 5, 9, MovingAverageType.Weighted);
            _parabolic = Indicators.ParabolicSAR(minaf, maxaf);
        }
        public override void Calculate(int index)
        {
            
            Result[index] = _ma.Result[index];
            LRFResult[index] = LRF.Result[index];
            double StochResultK = Stoch.PercentK[index];
            double StochResultD = Stoch.PercentD[index];
            PSARResult[index] = _parabolic.Result[index];
            
            DateTime today = dailySeries.OpenTimes[index].Date;
            DateTime todayCheck = dailySeries.OpenTimes[index].Date;
            DateTime tomorrow = today.AddDays(1);
    
            High = dailySeries.HighPrices.LastValue;
            Low = dailySeries.LowPrices.LastValue;
            Open = dailySeries.OpenPrices.LastValue;
            Close = dailySeries.ClosePrices.LastValue;

            Bearhigh = dailySeries.HighPrices.LastValue;
            Bearlow = dailySeries.LowPrices.LastValue;
            Bullhigh = dailySeries.HighPrices.LastValue;
            Bulllow = dailySeries.LowPrices.LastValue;
            
            

            bool bullishBar = Close > Open;
            bool bearishBar = Close < Open;
            var previousLow = Bars.LowPrices.Last(1);
            var previousHigh = Bars.HighPrices.Last(1);
            bool BearCrossingBar = Open>Result.LastValue & Close<Result.LastValue;
            bool BullCrossingBar = Open<Result.LastValue & Close>Result.LastValue;
            bool BearMarketCross = LRFResult.LastValue>Result.LastValue;
            bool BullMarketCross = LRFResult.LastValue<Result.LastValue;
            bool StochAboveThreshold = StochResultK > 50 & StochResultD > 50; 
            bool StochBelowThreshold = StochResultK < 50 & StochResultD < 50;
            bool priceAboveSAR = Close>PSARResult.LastValue;
            bool priceBelowSAR = Close<PSARResult.LastValue;
            
            //bool CrossOver = Close>LongEntry | Close<ShortEntry;
            
            //int[] BullIndexes = new int[]{};
            for (int i = dailySeries.ClosePrices.Count - 1; i > 0; i--)
            {

                if (dailySeries.OpenTimes[i].Date < todayCheck)
                    break;
                High = Math.Max(High, dailySeries.HighPrices[i]);
                Low = Math.Min(Low, dailySeries.LowPrices[i]);
                Open = dailySeries.OpenPrices[i];
                Close = dailySeries.ClosePrices[i];
                
                double previousOpen = dailySeries.OpenPrices[i+1];
                double previousClose = dailySeries.ClosePrices[i+1];
                
                if (bearishBar)
                {

                    BearIndexes.Add(index);
                    
                    Bearhigh = Math.Max(Bearhigh, dailySeries.HighPrices[i]);
                    Bearlow = Math.Min(Bearlow, dailySeries.LowPrices[i]);
                    bearLows[index] = Math.Min(Bearlow, dailySeries.LowPrices[i]);   //stoploss
                    bearHighs[index] = Math.Max(Bearhigh, dailySeries.HighPrices[i]);//entry
                    
                    // Print("Long Entry : {0} ", LongEntry);
                    /*int[]*/ BearIndexesArr = BearIndexes.ToArray();
                    
                    if (BearIndexesArr.Length>0 & previousClose>previousOpen)
                    {
                    //int differenceInDays=(BearIndexesArr[BearIndexesArr.Length-1])-(BearIndexesArr[BearIndexesArr.Length-2]);
                    //Chart.DrawTrendLine("Bear high " + today, today, Bearhigh, tomorrow.AddDays(differenceInDays), Bearhigh, BearHighcolor);
                    LongEntry = bearHighs.LastValue;//entry trigger
                    BearSignalNotSent=true;
                    }
                    if(Close<ShortEntry & BullSignalNotSent & BearCrossingBar  & BearMarketCross & StochAboveThreshold & priceBelowSAR)
                    {
                    Chart.DrawIcon("Sell Signal"+index, ChartIconType.DownTriangle, index, High+0.00, Color.Crimson);
                    BullSignalNotSent=false;
                    }

                }
                    
                    
                
                if (bullishBar)
                {
                    BullIndexes.Add(index);
                    
                    Bullhigh = Math.Max(Bullhigh, dailySeries.HighPrices[i]);
                    Bulllow = Math.Max(Bulllow, dailySeries.LowPrices[i]);
                    bullHighs[index] = Math.Max(Bullhigh, dailySeries.HighPrices[i]);
                    bullLows[index] = Math.Max(Bulllow, dailySeries.LowPrices[i]);
                    
                    BullIndexesArr = BullIndexes.ToArray();
                    
                    if (BullIndexesArr.Length>0 & previousClose<previousOpen)
                    {
                    //int differenceInDays=(BullIndexesArr[BullIndexesArr.Length-1])-(BullIndexesArr[BullIndexesArr.Length-2]);
                    //Chart.DrawTrendLine("Bull low " + today, today, Bulllow, tomorrow.AddDays(differenceInDays), Bulllow, BullLowcolor);
                    ShortEntry = bullLows.LastValue;//entry trigger
                    BullSignalNotSent=true;
                    }
                    if(Close>LongEntry & BearSignalNotSent & BullCrossingBar & BullMarketCross & StochBelowThreshold & priceAboveSAR)
                    {
                    Chart.DrawIcon("Buy Signal"+index, ChartIconType.UpTriangle, index, Low, Color.DodgerBlue);
                    BearSignalNotSent=false;
                    }
                }
            }
            
        }
    }
}

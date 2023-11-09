using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;
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
        public string BarType ="";
        
        Color BullHighcolor = "Red";
        Color BullLowcolor = "Red";
        Color BearHighcolor = "Green";
        Color BearLowcolor = "Green";

        public IndicatorDataSeries bullHighs;
        public IndicatorDataSeries bearHighs;
        public IndicatorDataSeries bearLows;
        public IndicatorDataSeries bullLows;
        public Bars dailySeries;

        public double ShortEntry;
        public double LongEntry;
        public int NumberOfGreenBars=0;
        protected override void Initialize()
        {
            dailySeries = MarketData.GetBars(TimeFrame.Daily);
            bullLows = CreateDataSeries();
            bullHighs = CreateDataSeries();
            bearLows = CreateDataSeries();
            bearHighs = CreateDataSeries();
            

        }
        public override void Calculate(int index)
        {
            DateTime today = dailySeries.OpenTimes[index].Date;

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
            
            
            for (int i = dailySeries.ClosePrices.Count - 1; i > 0; i--)
            {

                if (dailySeries.OpenTimes[i].Date < today)
                    break;
                High = Math.Max(High, dailySeries.HighPrices[i]);
                Low = Math.Min(Low, dailySeries.LowPrices[i]);
                Open = dailySeries.OpenPrices[i];
                Close = dailySeries.ClosePrices[i];

                if (bearishBar)
                {
                    Bearhigh = Math.Max(Bearhigh, dailySeries.HighPrices[i]);
                    Bearlow = Math.Min(Bearlow, dailySeries.LowPrices[i]);
                    bearLows[index] = Math.Min(Bearlow, dailySeries.LowPrices[i]);   //stoploss
                    bearHighs[index] = Math.Max(Bearhigh, dailySeries.HighPrices[i]);//entry
                    LongEntry = bearHighs.LastValue;
                    // Print("Long Entry : {0} ", LongEntry);

                }
                if (bullishBar)
                {
                    Bullhigh = Math.Max(Bullhigh, dailySeries.HighPrices[i]);
                    Bulllow = Math.Max(Bulllow, dailySeries.LowPrices[i]);
                    bullHighs[index] = Math.Max(Bullhigh, dailySeries.HighPrices[i]);
                    bullLows[index] = Math.Max(Bulllow, dailySeries.LowPrices[i]);
                    ShortEntry = bullLows.LastValue;
                    //Print("Short Entry : {0}", ShortEntry);

                }
            }
        
            if (bearishBar)
            {
                //Chart.DrawTrendLine("Bear high " + DrawStartday, DrawStartDay, Bearhigh, tomorrow, Bearhigh, BearHighcolor);

                 Chart.DrawTrendLine("Bear high " + today, today, Bearhigh, tomorrow, Bearhigh, BearHighcolor);

                //Chart.DrawTrendLine("Bear low " + today, today, Bearlow, tomorrow, Bearlow, bearLowcolor);
                
            }

            if (bullishBar)
            {
                // Chart.DrawTrendLine("Bull high " + today, today, Bullhigh, tomorrow, Bullhigh, bullHighcolor);
                Chart.DrawTrendLine("Bull low " + today, today, Bulllow, tomorrow, Bulllow, BullLowcolor);
                Chart.DrawIcon("Buy Signal", ChartIconType.UpArrow, today.AddDays(1), previousLow, Color.DodgerBlue);
            }
            
        }
    }
}

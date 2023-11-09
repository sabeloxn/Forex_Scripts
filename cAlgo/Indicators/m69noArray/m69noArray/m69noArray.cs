using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

namespace cAlgo
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class m69noArray : Indicator
    {
        public double high;
        public double low;
        public double open;
        public double close;

        public IndicatorDataSeries bullHighs;
        public IndicatorDataSeries bearHighs;
        public IndicatorDataSeries bearLows;
        public IndicatorDataSeries bullLows;
        Color bullHighcolor = "blue";
        Color bullLowcolor = "orange";
        Color bearHighcolor = "yellow";
        Color bearLowcolor = "purple";
        protected override void Initialize()
        {
            // Initialize and create nested indicators
            bullLows = CreateDataSeries();
            bullHighs = CreateDataSeries();
            bearLows = CreateDataSeries();
            bearHighs = CreateDataSeries();



        }

        public override void Calculate(int index)
        {
            // Calculate value at specified index
            // Result[index] = ...
            DateTime today = MarketSeries.OpenTime[index].Date;
            DateTime tomorrow = today.AddDays(1);

            high = MarketSeries.High.Last(1);
            low = MarketSeries.Low.Last(1);
            open = MarketSeries.Open.Last(1);
            close = MarketSeries.Close.Last(1);

            bool bullishBar = close > open;
            bool bearishBar = close < open;

            var Bearhigh = bearHighs.LastValue;
            var Bearlow = bearLows.LastValue;
            var Bullhigh = bullHighs.LastValue;
            var Bulllow = bullLows.LastValue;

            for (int i = index - 1; i > 0; i--)
            {

                if (MarketSeries.OpenTime[i].Date < today)
                    break;

                high = Math.Max(high, MarketSeries.High[i]);
                low = Math.Min(low, MarketSeries.Low[i]);
                open = MarketSeries.Open[i];
                close = MarketSeries.Close[i];

                if (bearishBar)
                {
                    Bearhigh = Math.Max(Bearhigh, MarketSeries.High[i]);
                    Bearlow = Math.Min(Bearlow, MarketSeries.Low[i]);
                }
                if (bullishBar)
                {
                    Bullhigh = Math.Max(Bullhigh, bullHighs[i]);
                    Bulllow = Math.Min(Bulllow, bullLows[i]);
                }

            }

            if (bearishBar)
            {
                //bearLows[index] = MarketSeries.Low[index];
                //bearHighs[index] = MarketSeries.High[index];
                //ChartObjects.DrawLine("high " + today, today, high, tomorrow, high, Colors.Pink);
                Chart.DrawTrendLine("Bearhigh" + today, today, Bearhigh, tomorrow, Bearhigh, bearHighcolor, 2, LineStyle.Solid);
                Chart.DrawTrendLine("Bearishlow" + today, today, Bearlow, tomorrow, Bearlow, bearLowcolor, 2, LineStyle.Solid);

            }

            if (bullishBar)
            {
                //bullHighs[index] = MarketSeries.High[index];
                //bullLows[index] = MarketSeries.Low[index];

                Chart.DrawTrendLine("Bullhigh" + today, today, Bullhigh, tomorrow, Bullhigh, bullHighcolor, 2, LineStyle.Solid);
                Chart.DrawTrendLine("Bulllow" + today, today, Bulllow, tomorrow, Bulllow, bullLowcolor, 2, LineStyle.Solid);
            }

        }
    }
}

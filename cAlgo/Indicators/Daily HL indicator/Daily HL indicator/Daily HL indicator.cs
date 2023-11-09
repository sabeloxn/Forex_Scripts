using System;
using cAlgo.API;

namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = true, AccessRights = AccessRights.None)]
    public class DailyHighLow : Indicator
    {
        public override void Calculate(int index)
        {
            DateTime today = MarketSeries.OpenTime[index].Date;
            DateTime tomorrow = today.AddDays(1);

            double high = MarketSeries.High.LastValue;
            double low = MarketSeries.Low.LastValue;
            double open = MarketSeries.Open.LastValue;
            double close = MarketSeries.Close.LastValue;

            for (int i = MarketSeries.Close.Count - 1; i > 0; i--)
            {
                if (MarketSeries.OpenTime[i].Date < today)
                    break;

                high = Math.Max(high, MarketSeries.High[i]);
                low = Math.Min(low, MarketSeries.Low[i]);
            }

            ChartObjects.DrawLine("high " + today, today, high, tomorrow, high, Colors.Pink);
            ChartObjects.DrawLine("low " + today, today, low, tomorrow, low, Colors.Pink);
        }
    }
}

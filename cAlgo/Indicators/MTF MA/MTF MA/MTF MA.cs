using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;

namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class MTFMA : Indicator
    {
        [Parameter(DefaultValue = 50)]
        public int Period { get; set; }

        [Output("MA", Color = Colors.Yellow)]
        public IndicatorDataSeries MA { get; set; }

        [Output("MA5", Color = Colors.Orange)]
        public IndicatorDataSeries MA5 { get; set; }

        [Output("MA10", Color = Colors.Red)]
        public IndicatorDataSeries MA10 { get; set; }

        private MarketSeries series5;
        private MarketSeries series10;

        private MovingAverage ma;
        private MovingAverage ma5;
        private MovingAverage ma10;

        protected override void Initialize()
        {
            series5 = MarketData.GetSeries(TimeFrame.Minute5);
            series10 = MarketData.GetSeries(TimeFrame.Minute10);

            ma = Indicators.MovingAverage(MarketSeries.Close, Period, MovingAverageType.Triangular);
            ma5 = Indicators.MovingAverage(series5.Close, Period, MovingAverageType.Triangular);
            ma10 = Indicators.MovingAverage(series10.Close, Period, MovingAverageType.Triangular);
        }

        public override void Calculate(int index)
        {
            MA[index] = ma.Result[index];

            var index5 = GetIndexByDate(series5, MarketSeries.OpenTime[index]);
            if (index5 != -1)
                MA5[index] = ma5.Result[index5];

            var index10 = GetIndexByDate(series10, MarketSeries.OpenTime[index]);
            if (index10 != -1)
                MA10[index] = ma10.Result[index10];
        }


        private int GetIndexByDate(MarketSeries series, DateTime time)
        {
            for (int i = series.Close.Count - 1; i > 0; i--)
            {
                if (time == series.OpenTime[i])
                    return i;
            }
            return -1;
        }
    }
}

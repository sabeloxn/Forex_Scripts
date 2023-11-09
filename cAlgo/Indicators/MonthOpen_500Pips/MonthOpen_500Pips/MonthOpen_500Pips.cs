using System;
using cAlgo.API;

namespace cAlgo.Indicators.PCL
{
    [Indicator(IsOverlay = true, AccessRights = AccessRights.None)]
    public class MonthOpen_500Pips : Indicator
    {
        [Output("DayOpen", Color = Colors.Green, PlotType = PlotType.Line, Thickness = 1)]
        public IndicatorDataSeries OpenDay { get; set; }

        [Output("WeekOpen", Color = Colors.Red, PlotType = PlotType.Line, Thickness = 2)]
        public IndicatorDataSeries OpenWeek { get; set; }

        [Output("MonthOpen", Color = Colors.Gold, PlotType = PlotType.Line, Thickness = 3)]
        public IndicatorDataSeries OpenMonth { get; set; }

        [Parameter("Show 100PipsLevels", DefaultValue = 1)]
        public bool Set100Levels { get; set; }

        [Parameter("Show 500PipsLevels", DefaultValue = 1)]
        public bool Set500Levels { get; set; }

        [Parameter("MinLevel", DefaultValue = 0, MinValue = 0)]
        public int MinLevel { get; set; }

        [Parameter("MaxLevel", DefaultValue = 200, MinValue = 2)]
        public int MaxLevel { get; set; }

        public double openprice1 = 0;
        public double openprice2 = 0;
        public double openprice3 = 0;

        public override void Calculate(int index)
        {

            if (index < 1)
            {
                // If first bar is first bar of the day set open
                if (MarketSeries.OpenTime[index].TimeOfDay == TimeSpan.Zero)
                {
                    OpenWeek[index] = MarketSeries.Open[index];
                    OpenMonth[index] = MarketSeries.Open[index];
                    OpenDay[index] = MarketSeries.Open[index];
                    return;
                }
            }

            DateTime openTime = MarketSeries.OpenTime[index];
            DateTime lastOpenTime = MarketSeries.OpenTime[index - 1];
            const string objectName = "messageNA";

            if (!ApplicableTimeFrame(openTime, lastOpenTime))
            {
                // Display message that timeframe is N/A
                const string text = "TimeFrame Not Applicable. Choose a lower Timeframe";
                ChartObjects.DrawText(objectName, text, StaticPosition.TopLeft, Colors.Red);
                return;
            }

            // If TimeFrame chosen is applicable remove N/A message
            ChartObjects.RemoveObject(objectName);

            // Plot Daily Open and Close
            PlotDailyOpenClose(openTime, lastOpenTime, index);

            // Day pips
            double Pips1 = 0;
            //if (Symbol.Ask > openprice1)
            Pips1 = (Symbol.Ask - openprice1) / Symbol.PipSize;
            //if (Symbol.Ask < openprice1)
            //Pips1 = (openprice1 - Symbol.Ask) / Symbol.PipSize;
            double Profit1 = (Pips1 / 100) * 1000;
            var text0 = "Day Open:    " + openprice1.ToString() + "\nPips: " + (int)Pips1;
            ChartObjects.DrawText("Day", text0, StaticPosition.TopLeft, Colors.Green);

            // Week pips
            double Pips2 = 0;
            //if (Symbol.Ask > openprice2)
            Pips2 = (Symbol.Ask - openprice2) / Symbol.PipSize;
            //if (Symbol.Ask < openprice2)
            //    Pips2 = (openprice2 - Symbol.Ask) / Symbol.PipSize;
            double Profit2 = (Pips2 / 100) * 1000;
            var text2 = "\n\nWeek Open: " + openprice2.ToString() + "\nPips: " + (int)Pips2;
            ChartObjects.DrawText("Week", text2, StaticPosition.TopLeft, Colors.Red);

            // Month pips
            double Pips3 = 0;
            //if (Symbol.Ask > openprice3)
            Pips3 = (Symbol.Ask - openprice3) / Symbol.PipSize;
            //if (Symbol.Ask < openprice3)
            //    Pips3 = (openprice3 - Symbol.Ask) / Symbol.PipSize;
            double Profit3 = (Pips3 / 100) * 1000;
            var text3 = "\n\n\n\nMonth Open: " + openprice3.ToString() + "\nPips: " + (int)Pips3;
            ChartObjects.DrawText("Month", text3, StaticPosition.TopLeft, Colors.Gold);

            // 100 pips levels
            if (Set100Levels && MinLevel < MaxLevel)
            {
                for (int i = MinLevel; i < MaxLevel; i++)
                {
                    ChartObjects.DrawHorizontalLine("Level" + i, i * 100 * Symbol.PipSize, Colors.Gray, 1, LineStyle.LinesDots);
                }
            }

            if (Set500Levels && MinLevel < MaxLevel)
            {
                for (int i = MinLevel; i < MaxLevel; i++)
                {
                    ChartObjects.DrawHorizontalLine("Level500" + i, i * 500 * Symbol.PipSize, Colors.DodgerBlue, 1, LineStyle.Solid);
                }
            }

        }

        private bool ApplicableTimeFrame(DateTime openTime, DateTime lastOpenTime)
        {
            // minutes difference between bars
            var timeFrameMinutes = (int)(openTime - lastOpenTime).TotalMinutes;

            bool daily = timeFrameMinutes == 1440;
            bool weeklyOrGreater = timeFrameMinutes >= 7200;

            bool timeFrameNotApplicable = daily || weeklyOrGreater;

            if (timeFrameNotApplicable)
                return false;

            return true;
        }

        private void PlotDailyOpenClose(DateTime openTime, DateTime lastOpenTime, int index)
        {

            DateTime currentTime = MarketSeries.OpenTime[MarketSeries.OpenTime.Count - 1];
            DateTime previousTime = MarketSeries.OpenTime[MarketSeries.OpenTime.Count - 2];



            // Day change
            if (openTime.Day != lastOpenTime.Day)
            {
                // Plot Open
                OpenDay[index] = MarketSeries.Open[index];
                openprice1 = OpenDay[index];

            }
            // Same Day
            else
            {
                // Plot Open
                OpenDay[index] = OpenDay[index - 1];
                openprice1 = OpenDay[index];
            }




            // Week change
            if (currentTime.DayOfWeek == DayOfWeek.Monday && previousTime.DayOfWeek != DayOfWeek.Monday)
            {
                // Plot Open
                OpenWeek[index] = MarketSeries.Open[index];
                openprice2 = OpenWeek[index];
            }
            // Same Day
            else
            {
                // Plot Open
                OpenWeek[index] = OpenWeek[index - 1];
                openprice2 = OpenWeek[index];
            }

            // Month
            if (currentTime.Month == currentTime.Month && previousTime.Month != currentTime.Month)
            {
                // Plot Open
                OpenMonth[index] = MarketSeries.Open[index];
                openprice3 = OpenMonth[index];
            }
            // Same Day
            else
            {
                // Plot Open
                OpenMonth[index] = OpenMonth[index - 1];
                openprice3 = OpenMonth[index];
            }

            // Plot todays close
            DateTime today = DateTime.Now.Date;
            if (openTime.Date != today)
                return;

        }
    }
}

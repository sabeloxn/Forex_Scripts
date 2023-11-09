using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    [Indicator(IsOverlay =true, AccessRights = AccessRights.None)]
    
    public class Showweekdays : Indicator
    {
        public int lastIndex =-1;
        public Bars Series;
        public bool MondayMarked;
        public bool WednesdayMarked;
        public bool Monday;
        public bool Wednesday;
        DateTime todayCheck ;
        DateTime WeekDay;
        [Output("1")]
        public IndicatorDataSeries LongEntry{get;set;}
        [Output("2")]
        public IndicatorDataSeries ShortEntry{get;set;}
        [Parameter("Show Weekdays",DefaultValue =true)]
        public bool ShowWeekdays{get;set;}
        
        protected override void Initialize()
        {
            Series=MarketData.GetBars(TimeFrame.Daily);
            //LongEntry=CreateDataSeries();
            //ShortEntry=CreateDataSeries();
        }

        public override void Calculate(int index)
        {
        //LongEntry[index]=Series.HighPrices.Last(1);
        //ShortEntry[index]=Series.LowPrices.Last(1);
        todayCheck = Bars.OpenTimes[index].Date;
        WeekDay= Convert.ToDateTime(todayCheck);
        string WeekDayName =Convert.ToString(WeekDay.DayOfWeek);
        Monday=WeekDayName=="Monday";
        Wednesday=WeekDayName=="Wednesday";
        Draw(index);
    }
    
    void Draw(int index)
    {
        for(int i = Series.ClosePrices.Count-1;i>0;i--)
        {
        bool newbar=index>lastIndex;
            if(newbar)
            {
            lastIndex=index;
    
                if(Monday && !MondayMarked)
                {
                WednesdayMarked=false;
                Chart.DrawVerticalLine("Monday"+todayCheck,todayCheck,Color.DarkGray,1,LineStyle.Lines);
                DrawHighLow( index);
                MondayMarked=true;            
                }
                if(Wednesday && !WednesdayMarked)
                {
                MondayMarked=false;
                Chart.DrawVerticalLine("Wednesday"+todayCheck,todayCheck,Color.DarkGray,1,LineStyle.Lines);
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
        Chart.DrawTrendLine("MondayHigh"+index,todayCheck,High,todayCheck.AddDays(2),High,Color.CadetBlue);
        Chart.DrawTrendLine("MondayLow"+index,todayCheck,Low,todayCheck.AddDays(2),Low,Color.CadetBlue);
        LongEntry[index]=High;
        ShortEntry[index]=Low;
        }
        if(Wednesday)
        {
        Chart.DrawTrendLine("WednesdayHigh"+index,todayCheck,High,todayCheck.AddDays(2),High,Color.CadetBlue);
        Chart.DrawTrendLine("WednesdayLow"+index,todayCheck,Low,todayCheck.AddDays(2),Low,Color.CadetBlue);
        LongEntry[index]=High;
        ShortEntry[index]=Low;
        }
    }
}
}
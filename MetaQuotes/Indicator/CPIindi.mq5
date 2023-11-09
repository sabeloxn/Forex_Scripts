//+------------------------------------------------------------------+
//|                                                 CPIindicator.mq5 |
//|                                           Copyright SabeloN 2023 |
//|                                                                  |
//+------------------------------------------------------------------+
input string IndicatorName="cpiIndi";        //Indicator Short Name
input int HistoryBarsToLoad=1000;
input string   StartTime  ="1425";
int StartHour=0;
int StartMinute=0;
//--- indicator buffers
double         DayBuffer[];
MqlRates mrate[];
double LongEntry, ShortEntry;
//+------------------------------------------------------------------+
//| Custom indicator initialization function                         |
//+------------------------------------------------------------------+
int OnInit()
  {OnInitInitialization();
//--- indicator buffers mapping
   SetIndexBuffer(0,DayBuffer,INDICATOR_DATA);
   
//---
   return(INIT_SUCCEEDED);
  }
//+------------------------------------------------------------------+
//| Expert deinitialization function                                 |
//+------------------------------------------------------------------+ 
  void OnDeinit(const int reason)
  {
   CleanChart();        //Removes graphical objects from the chart
  } 
//+------------------------------------------------------------------+
//| Custom indicator iteration function                              |
//+------------------------------------------------------------------+
int OnCalculate(const int rates_total,
                const int prev_calculated,
                const datetime &time[],
                const double &open[],
                const double &high[],
                const double &low[],
                const double &close[],
                const long &tick_volume[],
                const long &volume[],
                const int &spread[])
  {
//---
   
   int CandleNumber=iBars(Symbol(),PERIOD_CURRENT);
   bool NewCandleAppeared=false;
   NewCandleAppeared = CheckForNewCandle(CandleNumber);
   ArraySetAsSeries(mrate,true);
   int mrate_copied=CopyRates(NULL,PERIOD_CURRENT,0,HistoryBarsToLoad,mrate);
   for(int i=HistoryBarsToLoad-1; i>0; i--)
     {
         MqlDateTime BarTime ;
         TimeToStruct(mrate[i].time,BarTime);
         string BarTimeStr=(string)BarTime.year+"."+(string)BarTime.mon+"."+(string)BarTime.day+" "+(string)StartHour+":"+(string)StartMinute;
         datetime BarTimeTmp = StringToTime(BarTimeStr);
         
         if(NewCandleAppeared)
         {
         if(TimeCurrent()>BarTimeTmp)
            {
               LongEntry=iHigh(Symbol(),PERIOD_CURRENT,i);
               ShortEntry=iLow(Symbol(),PERIOD_CURRENT,i);
               
               DrawPeriod(BarTimeTmp);
            }
   }
   }
   //--- return value of prev_calculated for next call
   return(rates_total);
  }
//+------------------------------------------------------------------+
//+------------------------------------------------------------------+
//|  Function to check for a new candle, to do stuff on bar          |
//+------------------------------------------------------------------+  
//+--
 bool CheckForNewCandle(int CandleNumber)
 {
   static int LastCandleNumber;

   bool isNewCandle=false;
   if (CandleNumber>LastCandleNumber)
   {
   isNewCandle=true;
   LastCandleNumber=CandleNumber;
   
   }
   return isNewCandle;
 }
 
 void DrawPeriod(datetime BarOpenTime)
 {
   string AreaName =IndicatorName+"-Level-"+TimeToString(BarOpenTime);
   
   ObjectCreate(0,AreaName,OBJ_VLINE,0,BarOpenTime,0);
   ObjectSetInteger(0,AreaName,OBJPROP_COLOR,clrDarkCyan);
   ObjectSetInteger(0,AreaName,OBJPROP_BACK,true);
   ObjectSetInteger(0,AreaName,OBJPROP_STYLE,STYLE_DOT);
   ObjectSetInteger(0,AreaName,OBJPROP_HIDDEN,true);
   ObjectSetInteger(0,AreaName,OBJPROP_FILL,false);
   ObjectSetInteger(0,AreaName,OBJPROP_SELECTED,false);
   ObjectSetInteger(0,AreaName,OBJPROP_WIDTH,1);
   }
//+------------------------------------------------------------------+
//|  Function to remove graphical objects from the chart             |
//+------------------------------------------------------------------+     

void CleanChart(){
   //Set the Windows to 0 means used the current chart
   int Window=0;
   //Scan all the graphical objects in the current chart and delete them if their name contains the indicator name
   for(int i=ObjectsTotal(ChartID(),Window,-1)-1;i>=0;i--){
      if(StringFind(ObjectName(0,i),IndicatorName,0)>=0){
         ObjectDelete(0,ObjectName(0,i));
      }
   }
}
void OnInitInitialization(){
   StartHour=(int)StringSubstr(StartTime,0,2);
   StartMinute=(int)StringSubstr(StartTime,2,2);
   }
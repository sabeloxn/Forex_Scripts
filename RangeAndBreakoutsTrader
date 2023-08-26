//+------------------------------------------------------------------+
//|                                            RangeAndBreakouts.mq5 |
//|                                                   Copyright 2022 |
//|                                                                  |
//+------------------------------------------------------------------+
#property description   "WARNING : You use this software at your own risk."
#property description   "The creator of these plugins cannot be held responsible for any damage or loss."

#property indicator_chart_window
#property indicator_buffers 0
#property indicator_plots 0

input string Comment1="========================";     //TradingDay Trading Session Time
input string IndicatorName="Range-And-Breakouts";        //Indicator Short Name
input double StopLoss =0.50;                          //Stop Loss Points
input double TakeProfit=1.50;                         //Take Profit Points
input bool   ShowRangeTargets=true;                   //Show Range SL and TP Levels?
input string Comment2="========================";     //Indicator Parameters
input string TimeLineStart="0000";                    //Start Time To Draw (Format 24H HHMM)
input string TimeLineEnd="2359";                      //End Time To Draw (Optional - Format HHMM)
input string DayStart="0000";                         //Start Time To Draw (Format 24H HHMM)
input string DayEnd="2359";                           //End Time To Draw (Optional - Format HHMM)
input string SignalStart="0000";                      //Start Time To Send Signal (Format 24H HHMM)
 bool ShowMonday=true;                                //Show If Monday
 bool ShowTuesday=true;                               //Show If Tuesday
 bool ShowWednesday=true;                             //Show If Wednesday
 bool ShowThursday=true;                              //Show If Thursday
 bool ShowFriday=true;                                //Show If Friday
input bool ShowSaturday=false;                        //Show If Saturday
input bool ShowSunday=false;                          //Show If Sunday
input int BarsToScan=10;                              //Maximum Bars To Search (0=No Limit)
input int OrangeBarsToScan=5;                        //Maximum Bars Daily To Search (0=No Limit)
input string Comment_3="====================";        //Objects Options
input color LineColor=clrGray;                        //Range Objects Color
input color LineColor1=clrCornflowerBlue;             //Buy Objects Color
input color LineColor2=clrSalmon;                     //Sell Objects Color

input color LabelColor=clrGray;                       //Label Objects Color
input int LineTickness=1;                             //Objects Thickness (For Line, Set 1 to 5)
//--
//First TimeFrame
int StartHour=0;
int StartMinute=0;
int EndHour=0;
int EndMinute=0;
int BarsInChart=0;

//Second Timeframe
int StartHour1=0;
int StartMinute1=0;
int EndHour1=0;
int EndMinute1=0;
int BarsInM30Chart=0;

//Third Timeframe
int StartHour2=0;
int StartMinute2=0;
int BarsInH1Chart=0;

//--
int chart_ID;
double OrangeHighPoint, OrangeLowPoint;
datetime SetEndTimeTmp,TodayEndTimeTmp,dayStartTime;
bool SignalSent,BreakoutSignalSent,SignalsPending,H1BreakoutSignalSent,NonTradingDay;
string SignalDate;
MqlRates M5mrate[]; 
MqlRates H1mrate[];
//+------------------------------------------------------------------+
//| Custom indicator initialization function                         |
//+------------------------------------------------------------------+
int OnInit(void){

   OnInitInitialization();       //Internal function to initialize other variables
   NonTradingDay=false;
   SignalSent=false;
   BreakoutSignalSent=false;
   H1BreakoutSignalSent=false;
   SignalsPending=false;
   
   MqlDateTime TodayStartTimeStruct;
   TimeToStruct(TimeCurrent(),TodayStartTimeStruct);
   string TodayStartTimeStructStr=(string)TodayStartTimeStruct.year+"."+(string)TodayStartTimeStruct.mon+"."+(string)TodayStartTimeStruct.day+" "+(string)StartHour2+":"+(string)StartMinute2;
   dayStartTime=StringToTime(TodayStartTimeStructStr);
   
   SignalDate=(string)TodayStartTimeStruct.year+"."+(string)TodayStartTimeStruct.mon+"."+(string)TodayStartTimeStruct.day;
   datetime TodayStartTimeTmp=StringToTime(TodayStartTimeStructStr);
   
   if(TodayStartTimeStruct.day_of_week==6 || TodayStartTimeStruct.day_of_week==0)//If it's a Saturday or Sunday 
   NonTradingDay=true;

   chart_ID=0;
   int x=50, y=50;
   IndicatorSetString(INDICATOR_SHORTNAME,IndicatorName);      //Set the indicator name
   
   if(!OnInitPreChecksPass()){   //Check to see there are requirements that need to be met in order to run
      return(INIT_FAILED);
   }   
   //--Start Painting indicator
   CleanChart();
   DrawAreas();
   
   BarsInChart=Bars(Symbol(),PERIOD_M5);
   BarsInH1Chart=Bars(Symbol(),PERIOD_H1);
   BarsInM30Chart=Bars(Symbol(),PERIOD_H1);
   //--
   return(INIT_SUCCEEDED);       
}


//OnCalculate runs at every tick or price change received for the current chart and has a set of default input parameters
int OnCalculate(const int rates_total,
                const int prev_calculated,
                const datetime &time[],
                const double &open[],
                const double &high[],
                const double &low[],
                const double &close[],
                const long &tick_volume[],
                const long &volume[],
                const int &spread[]){
   if(Bars(Symbol(),PERIOD_CURRENT)!=BarsInChart || prev_calculated==0){

      ArraySetAsSeries(M5mrate,true);
      int M5copied=CopyRates(NULL,PERIOD_M5,0,3,M5mrate);
      
      ArraySetAsSeries(H1mrate,true);
      int H1Copied=CopyRates(NULL,PERIOD_H1,0,3,H1mrate);
      
      CleanChart();
      DrawAreas();
      SetOrange();
      DrawLevels();

      static datetime LastBar = 0;
      datetime ThisBar = (datetime)SeriesInfoInteger(_Symbol,PERIOD_M5,SERIES_LASTBAR_DATE);
      
      static datetime H1LastBar = 0;
      datetime H1ThisBar = (datetime)SeriesInfoInteger(_Symbol,PERIOD_H1,SERIES_LASTBAR_DATE);
      
      datetime CurrntTime= TimeCurrent();

      //START SENDING SIGNALS----
      if(CurrntTime>dayStartTime&&NonTradingDay==false)//If the Current time is after 17:30PM and It is not a non Trading day.
      { 
      double M5p_close=M5mrate[1].close;
      double H1p_close=H1mrate[1].close;
      
      DrawLabels();
         if(!SignalsPending)     //If Signals are not set to Pending
            { 
               if(SignalSent==false)//If No Signal Has Been Sent
               { 
      double BuySL=OrangeLowPoint-StopLoss;
      double SellSL=OrangeHighPoint+StopLoss;
      double BuyTP=OrangeLowPoint+TakeProfit;
      double SellTP=OrangeHighPoint-TakeProfit;
                  Alert(Symbol()," Buy: ",OrangeLowPoint," SL: ",BuySL," TP: ",BuyTP,
                        " || Sell: ",OrangeHighPoint," SL: ",SellSL," TP: ",SellTP);
                        
                  string OSignal=Symbol()+" "+(string)SignalDate+"\n--------------\n"+" Buy: "+(string)OrangeLowPoint+"\n SL  : "+(string)BuySL+"\n TP  : "+(string)BuyTP+"\n--------------\n Sell: "+(string)OrangeHighPoint+"\n SL  : "+(string)SellSL+"\n TP  : "+(string)SellTP;
                  SendNotification(OSignal);
                  SignalSent=true;
                  SignalsPending=true;
               }
            }
         else
         {
            Print("Signal(s) Pending. HODL...");
         }

      
      if(H1LastBar!=H1ThisBar)//On Each New H1 Bar
      {
         if(H1p_close<OrangeLowPoint&& H1BreakoutSignalSent==false)
         {
            Alert(Symbol()," ",H1ThisBar," H1 BREAKOUT!! == Short: ", H1p_close," High: ",OrangeHighPoint," Low: ",OrangeLowPoint);
            string H1Break=Symbol()+" "+(string)SignalDate+"\n--------------\n"+(string)H1ThisBar+"\n H1 BREAKOUT!! == Short: "+ (string)H1p_close+"\n High: "+(string)OrangeHighPoint+"\n Low: "+(string)OrangeLowPoint;
            SendNotification(H1Break);
            H1BreakoutSignalSent=true;
         }
         if(H1p_close>OrangeHighPoint&& H1BreakoutSignalSent==false)
         {
            Alert(Symbol()," ",H1ThisBar," H1 BREAKOUT!! == Long : ", H1p_close," High: ",OrangeHighPoint," Low: ",OrangeLowPoint);
            string H1Break=Symbol()+" "+(string)SignalDate+"\n--------------\n"+(string)H1ThisBar+"\n H1 BREAKOUT!! == Long : "+(string) H1p_close+"\n High: "+(string)OrangeHighPoint+"\n Low: "+(string)OrangeLowPoint;
            SendNotification(H1Break);
            H1BreakoutSignalSent=true;
         }
         H1LastBar = ThisBar;
      }
      
      //M5 Bar Previous Close
      if(LastBar != ThisBar)//On Each New M5 Bar
      {
         if(M5p_close<OrangeLowPoint&& BreakoutSignalSent==false)
         {
            Alert(Symbol()," ",ThisBar," M5 BREAKOUT: ", M5p_close," High: ",OrangeHighPoint," Low: ",OrangeLowPoint);
            string Break=Symbol()+" "+(string)SignalDate+"\n--------------\n"+(string)ThisBar+"\n M5 BREAKOUT!! == Short: "+(string) M5p_close+"\n High: "+(string)OrangeHighPoint+"\n Low: "+(string)OrangeLowPoint;
            SendNotification(Break);
            BreakoutSignalSent=true;
         }
         if(M5p_close>OrangeHighPoint&& BreakoutSignalSent==false)
         {
            Alert(Symbol()," ",ThisBar," M5 BREAKOUT: ", M5p_close," High: ",OrangeHighPoint," Low: ",OrangeLowPoint);
            string Break=Symbol()+" "+(string)SignalDate+"\n--------------\n"+(string)ThisBar+"\n M5 BREAKOUT!! == Short: "+(string) M5p_close+"\n High: "+(string)OrangeHighPoint+"\n Low: "+(string)OrangeLowPoint;
            SendNotification(Break);
            BreakoutSignalSent=true;
         }
         LastBar = ThisBar;
      }
      BarsInChart=Bars(Symbol(),PERIOD_CURRENT); 
      }
   }
   return(rates_total);
}
  
void OnDeinit(const int reason){
   CleanChart();        //Removes graphical objects from the chart
}  

void OnInitInitialization(){
   StartHour=(int)StringSubstr(TimeLineStart,0,2);
   EndHour=(int)StringSubstr(TimeLineEnd,0,2);
   StartMinute=(int)StringSubstr(TimeLineStart,2,2);
   EndMinute=(int)StringSubstr(TimeLineEnd,2,2);
   
   StartHour1=(int)StringSubstr(DayStart,0,2);
   EndHour1=(int)StringSubstr(DayEnd,0,2);
   StartMinute1=(int)StringSubstr(DayStart,2,2);
   EndMinute1=(int)StringSubstr(DayEnd,2,2);
   
   StartHour2=(int)StringSubstr(SignalStart,0,2);
   StartMinute2=(int)StringSubstr(SignalStart,2,2);
}

//--Function for run checks of requirements for the indicator to run
bool OnInitPreChecksPass(){
   if(StartHour<0 || StartMinute<0 || StartHour>23 || StartMinute>59){
      Print("Time Start value not valid, it has to be in the format 0000-2359");
      return false;
   }
   if(TimeLineEnd!="" && (EndHour<0 || EndMinute<0 || EndHour>23 || EndMinute>59)){
      Print("Time End value not valid, it has to be in the format 0000-2359");
      return false;
   }
   if(LineTickness<1 || LineTickness>5){
      Print("Line Thickness must be between 1 and 5");
      return false;
   }
   return true;
}


//--Function to remove graphical objects from the chart
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

//--This function is to initialize the Buffers necessary to draw the signals and store the signals
void InitialiseBuffers(){

}

//Useful ready to use function to check if the price is in a new candle, it returns true only once at the first price change received in a new candle
datetime NewCandleTime=TimeCurrent();
bool CheckIfNewCandle(){
   if(NewCandleTime==iTime(Symbol(),0,0)) return false;
   else{
      NewCandleTime=iTime(Symbol(),0,0);
      return true;
   }
}

//--
void DrawAreas(){
   int MaxBars=BarsToScan;
   if(Bars(Symbol(),PERIOD_M30)<MaxBars || MaxBars==0) MaxBars=Bars(Symbol(),PERIOD_M30);
   datetime MaxTime=iTime(Symbol(),PERIOD_M30,MaxBars-1);
   MqlDateTime StartTimeStruct;
   MqlDateTime EndTimeStruct;
   TimeToStruct(iTime(Symbol(),PERIOD_M30,0),StartTimeStruct);
   TimeToStruct(iTime(Symbol(),PERIOD_M30,0),EndTimeStruct);
   string StartTimeStructStr=(string)StartTimeStruct.year+"."+(string)StartTimeStruct.mon+"."+(string)StartTimeStruct.day+" "+(string)StartHour+":"+(string)StartMinute;
   string EndTimeStructStr=(string)EndTimeStruct.year+"."+(string)EndTimeStruct.mon+"."+(string)EndTimeStruct.day+" "+(string)EndHour+":"+(string)EndMinute;
   datetime StartTime=StringToTime(StartTimeStructStr);
   datetime EndTime=StringToTime(EndTimeStructStr);
   datetime StartTimeTmp=StringToTime(StartTimeStructStr);
   datetime EndTimeTmp=StringToTime(EndTimeStructStr);
   if(StartTimeTmp>EndTimeTmp){
      EndTimeTmp+=PeriodSeconds(PERIOD_D1);
   }
   while(StartTimeTmp>MaxTime && iClose(Symbol(),PERIOD_M30,iBarShift(Symbol(),PERIOD_M30,StartTimeTmp))>0){
      TimeToStruct(StartTimeTmp,StartTimeStruct);
      if(StartTimeStruct.day_of_week==0 && ShowSunday) DrawArea(StartTimeTmp,EndTimeTmp);
      if(StartTimeStruct.day_of_week==1 && ShowMonday) DrawArea(StartTimeTmp,EndTimeTmp);
      if(StartTimeStruct.day_of_week==2 && ShowTuesday) DrawArea(StartTimeTmp,EndTimeTmp);
      if(StartTimeStruct.day_of_week==3 && ShowWednesday) DrawArea(StartTimeTmp,EndTimeTmp);
      if(StartTimeStruct.day_of_week==4 && ShowThursday) DrawArea(StartTimeTmp,EndTimeTmp);
      if(StartTimeStruct.day_of_week==5 && ShowFriday) DrawArea(StartTimeTmp,EndTimeTmp);
      if(StartTimeStruct.day_of_week==6 && ShowSaturday) DrawArea(StartTimeTmp,EndTimeTmp);
      StartTimeTmp-=PeriodSeconds(PERIOD_D1);
      EndTimeTmp-=PeriodSeconds(PERIOD_D1);
   }
}

//--
void DrawArea(datetime Start, datetime End){
   string AreaName=IndicatorName+"-AREA-"+IntegerToString(Start);
   int StartBar=iBarShift(Symbol(),PERIOD_M30,Start);
   int EndBar=iBarShift(Symbol(),PERIOD_M30,End);
   int BarsCount=StartBar-EndBar;
   double HighPoint=iHigh(Symbol(),PERIOD_M30,iHighest(Symbol(),PERIOD_M30,MODE_HIGH,BarsCount,EndBar));
   double LowPoint=iLow(Symbol(),PERIOD_M30,iLowest(Symbol(),PERIOD_M30,MODE_LOW,BarsCount,EndBar));
   OrangeHighPoint=iHigh(Symbol(),PERIOD_M30,iHighest(Symbol(),PERIOD_M30,MODE_HIGH,BarsCount,EndBar));
   OrangeLowPoint=iLow(Symbol(),PERIOD_M30,iLowest(Symbol(),PERIOD_M30,MODE_LOW,BarsCount,EndBar));

}

//--
void GetOrange(datetime Start, datetime End){
   string AreaName=IndicatorName+"L-AREA-"+IntegerToString(Start);
   int StartBar=iBarShift(Symbol(),PERIOD_M30,Start);
   int EndBar=iBarShift(Symbol(),PERIOD_M30,End);
   int BarsCount=StartBar-EndBar;
   OrangeHighPoint=iHigh(Symbol(),PERIOD_M30,iHighest(Symbol(),PERIOD_M30,MODE_HIGH,BarsCount,EndBar));
   OrangeLowPoint=iLow(Symbol(),PERIOD_M30,iLowest(Symbol(),PERIOD_M30,MODE_LOW,BarsCount,EndBar)); 
}

//--
void SetOrange(){
   int MaxBars=OrangeBarsToScan;
   if(Bars(Symbol(),PERIOD_M30)<MaxBars || MaxBars==0) MaxBars=Bars(Symbol(),PERIOD_M30);
   datetime MaxTime=iTime(Symbol(),PERIOD_M30,MaxBars-1);
   MqlDateTime StartTimeStruct;
   MqlDateTime EndTimeStruct;
   TimeToStruct(iTime(Symbol(),PERIOD_M30,0),StartTimeStruct);
   TimeToStruct(iTime(Symbol(),PERIOD_M30,0),EndTimeStruct);
   string StartTimeStructStr=(string)StartTimeStruct.year+"."+(string)StartTimeStruct.mon+"."+(string)StartTimeStruct.day+" "+(string)StartHour+":"+(string)StartMinute;
   string EndTimeStructStr=(string)EndTimeStruct.year+"."+(string)EndTimeStruct.mon+"."+(string)EndTimeStruct.day+" "+(string)EndHour+":"+(string)EndMinute;
   datetime StartTime=StringToTime(StartTimeStructStr);
   datetime EndTime=StringToTime(EndTimeStructStr);
   datetime StartTimeTmp=StringToTime(StartTimeStructStr);
   SetEndTimeTmp=StringToTime(EndTimeStructStr);
   
   if(StartTimeTmp>SetEndTimeTmp){
      SetEndTimeTmp+=PeriodSeconds(PERIOD_D1);
   }
   while(StartTimeTmp>MaxTime && iClose(Symbol(),PERIOD_M30,iBarShift(Symbol(),PERIOD_M30,StartTimeTmp))>0){
      TimeToStruct(StartTimeTmp,StartTimeStruct);
      if(StartTimeStruct.day_of_week==0 && ShowSunday) GetOrange(StartTimeTmp,SetEndTimeTmp);
      if(StartTimeStruct.day_of_week==1 && ShowMonday) GetOrange(StartTimeTmp,SetEndTimeTmp);
      if(StartTimeStruct.day_of_week==2 && ShowTuesday) GetOrange(StartTimeTmp,SetEndTimeTmp);
      if(StartTimeStruct.day_of_week==3 && ShowWednesday) GetOrange(StartTimeTmp,SetEndTimeTmp);
      if(StartTimeStruct.day_of_week==4 && ShowThursday) GetOrange(StartTimeTmp,SetEndTimeTmp);
      if(StartTimeStruct.day_of_week==5 && ShowFriday) GetOrange(StartTimeTmp,SetEndTimeTmp);
      if(StartTimeStruct.day_of_week==6 && ShowSaturday) GetOrange(StartTimeTmp,SetEndTimeTmp);
      StartTimeTmp-=PeriodSeconds(PERIOD_D1);
      SetEndTimeTmp-=PeriodSeconds(PERIOD_D1);
   }
}
//--
void LabelCreate(string name, int x, int y, string text, color clr, int font_size){
 
 int sub_window=0;
 int height=50;
 int width=100;
 int corner=CORNER_RIGHT_UPPER;
 string font="Arial";
 color back_clr=clrBeige;
 color border_clr=clrBlack;
 bool back=false;
 bool state=true;
 bool selection=false;
 bool hidden=true;
 int z_order=0;
 double angle=0.0;
 ENUM_ANCHOR_POINT anchor=ANCHOR_LEFT_UPPER;
//--- reset the error value 
   ResetLastError(); 
//--- create a text label 
 if(!ObjectCreate(chart_ID,name,OBJ_LABEL,sub_window,0,0)) 
 { 
  Print(__FUNCTION__, ": failed to create spread label! Error code = ",GetLastError()); 
  return; 
 } 
//--- set label coordinates 
   ObjectSetInteger(chart_ID,name,OBJPROP_XDISTANCE,x); 
   ObjectSetInteger(chart_ID,name,OBJPROP_YDISTANCE,y); 
//--- set the chart's corner, relative to which point coordinates are defined 
   ObjectSetInteger(chart_ID,name,OBJPROP_CORNER,corner); 
//--- set the text 
   ObjectSetString(chart_ID,name,OBJPROP_TEXT,text); 
//--- set text font 
   ObjectSetString(chart_ID,name,OBJPROP_FONT,font); 
//--- set font size 
   ObjectSetInteger(chart_ID,name,OBJPROP_FONTSIZE,font_size); 
//--- set the slope angle of the text 
   ObjectSetDouble(chart_ID,name,OBJPROP_ANGLE,angle); 
//--- set anchor type 
   ObjectSetInteger(chart_ID,name,OBJPROP_ANCHOR,anchor); 
//--- set color 
   ObjectSetInteger(chart_ID,name,OBJPROP_COLOR,clr); 
//--- display in the foreground (false) or background (true) 
   ObjectSetInteger(chart_ID,name,OBJPROP_BACK,back); 
//--- enable (true) or disable (false) the mode of moving the label by mouse 
   ObjectSetInteger(chart_ID,name,OBJPROP_SELECTABLE,selection); 
   ObjectSetInteger(chart_ID,name,OBJPROP_SELECTED,selection); 
//--- hide (true) or display (false) graphical object name in the object list 
   ObjectSetInteger(chart_ID,name,OBJPROP_HIDDEN,hidden); 
//--- set the priority for receiving the event of a mouse click in the chart 
   ObjectSetInteger(chart_ID,name,OBJPROP_ZORDER,z_order); 
//--- successful execution 
} 

//--
void DrawLabels(){
      
      double balance=NormalizeDouble(AccountInfoDouble(ACCOUNT_BALANCE),2);
      double Risk=balance*0.10; //10%
      double Points = (OrangeHighPoint-OrangeLowPoint)*100;//Range Points
      double Lot = (Risk/Points)*100;//Lot Size for Breakout
      double totalRisk=(Lot*Points)/100;//Risk in Money
      double RLot=Risk/StopLoss; //Range Trading Lot Size
      double Rrisk=(RLot*(StopLoss*100))/100;
      
      double BuySL=OrangeLowPoint-StopLoss;
      double SellSL=OrangeHighPoint+StopLoss;
      double BuyTP=OrangeLowPoint+TakeProfit;
      double SellTP=OrangeHighPoint-TakeProfit;
      
      int x=115, y=1, space=15,FntSze=8;
      LabelCreate(IndicatorName+"Header",x,y+(space),"Next Signal: "+TimeToString(dayStartTime),LabelColor,FntSze);
      LabelCreate(IndicatorName+"balance",x,y+(space*2),"$"+(string)(balance),LabelColor,FntSze);
      LabelCreate(IndicatorName+"10% Risk",x,y+(space*3),"$"+(string)(Risk),LabelColor,FntSze);
      
      LabelCreate(IndicatorName+"comm1",x,y+(space)*4,"---------",LabelColor,FntSze);
      LabelCreate(IndicatorName+"OrangeH",x,y+(space)*5,"High: "+(string)(OrangeHighPoint),LabelColor,FntSze);
      LabelCreate(IndicatorName+"OrangeL",x,y+(space)*6,"Low:  "+(string)(OrangeLowPoint),LabelColor,FntSze);
      LabelCreate(IndicatorName+"Points",x,y+(space*7),"Range: "+(string)(Points),LabelColor,FntSze);
      LabelCreate(IndicatorName+"OrangeLot",x,y+(space*8),"Lot: "+(string)(Lot),LabelColor,FntSze);
      LabelCreate(IndicatorName+"OrangetotalRisk",x,y+(space*9),"$"+(string)(totalRisk),LabelColor,FntSze);
      if(ShowRangeTargets){
      LabelCreate(IndicatorName+"comm2",x,y+(space)*10,"---------",LabelColor,FntSze);
      LabelCreate(IndicatorName+"Buy SL",x,y+(space*11),"Buy SL: "+(string)(BuySL),LabelColor,FntSze);
      LabelCreate(IndicatorName+"Buy TP",x,y+(space*12),"Buy TP: "+(string)(BuyTP),LabelColor,FntSze);
      LabelCreate(IndicatorName+"Sell SL",x,y+(space*13),"Sell SL: "+(string)(SellSL),LabelColor,FntSze);
      LabelCreate(IndicatorName+"Sell TP",x,y+(space*14),"Sell TP: "+(string)(SellTP),LabelColor,FntSze);  
      LabelCreate(IndicatorName+"Lot",x,y+(space*15),"Lot: "+(string)(RLot),LabelColor,FntSze);
      LabelCreate(IndicatorName+"Risk",x,y+(space*16),"$"+(string)(Rrisk),LabelColor,FntSze);
      }       
}

//--
void DrawLevels(){
   int MaxBars=BarsToScan;
   if(Bars(Symbol(),PERIOD_M30)<MaxBars || MaxBars==0) MaxBars=Bars(Symbol(),PERIOD_M30);
   datetime MaxTime=iTime(Symbol(),PERIOD_M30,MaxBars-1);
   MqlDateTime StartTimeStruct;
   MqlDateTime EndTimeStruct;
   MqlDateTime LineEndTimeStruct;
   TimeToStruct(iTime(Symbol(),PERIOD_M30,0),StartTimeStruct);
   TimeToStruct(iTime(Symbol(),PERIOD_M30,0),EndTimeStruct);
   TimeToStruct(iTime(Symbol(),PERIOD_M30,0),LineEndTimeStruct);
   string StartTimeStructStr=(string)StartTimeStruct.year+"."+(string)StartTimeStruct.mon+"."+(string)StartTimeStruct.day+" "+(string)StartHour+":"+(string)StartMinute;
   string EndTimeStructStr=(string)EndTimeStruct.year+"."+(string)EndTimeStruct.mon+"."+(string)EndTimeStruct.day+" "+(string)EndHour+":"+(string)EndMinute;
   string LineEndTimeStructStr=(string)EndTimeStruct.year+"."+(string)EndTimeStruct.mon+"."+(string)EndTimeStruct.day+" "+(string)EndHour1+":"+(string)EndMinute1;
   datetime StartTime=StringToTime(StartTimeStructStr);
   datetime EndTime=StringToTime(EndTimeStructStr);
   datetime LineEndTime=StringToTime(LineEndTimeStructStr);
   datetime StartTimeTmp=StringToTime(StartTimeStructStr);
   datetime EndTimeTmp=StringToTime(EndTimeStructStr);
   datetime LineEndTimeTmp=StringToTime(LineEndTimeStructStr);
   if(StartTimeTmp>EndTimeTmp){
      EndTimeTmp+=PeriodSeconds(PERIOD_D1);
      LineEndTimeTmp+=PeriodSeconds(PERIOD_D1);
   }
   //Print(iClose(Symbol(),PERIOD_CURRENT,iBarShift(Symbol(),PERIOD_CURRENT,StartTimeTmp))," ",StartTimeTmp);
   while(StartTimeTmp>MaxTime && iClose(Symbol(),PERIOD_M30,iBarShift(Symbol(),PERIOD_M30,StartTimeTmp))>0){//While Start Time is Greater than the Time of the last bar to scan and we have more than Zero closes.
      TimeToStruct(StartTimeTmp,StartTimeStruct);
      if(StartTimeStruct.day_of_week==0 && ShowSunday) DrawLevel(StartTimeTmp,EndTimeTmp,LineEndTimeTmp);
      if(StartTimeStruct.day_of_week==1 && ShowMonday) DrawLevel(StartTimeTmp,EndTimeTmp,LineEndTimeTmp);
      if(StartTimeStruct.day_of_week==2 && ShowTuesday) DrawLevel(StartTimeTmp,EndTimeTmp,LineEndTimeTmp);
      if(StartTimeStruct.day_of_week==3 && ShowWednesday) DrawLevel(StartTimeTmp,EndTimeTmp,LineEndTimeTmp);
      if(StartTimeStruct.day_of_week==4 && ShowThursday) DrawLevel(StartTimeTmp,EndTimeTmp,LineEndTimeTmp);
      if(StartTimeStruct.day_of_week==5 && ShowFriday) DrawLevel(StartTimeTmp,EndTimeTmp,LineEndTimeTmp);
      if(StartTimeStruct.day_of_week==6 && ShowSaturday) DrawLevel(StartTimeTmp,EndTimeTmp,LineEndTimeTmp);
      StartTimeTmp-=PeriodSeconds(PERIOD_D1);
      EndTimeTmp-=PeriodSeconds(PERIOD_D1);
      LineEndTimeTmp-=PeriodSeconds(PERIOD_D1);
   }
}

//--
void DrawLevel(datetime Start, datetime End,datetime LineEnd){
   string AreaName =IndicatorName+"-Level-"+IntegerToString(Start);
   string AreaName1=IndicatorName+"-Level1-"+IntegerToString(Start);
   string AreaName2=IndicatorName+"-Level2-"+IntegerToString(Start);
   string AreaName3=IndicatorName+"-Level3-"+IntegerToString(Start);
   string AreaName4=IndicatorName+"-Level4-"+IntegerToString(Start);
   string AreaName5=IndicatorName+"-Level5-"+IntegerToString(Start);
   
   int StartBar=iBarShift(Symbol(),PERIOD_M30,Start);
   int EndBar=iBarShift(Symbol(),PERIOD_M30,End);
   int LineEndBar=iBarShift(Symbol(),PERIOD_M30,LineEnd);
   
   int BarsCount=StartBar-EndBar;
   double HighPoint=iHigh(Symbol(),PERIOD_M30,iHighest(Symbol(),PERIOD_M30,MODE_HIGH,BarsCount,EndBar));
   double LowPoint=iLow(Symbol(),PERIOD_M30,iLowest(Symbol(),PERIOD_M30,MODE_LOW,BarsCount,EndBar));
   
   double BuySL=LowPoint-StopLoss;
   double SellSL=HighPoint+StopLoss;
   double BuyTP=LowPoint+TakeProfit;
   double SellTP=HighPoint-TakeProfit;
   
   ObjectCreate(0,AreaName,OBJ_TREND,0,Start,HighPoint,LineEnd,HighPoint);
   ObjectSetInteger(0,AreaName,OBJPROP_COLOR,LineColor2);
   ObjectSetInteger(0,AreaName,OBJPROP_BACK,true);
   ObjectSetInteger(0,AreaName,OBJPROP_STYLE,STYLE_DOT);
   ObjectSetInteger(0,AreaName,OBJPROP_HIDDEN,true);
   ObjectSetInteger(0,AreaName,OBJPROP_FILL,false);
   ObjectSetInteger(0,AreaName,OBJPROP_SELECTED,false);
   ObjectSetInteger(0,AreaName,OBJPROP_WIDTH,LineTickness);
   
   ObjectCreate(0,AreaName1,OBJ_TREND,0,Start,LowPoint,LineEnd,LowPoint);
   ObjectSetInteger(0,AreaName1,OBJPROP_COLOR,LineColor1);
   ObjectSetInteger(0,AreaName1,OBJPROP_BACK,true);
   ObjectSetInteger(0,AreaName1,OBJPROP_STYLE,STYLE_DOT);
   ObjectSetInteger(0,AreaName1,OBJPROP_HIDDEN,true);
   ObjectSetInteger(0,AreaName1,OBJPROP_FILL,false);
   ObjectSetInteger(0,AreaName1,OBJPROP_SELECTED,false);
   ObjectSetInteger(0,AreaName1,OBJPROP_WIDTH,LineTickness);
   
   if(ShowRangeTargets){
   ObjectCreate(0,AreaName2,OBJ_TREND,0,Start,BuySL,LineEnd,BuySL);
   ObjectSetInteger(0,AreaName2,OBJPROP_COLOR,LineColor1);
   ObjectSetInteger(0,AreaName2,OBJPROP_BACK,true);
   ObjectSetInteger(0,AreaName2,OBJPROP_STYLE,STYLE_DOT);
   ObjectSetInteger(0,AreaName2,OBJPROP_HIDDEN,true);
   ObjectSetInteger(0,AreaName2,OBJPROP_FILL,false);
   ObjectSetInteger(0,AreaName2,OBJPROP_SELECTED,false);
   ObjectSetInteger(0,AreaName2,OBJPROP_WIDTH,LineTickness);
   
   ObjectCreate(0,AreaName3,OBJ_TREND,0,Start,SellSL,LineEnd,SellSL);
   ObjectSetInteger(0,AreaName3,OBJPROP_COLOR,LineColor2);
   ObjectSetInteger(0,AreaName3,OBJPROP_BACK,true);
   ObjectSetInteger(0,AreaName3,OBJPROP_STYLE,STYLE_DOT);
   ObjectSetInteger(0,AreaName3,OBJPROP_HIDDEN,true);
   ObjectSetInteger(0,AreaName3,OBJPROP_FILL,false);
   ObjectSetInteger(0,AreaName3,OBJPROP_SELECTED,false);
   ObjectSetInteger(0,AreaName3,OBJPROP_WIDTH,LineTickness);
   
   ObjectCreate(0,AreaName4,OBJ_TREND,0,Start,BuyTP,LineEnd,BuyTP);
   ObjectSetInteger(0,AreaName4,OBJPROP_COLOR,LineColor1);
   ObjectSetInteger(0,AreaName4,OBJPROP_BACK,true);
   ObjectSetInteger(0,AreaName4,OBJPROP_STYLE,STYLE_DOT);
   ObjectSetInteger(0,AreaName4,OBJPROP_HIDDEN,true);
   ObjectSetInteger(0,AreaName4,OBJPROP_FILL,false);
   ObjectSetInteger(0,AreaName4,OBJPROP_SELECTED,false);
   ObjectSetInteger(0,AreaName4,OBJPROP_WIDTH,LineTickness);
   
   ObjectCreate(0,AreaName5,OBJ_TREND,0,Start,SellTP,LineEnd,SellTP);
   ObjectSetInteger(0,AreaName5,OBJPROP_COLOR,LineColor2);
   ObjectSetInteger(0,AreaName5,OBJPROP_BACK,true);
   ObjectSetInteger(0,AreaName5,OBJPROP_STYLE,STYLE_DOT);
   ObjectSetInteger(0,AreaName5,OBJPROP_HIDDEN,true);
   ObjectSetInteger(0,AreaName5,OBJPROP_FILL,false);
   ObjectSetInteger(0,AreaName5,OBJPROP_SELECTED,false);
   ObjectSetInteger(0,AreaName5,OBJPROP_WIDTH,LineTickness);
   }
}

//+------------------------------------------------------------------+
//|                                           RangeAndBreakoutEA.mq5 |
//|                                  Copyright 2022, MetaQuotes Ltd. |
//|                                      https://github.com/sabeloxn |
//+------------------------------------------------------------------+
#include <Telegram.mqh>

//--- input parameters
input string InpChannelName="";//Channel Name
input string InpToken="";//Token

//--- global variables
CCustomBot bot;
input int EAMagic = 12345;
input double StopLoss =100;                          //Stop Loss Points
input double TakeProfit=100;                         //Take Profit Points
input string RStartTime="0000";                    //Start Time To Get Entry(s) (Format 24H HHMM)
input string REndTime="2359";                      //End Time To Get Entry(s) && Place/Execute Order(s) (Optional - Format HHMM)
input string SignalStart="0000";                      //Start Time To Send Signal (Format 24H HHMM)
input string DayEndTime="2359";                    //Day End Time (Optional - Format HHMM)
input int    BarsToScan=100;                         //Bars to Scan for Price Range
bool ShowMonday=true;                           //Show If Monday
bool ShowTuesday=true;                          //Show If Tuesday
bool ShowWednesday=true;                        //Show If Wednesday
bool ShowThursday=true;                         //Show If Thursday
bool ShowFriday=true;                           //Show If Friday
input bool ShowSaturday=false;                        //Show If Saturday
input bool ShowSunday=false;                          //Show If Sunday
int StartHour=0,StartHour1=0;;
int StartMinute=0,StartMinute1=0;;
int EndHour=0,EndHour1=0;
int EndMinute=0,EndMinute1=0;
int StartHour2=0;
int StartMinute2=0;
int BarsInChart=0;
int BarsInM30Chart=0;
int BarsInH1Chart=0;
double OrangeHighPoint, OrangeLowPoint;
datetime SetEndTimeTmp,TodayEndTimeTmp,dayStartTime,ScanEndTime,OrderExpTime;
string SignalDate;
bool NewDay =false,H1NewDay=false;
MqlRates M5mrate[];
MqlRates H1mrate[];
double BuySL,SellSL,BuyTP,SellTP;
double BreakoutBuySL,BreakoutSellSL,BreakoutBuyTP,BreakoutSellTP;
bool SignalSent,BreakoutSignalSent,SignalsPending,H1BreakoutSignalSent,NonTradingDay;
bool checked;
int maxVolume=100;
//+------------------------------------------------------------------+
//| Expert initialization function                                   |
//+------------------------------------------------------------------+
int OnInit()
  {
//---
   bot.Token(InpToken);
   OnInitInitialization();
   NonTradingDay=false;
   MqlDateTime TodayStartTimeStruct;
   TimeToStruct(TimeCurrent(),TodayStartTimeStruct);
   string TodayStartTimeStructStr=(string)TodayStartTimeStruct.year+"."+(string)TodayStartTimeStruct.mon+"."+(string)TodayStartTimeStruct.day+" "+(string)StartHour2+":"+(string)StartMinute2;
   dayStartTime=StringToTime(TodayStartTimeStructStr);
   if(TodayStartTimeStruct.day_of_week==6) //If it's a Saturday
     {
      NonTradingDay=true;
     }
   if(TodayStartTimeStruct.day_of_week==0)  //If it's a Sunday
     {
      NonTradingDay=true;
     }
   BarsInChart=Bars(Symbol(),PERIOD_M5);
   BarsInH1Chart=Bars(Symbol(),PERIOD_H1);
   BarsInM30Chart=Bars(Symbol(),PERIOD_M30);

//--- make sure that the account is demo
   if(AccountInfoInteger(ACCOUNT_TRADE_MODE)==ACCOUNT_TRADE_MODE_REAL)
     {
      Alert("Script operation is on a live account!");
      //return;
     }
//--- place or delete order
   if(GetOrdersTotalByMagic(EAMagic)>0)
     {
      //   //--- no current orders - place an order
      //   uint res=SendBuyPendingOrder(EAMagic);
      //   Print("Return code of the trade server ",res);
      //  }
      //else // there are orders - delete orders
      //  {
      DeleteAllOrdersByMagic(EAMagic);
     }
//---
   return(INIT_SUCCEEDED);
  }
//+------------------------------------------------------------------+
//| Expert deinitialization function                                 |
//+------------------------------------------------------------------+
void OnDeinit(const int reason)
  {
//---
   if(reason==REASON_PARAMETERS ||
      reason==REASON_RECOMPILE ||
      reason==REASON_ACCOUNT)
     {
      checked=false;
     }
   DeleteAllOrdersByMagic(EAMagic);
  }
//+------------------------------------------------------------------+
//| Expert tick function                                             |
//+------------------------------------------------------------------+
void OnTick()
  {
//---
   BreakoutBuySL=OrangeLowPoint;
   BreakoutSellSL=OrangeHighPoint;
      
   if(!checked)
     {
      if(StringLen(InpChannelName)==0)
        {
         Print("Error: Channel name is empty");
         Sleep(10000);
         return;
        }

      int result=bot.GetMe();
      if(result==0)
        {
         Print("Bot name: ",bot.Name());
         checked=true;
        }
      else
        {
         Print("Error: ",GetErrorDescription(result));
         Sleep(10000);
         return;
        }
     }

   SetOrange();
   ArraySetAsSeries(M5mrate,true);
   int M5copied=CopyRates(NULL,PERIOD_M5,0,3,M5mrate);
   ArraySetAsSeries(H1mrate,true);
   int H1Copied=CopyRates(NULL,PERIOD_H1,0,3,H1mrate);

   double M5p_close=M5mrate[0].close;

   double H1p_close=H1mrate[0].close;

   static datetime LastBar = 0;
   datetime ThisBar = (datetime)SeriesInfoInteger(_Symbol,PERIOD_M5,SERIES_LASTBAR_DATE);

   static datetime H1LastBar = 0;
   datetime H1ThisBar = (datetime)SeriesInfoInteger(_Symbol,PERIOD_H1,SERIES_LASTBAR_DATE);

   datetime TimeCurr=TimeCurrent();
   static datetime today;
   if(today != iTime(Symbol(), PERIOD_D1, 0))
     {
      today = iTime(Symbol(), PERIOD_D1, 0);
      NewDay=true;
      H1NewDay=true;
     }

//Trading Start
   if(TimeCurr>=dayStartTime&&NonTradingDay==false)
     {
         if(NewDay==true){
      BuySL=OrangeLowPoint-StopLoss;
      SellSL=OrangeHighPoint+StopLoss;
      BuyTP=OrangeLowPoint+TakeProfit;
      SellTP=OrangeHighPoint-TakeProfit;
      BreakoutBuySL=OrangeLowPoint;
      BreakoutSellSL=OrangeHighPoint;
      //Place range orders.
      SendBuyPendingOrder(EAMagic);
      SendSellPendingOrder(EAMagic);
      //Send Signal
      sendRangeSignal();
        NewDay=false;
         }
         if(H1NewDay==true)
         {
      //Place Breakout orders.
      if(H1LastBar!=H1ThisBar)//On Each New H1 Bar
        {
         if(H1p_close<OrangeLowPoint&& H1BreakoutSignalSent==false)
           {
            SendBreakoutSellPendingOrder(EAMagic);
            sendSellBreakoutSignal();
            H1BreakoutSignalSent=true;
            H1NewDay=false;
           }
         if(H1p_close>OrangeHighPoint&& H1BreakoutSignalSent==false)
           {
            SendBreakoutBuyPendingOrder(EAMagic);
            sendBuyBreakoutSignal();
            H1BreakoutSignalSent=true;
            H1NewDay=false;
           }
         H1LastBar = ThisBar;
         
        }
 
        }
     }
   else
     {
     }
  }
//+------------------------------------------------------------------+
void OnInitInitialization()
  {
   StartHour=(int)StringSubstr(RStartTime,0,2);
   EndHour=(int)StringSubstr(REndTime,0,2);
   StartMinute=(int)StringSubstr(RStartTime,2,2);
   EndMinute=(int)StringSubstr(REndTime,2,2);

   EndHour1=(int)StringSubstr(DayEndTime,0,2);
   EndMinute1=(int)StringSubstr(DayEndTime,2,2);

   StartHour2=(int)StringSubstr(SignalStart,0,2);
   StartMinute2=(int)StringSubstr(SignalStart,2,2);
  }
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
void GetRangeHnL(datetime Start, datetime End)
  {
   int StartBar=iBarShift(Symbol(),PERIOD_M30,Start);
   int EndBar=iBarShift(Symbol(),PERIOD_M30,End);
   int BarsCount=StartBar-EndBar;
   double HighPoint=iHigh(Symbol(),PERIOD_M30,iHighest(Symbol(),PERIOD_M30,MODE_HIGH,BarsCount,EndBar));
   double LowPoint =iLow(Symbol(),PERIOD_M30,iLowest(Symbol(),PERIOD_M30,MODE_LOW,BarsCount,EndBar));
   OrangeHighPoint=iHigh(Symbol(),PERIOD_M30,iHighest(Symbol(),PERIOD_M30,MODE_HIGH,BarsCount,EndBar));
   OrangeLowPoint=iLow(Symbol(),PERIOD_M30,iLowest(Symbol(),PERIOD_M30,MODE_LOW,BarsCount,EndBar));
  }
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
void SetTimes()
  {
//Day||Range Start Time:
   MqlDateTime TodayStartTimeStruct;
   TimeToStruct(TimeCurrent(),TodayStartTimeStruct);
   string TodayStartTimeStructStr=(string)TodayStartTimeStruct.year+"."+(string)TodayStartTimeStruct.mon+"."+(string)TodayStartTimeStruct.day+" "+(string)StartHour+":"+(string)StartMinute;
   dayStartTime=StringToTime(TodayStartTimeStructStr);
//Day||Range End Time:
   MqlDateTime EndTimeStruct;
   TimeToStruct(TimeCurrent(),EndTimeStruct);
   string EndTimeStructStr=(string)EndTimeStruct.year+"."+(string)EndTimeStruct.mon+"."+(string)EndTimeStruct.day+" "+(string)EndHour+":"+(string)EndMinute;
   ScanEndTime=StringToTime(EndTimeStructStr);
//Date for Signal Message.
   SignalDate=(string)TodayStartTimeStruct.year+"."+(string)TodayStartTimeStruct.mon+"."+(string)TodayStartTimeStruct.day;
//Day End Time
   MqlDateTime DayEndTimeStruct;
   TimeToStruct(TimeCurrent(),DayEndTimeStruct);
   string DayEndTimeStructStr=(string)EndTimeStruct.year+"."+(string)EndTimeStruct.mon+"."+(string)EndTimeStruct.day+" "+(string)EndHour1+":"+(string)EndMinute1;
   OrderExpTime=StringToTime(DayEndTimeStructStr);
  }

//+------------------------------------------------------------------+
void SetOrange()
  {
   int MaxBars=BarsToScan;
   if(Bars(Symbol(),PERIOD_M30)<MaxBars || MaxBars==0)
      MaxBars=Bars(Symbol(),PERIOD_M30);
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

   if(StartTimeTmp>SetEndTimeTmp)
     {
      SetEndTimeTmp+=PeriodSeconds(PERIOD_D1);
     }
//Print(iClose(Symbol(),PERIOD_CURRENT,iBarShift(Symbol(),PERIOD_CURRENT,StartTimeTmp))," ",StartTimeTmp);
   while(StartTimeTmp>MaxTime && iClose(Symbol(),PERIOD_M30,iBarShift(Symbol(),PERIOD_M30,StartTimeTmp))>0)
     {
      TimeToStruct(StartTimeTmp,StartTimeStruct);
      if(StartTimeStruct.day_of_week==0 && ShowSunday)
         GetRangeHnL(StartTimeTmp,SetEndTimeTmp);
      if(StartTimeStruct.day_of_week==1 && ShowMonday)
         GetRangeHnL(StartTimeTmp,SetEndTimeTmp);
      if(StartTimeStruct.day_of_week==2 && ShowTuesday)
         GetRangeHnL(StartTimeTmp,SetEndTimeTmp);
      if(StartTimeStruct.day_of_week==3 && ShowWednesday)
         GetRangeHnL(StartTimeTmp,SetEndTimeTmp);
      if(StartTimeStruct.day_of_week==4 && ShowThursday)
         GetRangeHnL(StartTimeTmp,SetEndTimeTmp);
      if(StartTimeStruct.day_of_week==5 && ShowFriday)
         GetRangeHnL(StartTimeTmp,SetEndTimeTmp);
      if(StartTimeStruct.day_of_week==6 && ShowSaturday)
         GetRangeHnL(StartTimeTmp,SetEndTimeTmp);
      StartTimeTmp-=PeriodSeconds(PERIOD_D1);
      SetEndTimeTmp-=PeriodSeconds(PERIOD_D1);
     }
  }
//+------------------------------------------------------------------+
//| Receives the current number of orders with specified ORDER_MAGIC |
//+------------------------------------------------------------------+
int GetOrdersTotalByMagic(long const magic_number)
  {
   ulong order_ticket;
   int total=0;
//--- go through all pending orders
   for(int i=0; i<OrdersTotal(); i++)
      if((order_ticket=OrderGetTicket(i))>0)
         if(EAMagic==OrderGetInteger(ORDER_MAGIC))
            total++;
//---
   return(total);
  }
//+------------------------------------------------------------------+
//| Deletes all pending orders with specified ORDER_MAGIC            |
//+------------------------------------------------------------------+
void DeleteAllOrdersByMagic(long const magic_number)
  {
   ulong order_ticket;
//--- go through all pending orders
   for(int i=OrdersTotal()-1; i>=0; i--)
      if((order_ticket=OrderGetTicket(i))>0)
         //--- order with appropriate ORDER_MAGIC
         if(EAMagic==OrderGetInteger(ORDER_MAGIC))
           {
            MqlTradeResult result= {};
            MqlTradeRequest request= {};
            request.order=order_ticket;
            request.action=TRADE_ACTION_REMOVE;
            OrderSend(request,result);
            //--- write the server reply to log
            Print(__FUNCTION__,": ",result.comment," reply code ",result.retcode);
           }
//---
  }
//+------------------------------------------------------------------+
//| Sets a pending order                                             |
//+------------------------------------------------------------------+
uint SendBuyPendingOrder(long const magic_number)
  {
//--- prepare a request
   double balance=NormalizeDouble(AccountInfoDouble(ACCOUNT_BALANCE),2);
   double Risk=balance*0.10; //10%
   double RLot=Risk/StopLoss; //30 Points
   MqlTradeRequest request= {};
   request.action=TRADE_ACTION_PENDING;         // setting a pending order
   request.magic=EAMagic;                  // ORDER_MAGIC
   request.symbol=_Symbol;                      // symbol
   request.volume=20.00;                          // volume in 0.1 lots
   request.sl=BuySL;                                // Stop Loss is not specified
   request.tp=BuyTP;                                // Take Profit is not specified
//--- form the order type
   request.type=ORDER_TYPE_BUY_LIMIT;                // order type
//--- form the price for the pending order
   request.price=OrangeLowPoint;  // low price
   request.expiration=OrderExpTime; //Expiration Time
   request.deviation=0.05; //Maximum points away from my Price to Open position(5 points)
//--- send a trade request
   MqlTradeResult result= {};
   OrderSend(request,result);
//--- write the server reply to log
   Print(__FUNCTION__,":",result.comment);
   if(result.retcode==10016)
      Print(result.bid,result.ask,result.price);
//--- return code of the trade server reply
   return result.retcode;
  }

//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
uint SendSellPendingOrder(long const magic_number)
  {
//--- prepare a request
   double balance=NormalizeDouble(AccountInfoDouble(ACCOUNT_BALANCE),2);
   double Risk=balance*0.10; //10%
   double RLot=Risk/StopLoss; //30 Points
   MqlTradeRequest request= {};
   request.action=TRADE_ACTION_PENDING;         // setting a pending order
   request.magic=EAMagic;                  // ORDER_MAGIC
   request.symbol=_Symbol;                      // symbol
   request.volume=20.00;                          // volume in 0.1 lots
   request.sl=SellSL;                                // Stop Loss is not specified
   request.tp=SellTP;                                // Take Profit is not specified
//--- form the order type
   request.type=ORDER_TYPE_SELL_LIMIT;                // order type
//--- form the price for the pending order
   request.price=OrangeHighPoint;  // low price
   request.expiration=OrderExpTime; //Expiration Time
   request.deviation=0.05; //Maximum points away from my Price to Open position(5 points)
//--- send a trade request
   MqlTradeResult result= {};
   OrderSend(request,result);
//--- write the server reply to log
   Print(__FUNCTION__,":",result.comment);
   if(result.retcode==10016)
      Print(result.bid,result.ask,result.price);
//--- return code of the trade server reply
   return result.retcode;
  }
  
//BREAKOUTS
uint SendBreakoutBuyPendingOrder(long const magic_number)
  {
//--- prepare a request
   double balance=NormalizeDouble(AccountInfoDouble(ACCOUNT_BALANCE),2);
   double Risk=balance*0.10; //10%
   double Points = (OrangeHighPoint-OrangeLowPoint)*100;//Range Points
   double Lot = (Risk/Points)*100;//Lot Size for Breakout
   double totalRisk=(Lot*Points)/100;//Risk in Money
   double RLot=Risk/StopLoss; //30 Points

   MqlTradeRequest request= {};
   request.action=TRADE_ACTION_PENDING;         // setting a pending order
   request.magic=EAMagic;                  // ORDER_MAGIC
   request.symbol=_Symbol;                      // symbol
   request.volume=Lot;                          // volume in 0.1 lots
   request.sl=BreakoutBuySL;                                // Stop Loss is not specified
   request.tp=BreakoutBuyTP;                                // Take Profit is not specified
//--- form the order type
   request.type=ORDER_TYPE_BUY_LIMIT;                // order type
//--- form the price for the pending order
   request.price=OrangeHighPoint;  // low price
   request.expiration=OrderExpTime; //Order expiration time.
   request.deviation=0.05; //Maximum points away from my Price to Open position(5 points)
//--- send a trade request
   MqlTradeResult result= {};
   OrderSend(request,result);
//--- write the server reply to log
   Print(__FUNCTION__,":",result.comment);
   if(result.retcode==10016)
      Print(result.bid,result.ask,result.price);
//--- return code of the trade server reply
   return result.retcode;
  }

//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
uint SendBreakoutSellPendingOrder(long const magic_number)
  {
//--- prepare a request
   double balance=NormalizeDouble(AccountInfoDouble(ACCOUNT_BALANCE),2);
   double Risk=balance*0.10; //10%
   double Points = (OrangeHighPoint-OrangeLowPoint)*100;//Range Points
   double Lot = (Risk/Points)*100;//Lot Size for Breakout
   double totalRisk=(Lot*Points)/100;//Risk in Money
   double RLot=Risk/StopLoss; //30 Points

   MqlTradeRequest request= {};
   request.action=TRADE_ACTION_PENDING;         // setting a pending order
   request.magic=EAMagic;                  // ORDER_MAGIC
   request.symbol=_Symbol;                      // symbol
   request.volume=Lot;                          // volume in 0.1 lots
   request.sl=BreakoutSellSL;                                // Stop Loss is not specified
   request.tp=BreakoutSellTP;                                // Take Profit is not specified
//--- form the order type
   request.type=ORDER_TYPE_SELL_LIMIT;                // order type
//--- form the price for the pending order
   request.price=OrangeHighPoint;  // low price
   request.expiration=OrderExpTime; //Order expiration time.
   request.deviation=0.05; //Maximum points away from my Price to Open position(5 points)
//--- send a trade request
   MqlTradeResult result= {};
   OrderSend(request,result);
//--- write the server reply to log
   Print(__FUNCTION__,":",result.comment);
   if(result.retcode==10016)
      Print(result.bid,result.ask,result.price);
//--- return code of the trade server reply
   return result.retcode;
  }
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
void OnChartEvent(const int id,const long &lparam,const double &dparam,const string &sparam)
  {
   if(id==CHARTEVENT_KEYDOWN &&
      lparam=='Q')
     {
      string msg=StringFormat("Name: %s Breakout Signal\xF4E3\nSell: %s\nS/L : %s\nT/P : %s\nTime: %s\n",
                              _Symbol,
                              (string)OrangeLowPoint,
                              (string)BreakoutSellSL,
                              (string)BreakoutSellTP,
                              TimeToString(TimeCurrent()));
      int res=bot.SendMessage(InpChannelName,msg);
      if(res!=0)
         Print("Error: ",GetErrorDescription(res));
     }
  }
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
void sendRangeSignal()
  {
   string msg=StringFormat("Name: %s Range Signal\xF4E3\nBuy: %s\nS/L : %s\nT/P : %s\n_____________\nSell : %s\nS/L : %s\nT/P : %s\nTime: %s\n",
                           _Symbol,
                           (string)OrangeLowPoint,
                           (string)BuySL,
                           (string)BuyTP,
                           (string)OrangeHighPoint,
                           (string)SellSL,
                           (string)SellTP,
                           TimeToString(TimeCurrent()));
   int res=bot.SendMessage(InpChannelName,msg);
   if(res!=0)
      Print("Error: ",GetErrorDescription(res));
  }
  
  void sendBuyBreakoutSignal()
  {
  string msg=StringFormat("Name: %s Breakout Signal\xF4E3\nBuy: %s\nS/L : %s\nT/P : %s\nTime: %s\n",
                              _Symbol,
                              (string)OrangeHighPoint,
                              (string)BreakoutBuySL,
                              (string)BreakoutBuyTP,
                              TimeToString(TimeCurrent()));
      int res=bot.SendMessage(InpChannelName,msg);
      if(res!=0)
         Print("Error: ",GetErrorDescription(res));
  }
  
  void sendSellBreakoutSignal()
  {
  string msg=StringFormat("Name: %s Breakout Signal\xF4E3\nSell: %s\nS/L : %s\nT/P : %s\nTime: %s\n",
                              _Symbol,
                              (string)OrangeLowPoint,
                              (string)BreakoutSellSL,
                              (string)BreakoutSellTP,
                              TimeToString(TimeCurrent()));
      int res=bot.SendMessage(InpChannelName,msg);
      if(res!=0)
         Print("Error: ",GetErrorDescription(res));
  }
//+------------------------------------------------------------------+

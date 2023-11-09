//+------------------------------------------------------------------+
//|                                                        Daily.mq5 |
//|                                      Copyright 2023,SabeloN Ltd. |
//|                                      https://github.com/sabeloxn |
//+------------------------------------------------------------------+
#include<Trade\Trade.mqh>
#include <Trade\PositionInfo.mqh>
#include <Trade\SymbolInfo.mqh> 
//--- input parameters
input string   botName           ="D5 Daily";
input int      EAMagic1           = 12534;
input int      EAMagic2           = 14523;
input int      EAMagic3           = 45123;
input string   SessionStartTime  ="2300";
input string   SessionEndTime    ="2259";
input bool     AddDays           =true;
input double   AmountToRisk      =100;
input double   TakeProfit        =2000;
input double TPratio =1.5;
int BarsInChart=0;
int StartHour=0;
int StartMinute=0;
int EndHour=0;
int EndMinute=0;
bool canBuy, canSell;
datetime dayStartTime, dayEndTime, OrderExpTime;
double LongEntry, ShortEntry;
MqlRates mrate[];
double mrate_close;

CTrade m_trade;
CPositionInfo  m_position;     
CSymbolInfo    m_symbol;

double balance,Risk,Points,Lot,totalRisk,SymbolSpread;
double TP1, TP2, TP3;
double CurrentExchangeRate;
double level;
string signal="";

MqlDateTime SessionStartTimeStruct;
MqlDateTime SessionEndTimeStruct;
string SessionStartTimeStructStr;
string SessionEndTimeStructStr;
bool StopsAtBreakEven =false;
//+------------------------------------------------------------------+
//| Expert initialization function                                   |
//+------------------------------------------------------------------+
int OnInit()
  {
  DeleteAllOrdersByMagic(EAMagic1);
  DeleteAllOrdersByMagic(EAMagic2);
//--- Confirm the Account Type that the script is operating on 
      if(AccountInfoInteger(ACCOUNT_TRADE_MODE)==ACCOUNT_TRADE_MODE_REAL)
        {
         Alert("Script operation is on a live account!");
        }
        
        //--
      SetSessionTradingTimes();
      setTradingTimes();
      canBuy=true;
      canSell=true;
      BarsInChart=iBars(Symbol(),PERIOD_D1);
      
      bool NewCandleAppeared=false;
      ArraySetAsSeries(mrate,true);
      int mrate_copied=CopyRates(NULL,PERIOD_D1,0,10,mrate);

        for(int i=0; i<8; i++)
        {
        MqlDateTime BarTime;
        TimeToStruct(mrate[i].time,BarTime);

           NewCandleAppeared = initCheckForNewCandle(BarsInChart);
           if(NewCandleAppeared)
            {
               BarsInChart++;
               
               if(my_condition)
               {
                  
                  Print(_Symbol,": ",botName, " Trade Values Set.");
                  
                  for(int i = BarsInChart-i;i<BarsInChart;i++)
                  {
                  mrate_close=mrate[1].close;
                  
                  if(mrate_close>LongEntry && LongEntry!=0.0)
                  {

                  signal="buy";
                  ShowTradeValues();

                  }
                  if(mrate_close<ShortEntry && ShortEntry!=0.0)
                  {
                  
                  signal="sell";
                  ShowTradeValues();

                  }
                  
                  }
                  
                  
                  break;
               }
               
               
               
               
            }
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
   Comment("");
  }
//+------------------------------------------------------------------+
//| Expert tick function                                             |
//+------------------------------------------------------------------+
void OnTick()
  {
//---
   calculatePosition();
   ShowTradeValues();
   int CandleNumber=iBars(Symbol(),PERIOD_D1);
   bool NewCandleAppeared=false;
   NewCandleAppeared = CheckForNewCandle(CandleNumber);
   
   
   datetime CurrentTime = TimeCurrent();
   
   ArraySetAsSeries(mrate,true);
   int mrate_copied=CopyRates(NULL,PERIOD_D1,0,10,mrate);
   
   if(CurrentTime>=dayStartTime && CurrentTime<dayEndTime)
     {
      if(NewCandleAppeared)
      {
         mrate_close = mrate[1].close;
         double mrate_pclose = mrate[0].close;
         double mrate_open  = mrate[1].open;
         double mrate_high  = mrate[1].high;
         double mrate_low   = mrate[1].low;
         
         datetime TodayStartTimeTmp=StringToTime(SessionStartTimeStructStr);
   
         if(mrate_pclose>LongEntry && LongEntry!=0.0)
         {
         if(canBuy)
         signal="buy";
         ShowTradeValues();
         SendBuyPendingOrder(EAMagic1,TP2,"Day Buy 1");
         SendBuyPendingOrder(EAMagic2,TP3,"Day Buy 2");
         canBuy=false;
        
         }
         if(mrate_pclose<ShortEntry && ShortEntry!=0.0)
         {
         if(canSell)
         signal="sell";
         ShowTradeValues();
         SendSellPendingOrder(EAMagic1,TP2,"Day Sell 1");
         SendSellPendingOrder(EAMagic2,TP3,"Day Sell 2");
         canSell=false;
         
         }

       }
     }

    if(signal=="buy" && iClose(Symbol(),PERIOD_M1,0)>=TP2-SymbolSpread && !StopsAtBreakEven)
      {
      Breakeven();
      Alert(_Symbol,": TP1 Hit.");
      StopsAtBreakEven=true;
      }
      else if(signal=="sell" && iClose(Symbol(),PERIOD_M1,0)<=TP2+SymbolSpread && !StopsAtBreakEven)
      {
      Breakeven();
      Alert(_Symbol,": TP1 Hit.");
      StopsAtBreakEven=true;
      }
  }
//+------------------------------------------------------------------+
//|  Function to check for a new candle, to do stuff on bar          |
//+------------------------------------------------------------------+  
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
//+----------------------------------------------------------------------+
//| Sets a Buy pending order                                             |
//+----------------------------------------------------------------------+
uint SendBuyPendingOrder(long const magic_number, double TakeProfitPrice,string Comment_label)
  {
    //--- prepare a request

   double aVolume        =       Lot/3;
   MqlTradeRequest request=      {};
   request.action        =       TRADE_ACTION_PENDING;         // setting a pending order
   request.magic         =       magic_number;                      // ORDER_MAGIC
   request.symbol        =       _Symbol;                      // symbol
   request.volume        =       aVolume;                      // volume in 0.1 lots
   double aPoint         =       SymbolInfoDouble(  _Symbol, SYMBOL_POINT  );
      double anAsk       =       SymbolInfoDouble(  _Symbol, SYMBOL_ASK    );
      double aBid        =       SymbolInfoDouble(  _Symbol, SYMBOL_BID    );
      int    aNumDigits  = (int) SymbolInfoInteger( _Symbol, SYMBOL_DIGITS );
   double aLongOpen      =       SymbolInfoDouble(  _Symbol, SYMBOL_ASK    ),
            SL           =       ShortEntry-SymbolSpread,
            TP           =       TakeProfitPrice+SymbolSpread;
            
   request.sl=SL;                                // Stop Loss is not specified
   request.tp=TP;                                // Take Profit is not specified
//--- form the order type
   request.type=ORDER_TYPE_BUY_LIMIT;            // order type
//--- form the price for the pending order
   request.price=LongEntry+SymbolSpread;        // low price
   request.expiration=OrderExpTime;             //Expiration Time
   request.deviation=0.05;                      //Maximum points away from my Price to Open position(5 points)
   request.comment=Comment_label;
//--- send a trade request
   MqlTradeResult result= {};
   OrderSend(request,result);
//--- write the server reply to log
   Print(__FUNCTION__,":",result.comment);
   if(result.retcode==10016)
      Print(result.bid,result.ask,result.price);
      //sendBuyLimitSignal(StopLossPrice,SL,TP);
   if(result.retcode==10015)
      {
      Print(result.bid,result.ask,result.price);
      //OpenBuyPosition(TP2);
      OpenBuyPosition(TP3);
      canBuy=false;
      }
      
//--- return code of the trade server reply
   return result.retcode;
  }

//+------------------------------------------------------------------+
//|  Sets a Sell pending order                                       |
//+------------------------------------------------------------------+
uint SendSellPendingOrder(long const magic_number, double TakeProfitPrice,string Comment_label)
  {
   //canSell=false;
   //--- prepare a request
   double aVolume        =       Lot/3;
   MqlTradeRequest request=      {};
   request.action        =       TRADE_ACTION_PENDING;         // setting a pending order
   request.magic         =       magic_number;                      // ORDER_MAGIC
   request.symbol        =       _Symbol;                      // symbol
   double aPoint         =       SymbolInfoDouble(  _Symbol, SYMBOL_POINT  );
      double anAsk       =       SymbolInfoDouble(  _Symbol, SYMBOL_ASK    );
      double aBid        =       SymbolInfoDouble(  _Symbol, SYMBOL_BID    );
      int    aNumDigits  = (int) SymbolInfoInteger( _Symbol, SYMBOL_DIGITS );
   double aShortOpen     =       SymbolInfoDouble(  _Symbol, SYMBOL_BID    ),
           SL            =       LongEntry+SymbolSpread,
           TP            =       TakeProfitPrice-SymbolSpread;
            
   request.volume=aVolume;                                  // volume in 0.1 lots
   request.sl=SL;                                           // Stop Loss is not specified
   request.tp=TP;                                           // Take Profit is not specified
//--- form the order type
   request.type=ORDER_TYPE_SELL_LIMIT;                      // order type
//--- form the price for the pending order
   request.price=ShortEntry-SymbolSpread;                   // low price
   request.expiration=OrderExpTime;                         //Expiration Time
   request.deviation=0.05;                                  //Maximum points away from my Price to Open position(5 points)
   request.comment=Comment_label;
//--- send a trade request
   MqlTradeResult result= {};
   OrderSend(request,result);
//--- write the server reply to log
   Print(__FUNCTION__,":",result.comment);
   if(result.retcode==10016 || result.retcode==10014)
      Print(result.bid,result.ask,result.price);
      //sendSellLimitSignal(StopLossPrice,SL, TP); 
   if(result.retcode==10015)
      {
      Print(result.bid,result.ask,result.price);
      Print("Executing Market Order.");
      //OpenSellPosition(TP2);
      OpenSellPosition(TP3);
      canSell=false;
      }
      
//--- return code of the trade server reply
   return result.retcode;
  }
//+-------------------------------------------------------------------+
//| Executes Buy Market Order on Hourly Timeframe                     |
//+-------------------------------------------------------------------+
 void OpenBuyPosition(double TakeProfitPrice)
 {
      //+--Buy
         //canBuy=false;
         double aVolume     =       Lot;
         double aPoint      =       SymbolInfoDouble(  _Symbol, SYMBOL_POINT  );
         double anAsk       =       SymbolInfoDouble(  _Symbol, SYMBOL_ASK    );
         double aBid        =       SymbolInfoDouble(  _Symbol, SYMBOL_BID    );
         int    aNumDigits  = (int) SymbolInfoInteger( _Symbol, SYMBOL_DIGITS );
         double aLongOpen   =       SymbolInfoDouble(  _Symbol, SYMBOL_ASK    ),
          SL           =       ShortEntry-SymbolSpread,
            TP           =       TakeProfitPrice+SymbolSpread;
         
         //+--Execute Market Order      
         m_trade.Buy(aVolume,Symbol(),aLongOpen,SL,TP,_Symbol+" Daily Market Buy");
           
 }
//+-------------------------------------------------------------------+
//| Executes Sell Market Order on Hourly Timeframe                    |
//+-------------------------------------------------------------------+
 void OpenSellPosition(double TakeProfitPrice)      
 {
      //+--Sell
         //canSell=false;
         double aVolume     = Lot;
         double aPoint      =       SymbolInfoDouble(  _Symbol, SYMBOL_POINT  );
         double anAsk       =       SymbolInfoDouble(  _Symbol, SYMBOL_ASK    );
         double aBid        =       SymbolInfoDouble(  _Symbol, SYMBOL_BID    );
         int    aNumDigits  = (int) SymbolInfoInteger( _Symbol, SYMBOL_DIGITS );
         double aShortOpen  =      SymbolInfoDouble(  _Symbol, SYMBOL_BID    ),
           SL            =       LongEntry+SymbolSpread,
           TP            =       TakeProfitPrice-SymbolSpread;
         //+--Execute Market Order
         m_trade.Sell(aVolume,Symbol(),aShortOpen,SL,TP,_Symbol+" Daily Market Sell");
 }
//+------------------------------------------------------------------+
//|  Function to get bars and prices in oninit()                     |
//+------------------------------------------------------------------+  
 bool initCheckForNewCandle(int CandleNumber)
 {
   static int initLastCandleNumber;
   //Print("Lst candle nr: ",initLastCandleNumber );
   bool isNewCandle=false;
   if (CandleNumber>initLastCandleNumber)
   {
   isNewCandle=true;
   initLastCandleNumber=CandleNumber;
   
   }
   return isNewCandle;
 }
//+------------------------------------------------------------------+
//| Calculates values needed for opening a position                  |
//+------------------------------------------------------------------+
 void calculatePosition()
 {
   CurrentExchangeRate=SymbolInfoDouble(_Symbol,SYMBOL_BID);
   balance=NormalizeDouble(AccountInfoDouble(ACCOUNT_BALANCE),2);
   Risk=AmountToRisk;   
   Points = NormalizeDouble((LongEntry-ShortEntry)*100000,2);//Range Points
   Lot = NormalizeDouble((Risk/Points)*CurrentExchangeRate,2);//Lot Size for Breakout
   totalRisk=NormalizeDouble((Lot/CurrentExchangeRate)*Points,2);//Risk in Money
 
 }
//+------------------------------------------------------------------+
//| Receives the current number of orders with specified ORDER_MAGIC |
//+------------------------------------------------------------------+
//int GetOrdersTotalByMagic(long const magic_number)
//  {
//   ulong order_ticket;
//   int total=0;
////--- go through all pending orders
//   for(int i=0; i<OrdersTotal(); i++)
//      if((order_ticket=OrderGetTicket(i))>0)
//         if(EAMagic==OrderGetInteger(ORDER_MAGIC))
//            total++;
////---
//   return(total);
//  }
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
         if(EAMagic1==OrderGetInteger(ORDER_MAGIC)||EAMagic2==OrderGetInteger(ORDER_MAGIC))
           {
            MqlTradeResult result= {};
            MqlTradeRequest request= {};
            request.order=order_ticket;
            request.action=TRADE_ACTION_REMOVE;
            OrderSend(request,result);
            //--- write the server reply to log
            Print(__FUNCTION__,": ",result.comment,"reply code ",result.retcode);
           }
//---
  }
//+------------------------------------------------------------------+
//|Automatically Places Orders when "Q" is pressed                   |
//+------------------------------------------------------------------+
//void OnChartEvent(const int id,const long &lparam,const double &dparam,const string &sparam)
//  {
//   if(id==CHARTEVENT_KEYDOWN &&
//      lparam=='Q')
//     {
//      //--- place or delete order
//         if(GetOrdersTotalByMagic(EAMagic)>0)
//           {
//            //DeleteAllOrdersByMagic(EAMagic);
//           }
//            double mrate_close = mrate[0].close;
//            calculatePosition();
//            canBuy=true;
//            canSell=true;
//            
//            if(mrate_close>LongEntry && LongEntry!=0.0)
//            {
//            if(canBuy)
//            SendBuyPendingOrder(EAMagic,ShortEntry-SymbolSpread,"Semi-Auto Buy");
//            }
//            if(mrate_close<ShortEntry && ShortEntry!=0.0)
//            {
//            if(canSell)
//            SendSellPendingOrder(EAMagic,LongEntry+SymbolSpread,"Semi-Auto Sell");
//            }
//     }
//  }
//+------------------------------------------------------------------+
//|Displays The Trade Values on the Chart                            |
//+------------------------------------------------------------------+
  void ShowTradeValues()
  {
  SymbolSpread =SymbolInfoInteger( _Symbol, SYMBOL_SPREAD );
  SymbolSpread/=100000;
  double maxPoints = Points * TPratio;
  double lotPerPosition =NormalizeDouble(Lot/2,2);
  double breakEvenPrice=0.0;
  if(signal == "sell")
  {
     TP1 =  NormalizeDouble(ShortEntry-((maxPoints/3)/100000),5);
     TP2 = NormalizeDouble(ShortEntry-((maxPoints/2)/100000),5);
     TP3 = NormalizeDouble(ShortEntry-((maxPoints)/100000),5);
     breakEvenPrice=(TP2+(Points/100000))-SymbolSpread;
  }
  else if (signal =="buy")
  {
     TP1 =  NormalizeDouble(LongEntry+((maxPoints/3)/100000),5);
     TP2 = NormalizeDouble(LongEntry+((maxPoints/2)/100000),5);
     TP3 = NormalizeDouble(LongEntry+((maxPoints)/100000),5);
     breakEvenPrice=(TP2-(Points/100000))+SymbolSpread;
  }
  
  double TP1profit=NormalizeDouble((lotPerPosition/CurrentExchangeRate)*(maxPoints/3),2);//Risk in Money
  double TP2profit=NormalizeDouble((lotPerPosition/CurrentExchangeRate)*(maxPoints/2),2);//Risk in Money
  double TP3profit=NormalizeDouble((lotPerPosition/CurrentExchangeRate)*(maxPoints),2);//Risk in Money
  
   Comment("Long Entry : ",LongEntry,"  || With Spread: ",LongEntry+SymbolSpread,"\n",
           "Short Entry: ",ShortEntry," || With Spread: ",ShortEntry-SymbolSpread,"\n",
           "Prev Close: ",mrate_close,"\n",  
           "Points     : ",Points,"\n",  
           "MAX Points     : ",maxPoints,"\n",
           "Lot        : ",Lot,"\n",
           "Total Risk : $",totalRisk,"\n",
           "Rec. Lot Per Position : ",lotPerPosition,"\n",
           //"Takeprofit 1 : ",TP1," = $",TP1profit,"\n",
           "Takeprofit 1 : ",TP2," = $",TP2profit,"\n",
           "Takeprofit 2 : ",TP3," = $",TP3profit,"\n",
           "Est. Total Profit: $",TP2profit+TP3profit,"\n",
           "Break Even : ",breakEvenPrice,"\n",
           "Day Start : ",dayStartTime,"\n",
           "Day End : ",dayEndTime,"\n",
           "Signal : ",signal
            );
  }
  
  void SetSessionTradingTimes()
  {
   StartHour   =(int)StringSubstr(SessionStartTime,0,2);
   EndHour     =(int)StringSubstr(SessionEndTime,0,2);
   StartMinute =(int)StringSubstr(SessionStartTime,2,2);
   EndMinute   =(int)StringSubstr(SessionEndTime,2,2);
  }
  //Function for run checks of requirements for the indicator to run
   bool OnInitPreChecksPass()
   {
   if(StartHour<0 || StartMinute<0 || StartHour>23 || StartMinute>59){
      Print("Time Start value not valid, it has to be in the format 0000-2359");
      return false;
   }
   if(SessionEndTime!="" && (EndHour<0 || EndMinute<0 || EndHour>23 || EndMinute>59)){
      Print("Time End value not valid, it has to be in the format 0000-2359");
      return false;
   }
   
   return true;
}

//+------------------------------------------------------------------+
//| Breakeven                                                        |
//+------------------------------------------------------------------+
void Breakeven()
  {
   for(int i=PositionsTotal()-1;i>=0;i--) // returns the number of open positions
      if(m_position.SelectByIndex(i))
      
         if(m_position.Symbol()==Symbol() && m_position.Magic()==EAMagic2)
           {
            if(m_position.PositionType()==POSITION_TYPE_BUY)
              {
               level=(TP2-(Points/100000))+SymbolSpread;

               m_trade.PositionModify(m_position.Ticket(),level,TP3);
               //m_trade.PositionClosePartial(m_position.Ticket(),Lot/2,-1);
               
               if(m_position.PriceOpen()>m_position.StopLoss() && level<m_position.PriceCurrent())
                 {
                  if(!m_trade.PositionModify(m_position.Ticket(),
                     m_position.PriceOpen(),
                     m_position.TakeProfit()))
                     Print("Breakeven ",m_position.Ticket(),
                           " Position -> false. Result Retcode: ",m_trade.ResultRetcode(),
                           ", description of result: ",m_trade.ResultRetcodeDescription());
                 }
              }
            else
              {
               level=(TP2+(Points/100000))-SymbolSpread;
               
               m_trade.PositionModify(m_position.Ticket(),level,TP3);
               //m_trade.PositionClosePartial(m_position.Ticket(),Lot/2,-1);
               
               if(m_position.PriceOpen()<m_position.StopLoss() && level>m_position.PriceCurrent())
                 {
                  if(!m_trade.PositionModify(m_position.Ticket(),
                     m_position.PriceOpen(),
                     m_position.TakeProfit()))
                     Print("Modify ",m_position.Ticket(),
                           " Breakeven -> false. Result Retcode: ",m_trade.ResultRetcode(),
                           ", description of result: ",m_trade.ResultRetcodeDescription());
                 }
              }
           }
  }
  
  void setTradingTimes()
  {
   
   TimeToStruct(TimeCurrent(),SessionStartTimeStruct);
   TimeToStruct(TimeCurrent(),SessionEndTimeStruct);
   
   SessionStartTimeStructStr=(string)SessionStartTimeStruct.year+"."+(string)SessionStartTimeStruct.mon+"."+(string)SessionStartTimeStruct.day+" "+(string)StartHour+":"+(string)StartMinute;
   SessionEndTimeStructStr=(string)SessionEndTimeStruct.year+"."+(string)SessionEndTimeStruct.mon+"."+(string)SessionEndTimeStruct.day+" "+(string)EndHour+":"+(string)EndMinute;
   
   dayStartTime=StringToTime(SessionStartTimeStructStr); //session start time : 0000
   dayEndTime=StringToTime(SessionEndTimeStructStr);
   if(AddDays)
   {
   dayEndTime =dayEndTime+ 86400;     //session end time : 2259
   dayStartTime=dayStartTime+ 86400; //session start time : 0000
   //Print(dayStartTime," - " ,dayEndTime+ 86400);
   }
   else
   {
   dayEndTime =StringToTime(SessionEndTimeStructStr);     //session end time : 2259
   dayStartTime=StringToTime(SessionStartTimeStructStr); //session start time : 0000
   }
  }
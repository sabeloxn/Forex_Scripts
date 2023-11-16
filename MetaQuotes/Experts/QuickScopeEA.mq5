//+------------------------------------------------------------------+
//|                                       XAU Qiuck Scope v1.0.1.mq5 |
//|                                  Copyright 2023,SabeloNxele Ltd. |
//|                                      https://github.com/sabeloxn |
//+------------------------------------------------------------------+
#property copyright ""
#property link      ""
#property version   "1.00"
#include<Trade\Trade.mqh>

//--- input parameters
input int      EAMagic = 54321;
input double   InitialQuantity=.10;
input double   StopLoss=100.0;
input double   TakeProfit=300.0;
//--- My Indicator
int _PSAR;
//--- Rates
int BarsInChart=0;
MqlRates mrate[];
//---Create instance of ctrade
CTrade m_trade;
//+------------------------------------------------------------------+
//| My Variables                                                     |
//+------------------------------------------------------------------+
int   LastRecordedPositions,
      OpenedPositionTicketNumber;
int PositionCounter=0; 

double PositionClosedInProfitTicketNumber;

double LongEntry, ShortEntry ,mrate_pclose, OrderProfit, SymbolSpread;
bool  BearBar, BullBar, DojiBar,BearBarH1, BullBarH1,
      canBuy, canSell, AboveSAR, BelowSAR;  
bool  LastCalculatedIndex = -1;

string label="none";
string AboveBelowSAR="";
datetime OrderExpTime;

//+------------------------------------------------------------------+
//| Telegram Signals                                                 |
//+------------------------------------------------------------------+
//#include <Telegram.mqh>
//--- input parameters
input string InpChannelName="";
input string InpToken="";
//CCustomBot bot; //Create instance of bot from Telegram.mqh
string     SignalDate;
bool       checked;
//+------------------------------------------------------------------+
//| Expert initialization function                                   |
//+------------------------------------------------------------------+
int OnInit()
  {
   //bot.Token(InpToken);
   //--Initialise indicator
   _PSAR = iSAR(Symbol(),PERIOD_CURRENT,0.02,0.2);
   _PSARH1 = iSAR(Symbol(),PERIOD_H1,0.02,0.2);
   canBuy=true;
   canSell=true;
   canBuyH1=true;
   canSellH1=true;
   //--- make sure that the account is demo
   if(AccountInfoInteger(ACCOUNT_TRADE_MODE)==ACCOUNT_TRADE_MODE_REAL)
     {
      Alert("Script operation is on a live account!");
      //return;
     }

     
//---
   return(INIT_SUCCEEDED);
  }
  
//+------------------------------------------------------------------+
//| Expert tick function                                             |
//+------------------------------------------------------------------+
void OnTick()
  {
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

  SymbolSpread =SymbolInfoInteger( _Symbol, SYMBOL_SPREAD );
  SymbolSpread/=100;
  //calculate last profit
  string MyLastProfit = GetLastProfit();
  //+--
  int CandleNumber=iBars(Symbol(),PERIOD_CURRENT);
  bool NewCandleAppeared=false;
  NewCandleAppeared = CheckForNewCandle(CandleNumber);

  Comment(
  "bars in chart: ",CandleNumber,"\n",
  "new candle appeared: ",NewCandleAppeared,"\n",
  "Long Entry: ",LongEntry,"\n",
  "Short Entry: ",ShortEntry,"\n",
  "Prev. Close: ",mrate_pclose,"\n",
  "Above Or Below SAR: ",AboveBelowSAR,"\n",
  "My last profit was: ","\n",MyLastProfit,"\n",
  "Comment To Delete : ",PositionClosedInProfitTicketNumber,"\n\n"
   );
   //+-PARABOLIC SAR VALUE
   double SARarray[];
   ArraySetAsSeries(SARarray,true);
   CopyBuffer(_PSAR,0,0,10,SARarray);
   double SARresult = SARarray[0];

   //+-OHLC VALUES   
   ArraySetAsSeries(mrate,true);
   int mrate_copied=CopyRates(NULL,PERIOD_CURRENT,0,10,mrate);
   mrate_pclose = mrate[1].close;
   
   if(NewCandleAppeared)
   {

      double mrate_close = mrate[1].close;
      double mrate_open  = mrate[1].open;
      double mrate_high  = mrate[1].high;
      double mrate_low   = mrate[1].low;

   //+-Define Bullish and Bearish Bars
      BullBar= mrate_close>mrate_open;
      BearBar= mrate_close<mrate_open;
      DojiBar=mrate_close==mrate_open;
      
   //+--Define Above and Below SAR
      AboveSAR=mrate_close>SARresult;
      BelowSAR=mrate_close<SARresult;
   //+--AboveBelowSAR="";
      if (AboveSAR){AboveBelowSAR="Above SAR";}
      if (BelowSAR){AboveBelowSAR="Below SAR";}
   
   //+-Store Highs and Lows based on Bartype
      if(BullBar)
      {
        LongEntry=iLow(Symbol(),PERIOD_CURRENT,1);
        canBuy=true;
      }
      if(BearBar)
      { 
        ShortEntry=iHigh(Symbol(),PERIOD_CURRENT,1);
        canSell=true;   
      }
      if(DojiBar)
      { 
         //Print("Doji Previous Bar");
      }
   //+--Open Positions on bar
     OpenPosition();
    }

  DeletePendingOrders();  
  }

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
//+-------------------------------------------------------------------+
//| Executes Market Order                                             |
//+-------------------------------------------------------------------+
 void OpenPosition()
 {
      //+--Buy
      if(mrate_pclose<LongEntry &&canBuy ) //&&BelowSAR
      {
         canBuy=false;
         int magic=EAMagic;
         double aVolume     = InitialQuantity;
         double aPoint      =       SymbolInfoDouble(  _Symbol, SYMBOL_POINT  );
         double anAsk       =       SymbolInfoDouble(  _Symbol, SYMBOL_ASK    );
         double aBid        =       SymbolInfoDouble(  _Symbol, SYMBOL_BID    );
         int    aNumDigits  = (int) SymbolInfoInteger( _Symbol, SYMBOL_DIGITS );
         double aLongOpen   =       SymbolInfoDouble(  _Symbol, SYMBOL_ASK    ),
          aLongSL     =       aBid - StopLoss * aPoint,
               SL     =       NormalizeDouble( aLongSL, aNumDigits ),
          aLongTP     =       aBid + TakeProfit * aPoint,
               TP     =       NormalizeDouble( aLongTP, aNumDigits );
         
         //+--Execute Market Order      
         m_trade.Buy(aVolume,Symbol(),aLongOpen,SL,TP,magic);
         
            if(PositionSelect(Symbol())==true)
            {
              for(int i = PositionsTotal()-1 ;i>=0 ;i--)
                 {
                  label = IntegerToString(PositionGetInteger(POSITION_TICKET));
                 }
            }
            //+--Place Hegdging Order
            SendBuyPendingOrder(EAMagic,SL,magic);
      }
      //+--Sell
      if(mrate_pclose>ShortEntry &&canSell) //&&AboveSAR
      {
         canSell=false;
         int magic=EAMagic;
         double aVolume     = InitialQuantity;
         double aPoint      =       SymbolInfoDouble(  _Symbol, SYMBOL_POINT  );
         double anAsk       =       SymbolInfoDouble(  _Symbol, SYMBOL_ASK    );
         double aBid        =       SymbolInfoDouble(  _Symbol, SYMBOL_BID    );
         int    aNumDigits  = (int) SymbolInfoInteger( _Symbol, SYMBOL_DIGITS );
         double aShortOpen   =      SymbolInfoDouble(  _Symbol, SYMBOL_BID    ),
          aShortSL     =      anAsk + StopLoss * aPoint,
               SL     =       NormalizeDouble( aShortSL, aNumDigits ),
          aShortTP     =      anAsk - TakeProfit * aPoint,
               TP     =       NormalizeDouble( aShortTP, aNumDigits );
         //+--Execute Market Order
         m_trade.Sell(aVolume,Symbol(),aShortOpen,SL,TP,magic);
         
            if(PositionSelect(Symbol())==true)
            {
              for(int i = PositionsTotal()-1 ;i>=0 ;i--)
                 {
                  label = IntegerToString(PositionGetInteger(POSITION_TICKET));
                 }
            }
         //+--Place Hedging Order
         SendSellPendingOrder(EAMagic,SL, magic);
      }
 }
//+----------------------------------------------------------------------+
//| Sets a Buy pending order                                             |
//+----------------------------------------------------------------------+
uint SendBuyPendingOrder(long const magic_number, double StopLossPrice,string Comment_label)
  {
  canBuyH1=false;
//--- prepare a request
   double aVolume     = InitialQuantity;
   MqlTradeRequest request= {};
   request.action=TRADE_ACTION_PENDING;         // setting a pending order
   request.magic=EAMagic;                  // ORDER_MAGIC
   request.symbol=_Symbol;                      // symbol
   request.volume=aVolume;                          // volume in 0.1 lots
   double aPoint      =       SymbolInfoDouble(  _Symbol, SYMBOL_POINT  );
      double anAsk       =       SymbolInfoDouble(  _Symbol, SYMBOL_ASK    );
      double aBid        =       SymbolInfoDouble(  _Symbol, SYMBOL_BID    );
      int    aNumDigits  = (int) SymbolInfoInteger( _Symbol, SYMBOL_DIGITS );
   double aLongOpen   =       SymbolInfoDouble(  _Symbol, SYMBOL_ASK    ),
       aLongSL     =       StopLossPrice - StopLoss * aPoint,
            SL     =       NormalizeDouble( aLongSL, aNumDigits ),
       aLongTP     =       aBid + 800.0 * aPoint,
            TP     =       NormalizeDouble( aLongTP, aNumDigits );
            
   request.sl=SL;                                // Stop Loss is not specified
   request.tp=TP;                                // Take Profit is not specified
//--- form the order type
   request.type=ORDER_TYPE_BUY_LIMIT;                // order type
//--- form the price for the pending order
   request.price=StopLossPrice+SymbolSpread;  // low price
   request.expiration=OrderExpTime; //Expiration Time
   request.deviation=0.05; //Maximum points away from my Price to Open position(5 points)
   request.comment=Comment_label;
//--- send a trade request
   MqlTradeResult result= {};
   OrderSend(request,result);
//--- write the server reply to log
   Print(__FUNCTION__,":",result.comment);
   if(result.retcode==10016)
      Print(result.bid,result.ask,result.price);
            
      //sendBuyLimitSignal(StopLossPrice,SL,TP);
      
      //if(SymbolInfoDouble(  _Symbol, SYMBOL_BID    ) < request.price)
      //{
      //OpenH1BuyPosition();
      //}
      
//--- return code of the trade server reply
   return result.retcode;
  }

//+------------------------------------------------------------------+
//|  Sets a Sell pending order                                       |
//+------------------------------------------------------------------+
uint SendSellPendingOrder(long const magic_number, double StopLossPrice,string Comment_label)
  {
  canSellH1=false;
//--- prepare a request
   double aVolume     = InitialQuantity;
   MqlTradeRequest request= {};
   request.action=TRADE_ACTION_PENDING;         // setting a pending order
   request.magic=EAMagic;                  // ORDER_MAGIC
   request.symbol=_Symbol;                      // symbol
   double aPoint      =       SymbolInfoDouble(  _Symbol, SYMBOL_POINT  );
      double anAsk       =       SymbolInfoDouble(  _Symbol, SYMBOL_ASK    );
      double aBid        =       SymbolInfoDouble(  _Symbol, SYMBOL_BID    );
      int    aNumDigits  = (int) SymbolInfoInteger( _Symbol, SYMBOL_DIGITS );
   double aShortOpen   =      SymbolInfoDouble(  _Symbol, SYMBOL_BID    ),
       aShortSL     =      StopLossPrice + StopLoss * aPoint,
           SL     =       NormalizeDouble( aShortSL, aNumDigits ),
       aShortTP     =      anAsk - 800.0 * aPoint,
            TP     =       NormalizeDouble( aShortTP, aNumDigits );
            
   request.volume=aVolume;                          // volume in 0.1 lots
   request.sl=SL;                                // Stop Loss is not specified
   request.tp=TP;                                // Take Profit is not specified
//--- form the order type
   request.type=ORDER_TYPE_SELL_LIMIT;                // order type
//--- form the price for the pending order
   request.price=StopLossPrice-SymbolSpread;  // low price
   request.expiration=OrderExpTime; //Expiration Time
   request.deviation=0.05; //Maximum points away from my Price to Open position(5 points)
   request.comment=Comment_label;
//--- send a trade request
   MqlTradeResult result= {};
   OrderSend(request,result);
//--- write the server reply to log
   Print(__FUNCTION__,":",result.comment);
   if(result.retcode==10016)
      Print(result.bid,result.ask,result.price);
      //sendSellLimitSignal(StopLossPrice,SL, TP);
      
      //if(SymbolInfoDouble(  _Symbol, SYMBOL_ASK    ) > request.price)
      //{
      //OpenH1SellPosition();
      //}
      
//--- return code of the trade server reply
   return result.retcode;
  }
//+------------------------------------------------------------------+
//| Gets all pending orders with specified ORDER_MAGIC               |
//+------------------------------------------------------------------+
int GetOrdersTotalByLabel(string const clabel)
  {
   ulong order_ticket;
   int total=0;
//--- go through all pending orders
   for(int i=0; i<PositionsTotal(); i++)
      if((order_ticket=PositionGetTicket(i))>0)
         if(clabel==PositionGetString(POSITION_COMMENT))
            total++;
//---
   return(total);
  }

//+------------------------------------------------------------------+
//| Gets Position that closes in profit                              |
//+------------------------------------------------------------------+
string  GetLastProfit ()
{
   uint        TotalNumberOfDeals = HistoryDealsTotal();
   ulong       TicketNumber = 0;
   double        OrderType, DealEntry,DealStopLoss;
               OrderProfit=0;
   string      MySymbol="";
   string      PositionDirection="";
   string      MyResult="";

   //get the history
   HistorySelect(0,TimeCurrent());
   
   //go through all the deals
   for(uint i=0; i<TotalNumberOfDeals; i++)
   {
      //look for a ticket number
      if((TicketNumber=HistoryDealGetTicket(i))>0)
      { //Print(i, " of ",TotalNumberOfDeals," = ",TicketNumber );
         //get the profit
         OrderProfit =HistoryDealGetDouble(TicketNumber, DEAL_PROFIT);
         //get order type
         OrderType=HistoryDealGetInteger(TicketNumber,DEAL_TYPE);
         //get the currency pair
         MySymbol = HistoryDealGetString(TicketNumber,DEAL_SYMBOL);
         //get the deal entry type to check for close prices
         DealEntry=HistoryDealGetInteger(TicketNumber,DEAL_ENTRY);
         //get the stop loss price of the deal
         DealStopLoss=HistoryDealGetDouble(TicketNumber,DEAL_SL);
         //if currency pair fits
         if(MySymbol==_Symbol)
         {
            //if it is a sell or buy oder
            if (OrderType==ORDER_TYPE_BUY || OrderType==ORDER_TYPE_SELL)
            //if the order was closed
            if(DealEntry==1)
            {
               //set all order type if close type was buy
               if(OrderType==ORDER_TYPE_BUY)
               PositionDirection="Sell_Trade";
               
               //set all order type if close type was sell
               if(OrderType==ORDER_TYPE_SELL)
               PositionDirection="Buy_Trade";
               
               MyResult="Profit: "+DoubleToString(OrderProfit)+" Deal Entry: "+DoubleToString(DealStopLoss)+" Position Direction: "+PositionDirection;
               PositionClosedInProfitTicketNumber=DealStopLoss;
            }
         }
      }
   }
   //return the result
   return MyResult;
}
//+------------------------------------------------------------------+
//| Trade function                                                   |
//+------------------------------------------------------------------+
void OnTrade()
  {
//---
   
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
  //DeleteAllOrdersByMagic(EAMagic);
 }
//+------------------------------------------------------------------+
//+------------------------------------------------------------------+
//| Send test message to telegram bot                                |
//+------------------------------------------------------------------+
//void OnChartEvent(const int id,const long &lparam,const double &dparam,const string &sparam)
//  {
//   if(id==CHARTEVENT_KEYDOWN &&
//      lparam=='Q')
//     {
//      string msg=StringFormat("Name: %s Breakout Signal\xF4E3 \nTime: %s\n",
//                              _Symbol,
//                              TimeToString(TimeCurrent()));
//      int res=bot.SendMessage(InpChannelName,msg);
//      if(res!=0)
//         Print("Error: ",GetErrorDescription(res));
//     }
//  }
//+------------------------------------------------------------------+
//| Send Buy limit signal                                            |
//+------------------------------------------------------------------+
  void sendBuyLimitSignal(double entry,double sl,double tp)
  {
  string msg=StringFormat("Buy Order: %s \xF4E3\nEntry: %s\nS/L : %s\nT/P : %s\nTime: %s\n",
                             _Symbol,
                             (string)entry,
                             (string)sl,
                             (string)tp,
                             TimeToString(TimeCurrent()));
     int res=bot.SendMessage(InpChannelName,msg);
     if(res!=0)
        Print("Error: ",GetErrorDescription(res));
  }
//+------------------------------------------------------------------+
//| Send Sell limit signal                                           |
//+------------------------------------------------------------------+ 
  void sendSellLimitSignal(double entry,double sl,double tp)
  {
  string msg=StringFormat("Sell Order: %s \xF4E3\nEntry: %s\nS/L : %s\nT/P : %s\nTime: %s\n",
                             _Symbol,
                             (string)entry,
                             (string)sl,
                             (string)tp,
                             TimeToString(TimeCurrent()));
     int res=bot.SendMessage(InpChannelName,msg);
     if(res!=0)
        Print("Error: ",GetErrorDescription(res));
  }
//+------------------------------------------------------------------+
//| Send Execute Market Order SELL signal                            |
//+------------------------------------------------------------------+ 
  void sendExecuteSellSignal()
  {
  string msg=StringFormat("Sell Market: %s \xF4E3\nAbove: %s\nS/L : %s\nT/P : %s\nTime: %s\n",
                             _Symbol,
                             (string)LongEntry,
                             (string)StopLoss,
                             (string)TakeProfit,
                             TimeToString(TimeCurrent()));
     int res=bot.SendMessage(InpChannelName,msg);
     if(res!=0)
        Print("Error: ",GetErrorDescription(res));
  }
//+------------------------------------------------------------------+
//| Send Execute Market Order BUY signal                             |
//+------------------------------------------------------------------+ 
  void sendExecuteBuySignal()
  {
  string msg=StringFormat("Buy Market: %s \xF4E3\nBelow: %s\nS/L : %s\nT/P : %s\nTime: %s\n",
                             _Symbol,
                             (string)ShortEntry,
                             (string)StopLoss,
                             (string)TakeProfit,
                             TimeToString(TimeCurrent()));
     int res=bot.SendMessage(InpChannelName,msg);
     if(res!=0)
        Print("Error: ",GetErrorDescription(res));
  }


void DeletePendingOrders()
{
    ulong Pending_order_ticket;
    double orderOpenPrice=0.0;
    string orderMagicLabel="";
    //--
    ulong order_ticket;
    double positionStoploss=0.0 ;
    int positionsTtl=GetPositionsTotalByMagic(EAMagic);
    
    for(int i=0; i<OrdersTotal(); i++)
    {
      if((Pending_order_ticket=OrderGetTicket(i))>0)
      {
         orderOpenPrice =OrderGetDouble(ORDER_PRICE_OPEN);
         orderMagicLabel =OrderGetString(ORDER_COMMENT);  
         //Print("Order Open Price:  ",orderOpenPrice);
         //--
         //if there are no open positions delete pending order
            if(positionsTtl==0)
              {
                 if(orderMagicLabel==IntegerToString(EAMagic) && OrderProfit>0.0)
                 {
                    Print("%%%%%%%%%deliting position");
                        MqlTradeResult result= {};
                        MqlTradeRequest request= {};
                        request.order=Pending_order_ticket;
                        request.action=TRADE_ACTION_REMOVE;
                        OrderSend(request,result);
                        //--- write the server reply to log
                        Print(__FUNCTION__,": ",result.comment," reply code ",result.retcode," No Positions. Closed in Profit.");
                        OrderProfit=0.0;//start over, get a new order profit.
                  }
              }
              
        }
        
    }
} 
//+------------------------------------------------------------------+
//| Receives the current number of orders with specified ORDER_MAGIC |
//+------------------------------------------------------------------+
int GetPositionsTotalByMagic(long const magic_number)
  {
   ulong order_ticket;
   int total=0;
//--- go through all pending orders
   for(int i=0; i<PositionsTotal(); i++)
      if((order_ticket=PositionGetTicket(i))>0)
         if(IntegerToString(EAMagic)==PositionGetString(POSITION_COMMENT))
            total++;
//---
   return(total);
  }

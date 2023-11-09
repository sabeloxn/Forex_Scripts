//+------------------------------------------------------------------+
//|                                       Daily Period Seperator.mq5 |
//|                                      Copyright 2023,SabeloN Ltd. |
//|                                      https://github.com/sabeloxn |
//+------------------------------------------------------------------+
#include<Trade\Trade.mqh>
CTrade m_trade;
input string   StartTime  ="1425";
input double   Lot  =1.00;
input double   StopLoss  =20.00;
input double   TakeProfit  =40.00;
int StartHour=0;
int StartMinute=0;
//--- indicator buffers
MqlRates mrate[];
bool  BearBar, BullBar, DojiBar,BearBarH1, BullBarH1,
      canBuy, canSell;
//+------------------------------------------------------------------+
//| Custom indicator initialization function                         |
//+------------------------------------------------------------------+
int OnInit()
  {OnInitInitialization();
//--- indicator buffers mapping

//---
   return(INIT_SUCCEEDED);
  }
//+------------------------------------------------------------------+
//| Expert deinitialization function                                 |
//+------------------------------------------------------------------+ 
  void OnDeinit(const int reason)
  {
   
  } 
//+------------------------------------------------------------------+
//| Expert tick function                                             |
//+------------------------------------------------------------------+
void OnTick()
  {
  int CandleNumber=iBars(Symbol(),PERIOD_CURRENT);
   bool NewCandleAppeared=false;
   NewCandleAppeared = CheckForNewCandle(CandleNumber);
   ArraySetAsSeries(mrate,true);
   int mrate_copied=CopyRates(NULL,PERIOD_CURRENT,0,10,mrate);
   
   MqlDateTime BarTime ;
         TimeToStruct(TimeCurrent(),BarTime);
         string BarTimeStr=(string)BarTime.year+"."+(string)BarTime.mon+"."+(string)BarTime.day+" "+(string)StartHour+":"+(string)StartMinute;
         datetime BarTimeTmp = StringToTime(BarTimeStr);
         
   if(NewCandleAppeared)
   {
      if(TimeCurrent()==BarTimeTmp)
      {
         double mrate_close = mrate[1].close;
         double mrate_open  = mrate[1].open;
         double mrate_high  = mrate[1].high;
         double mrate_low   = mrate[1].low;
   
      //+-Define Bullish and Bearish Bars
         BullBar= mrate_close>mrate_open;
         BearBar= mrate_close<mrate_open;
         DojiBar=mrate_close==mrate_open;
         
         if(BullBar)
         {
           canBuy=true;
           OpenBuyPosition();
         }
         if(BearBar)
         { 
           canSell=true;  
           OpenSellPosition(); 
         }
      }
      else{Comment("The time now is: ",TimeCurrent(),", Trading will commence at: ",BarTimeTmp);}
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
 
//+------------------------------------------------------------------+
//|  Function to remove graphical objects from the chart             |
//+------------------------------------------------------------------+     

void OnInitInitialization(){
   StartHour=(int)StringSubstr(StartTime,0,2);
   StartMinute=(int)StringSubstr(StartTime,2,2);
   }
//+-------------------------------------------------------------------+
//| Executes Buy Market Order on Hourly Timeframe                     |
//+-------------------------------------------------------------------+   
 void OpenBuyPosition()
 {
      //+--Buy
         canBuy=false;
         double aVolume     =       Lot;
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
         m_trade.Buy(aVolume,Symbol(),aLongOpen,SL,TP,_Symbol+" Buy");
           
 }
//+-------------------------------------------------------------------+
//| Executes Sell Market Order on Hourly Timeframe                    |
//+-------------------------------------------------------------------+
 void OpenSellPosition()      
 {
      //+--Sell
         canSell=false;
         double aVolume     = Lot;
         double aPoint      =       SymbolInfoDouble(  _Symbol, SYMBOL_POINT  );
         double anAsk       =       SymbolInfoDouble(  _Symbol, SYMBOL_ASK    );
         double aBid        =       SymbolInfoDouble(  _Symbol, SYMBOL_BID    );
         int    aNumDigits  = (int) SymbolInfoInteger( _Symbol, SYMBOL_DIGITS );
         double aShortOpen  =      SymbolInfoDouble(  _Symbol, SYMBOL_BID    ),
           aShortSL     =      anAsk + StopLoss * aPoint,
               SL     =       NormalizeDouble( aShortSL, aNumDigits ),
          aShortTP     =      anAsk - TakeProfit * aPoint,
               TP     =       NormalizeDouble( aShortTP, aNumDigits );
         //+--Execute Market Order
         m_trade.Sell(aVolume,Symbol(),aShortOpen,SL,TP,_Symbol+" Sell");
 }
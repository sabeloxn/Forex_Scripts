// -------------------------------------------------------------------------------------------------
//
//    This code is a cTrader Automate API example.
//
//    This cBot is intended to be used as a sample and does not guarantee any particular outcome or
//    profit of any kind. Use it at your own risk.
//    
//    All changes to this file might be lost on the next application update.
//    If you are going to modify this file please make a copy using the "Duplicate" command.
//
//    The "Sample RSI cBot" will create a buy order when the Relative Strength Index indicator crosses the  level 30, 
//    and a Sell order when the RSI indicator crosses the level 70. The order is closed be either a Stop Loss, defined in 
//    the "Stop Loss" parameter, or by the opposite RSI crossing signal (buy orders close when RSI crosses the 70 level 
//    and sell orders are closed when RSI crosses the 30 level). 
//
//    The cBot can generate only one Buy or Sell order at any given time.
//
// -------------------------------------------------------------------------------------------------

using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class SampleRSIcBot : Robot
    {
        [Parameter("Quantity (Lots)", Group = "Volume", DefaultValue = 1, MinValue = 0.01, Step = 0.01)]
        public double Quantity { get; set; }

        [Parameter("Source", Group = "RSI")]
        public DataSeries Source { get; set; }

        [Parameter("Periods", Group = "RSI", DefaultValue = 14)]
        public int Periods { get; set; }

        private RelativeStrengthIndex rsi;

        protected override void OnStart()
        {
            rsi = Indicators.RelativeStrengthIndex(Source, Periods);
        }

        protected override void OnTick()
        {
            if (rsi.Result.LastValue < 30)
            {
                Close(TradeType.Sell);
                Open(TradeType.Buy);
            }
            else if (rsi.Result.LastValue > 70)
            {
                Close(TradeType.Buy);
                Open(TradeType.Sell);
            }
        }

        private void Close(TradeType tradeType)
        {
            foreach (var position in Positions.FindAll("SampleRSI", SymbolName, tradeType))
                ClosePosition(position);
        }

        private void Open(TradeType tradeType)
        {
            var position = Positions.Find("SampleRSI", SymbolName, tradeType);
            var volumeInUnits = Symbol.QuantityToVolumeInUnits(Quantity);

            if (position == null)
                ExecuteMarketOrder(tradeType, SymbolName, volumeInUnits, "SampleRSI");
        }
    }
}

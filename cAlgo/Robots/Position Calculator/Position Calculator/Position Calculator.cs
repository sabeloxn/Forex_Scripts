using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class PositionCalculator : Robot
    {
        [Parameter("Amount To Risk in $",DefaultValue = 100)]
        public double AmountToRisk { get; set; }
        [Parameter("StopLoss",DefaultValue = 100)]
        public double StopLoss { get; set; }
        public double TakeProfit{ get; set; }
        protected override void OnStart()
        {
            //var volume = (AmountToRisk/StopLoss)/Symbol.PipValue;
            //Print(Symbol.NormalizeVolumeInUnits(AmountToRisk/StopLoss,RoundingMode.Up));
            double LotSize =Math.Round((AmountToRisk / StopLoss),2);
            Print(LotSize);
            // takeProfit has to equal $100 too. put the stop at 100 pips if it is less than
            TakeProfit =Math.Round((AmountToRisk / LotSize),2);
            Print(TakeProfit);
            if (TakeProfit<100){TakeProfit=100;Print("Proposed TP: {0} == {1}",TakeProfit,LotSize*TakeProfit);}
            else if (TakeProfit>100){TakeProfit=StopLoss;Print("Proposed TP: {0} == {1}",TakeProfit,LotSize*TakeProfit);}
        }

        protected override void OnTick()
        {
            // Handle price updates here
        }

        protected override void OnStop()
        {
            // Handle cBot stop here
        }
    }
}
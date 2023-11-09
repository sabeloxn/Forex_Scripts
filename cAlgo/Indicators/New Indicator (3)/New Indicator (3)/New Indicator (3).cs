using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;


//calculate position size for current balance and specified risk (stop loss) per position; 
//does not take not account commission or spread though, simply adjust risk % to accommodate those

namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = true, AccessRights = AccessRights.None)]
    public class PositionRiskkrp : Indicator
    {

        public override void Calculate(int index)
        {

        }

    }
}

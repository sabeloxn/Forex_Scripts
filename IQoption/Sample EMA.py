import talib
from talib.abstract import *
from iqoptionapi.stable_api import IQ_Option
import time
import numpy as np
print("login...")
Iq=IQ_Option("teyasabelo@gmail.com","wzt79iczg.*Z.Z9")
Iq.connect()#connect to iqoption
goal="EURUSD"
size=10#size=[1,5,10,15,30,60,120,300,600,900,1800,3600,7200,14400,28800,43200,86400,604800,2592000,"all"]
timeperiod=10
maxdict=20
print("start stream...")
Iq.start_candles_stream(goal,size,maxdict)
print("Start EMA Sample")
while True:
    candles=Iq.get_realtime_candles(goal,size)

    inputs = {
        'open': np.array([]),
        'high': np.array([]),
        'low': np.array([]),
        'close': np.array([]),
        'volume': np.array([])
    }
    for timestamp in candles:

        inputs["open"]=np.append(inputs["open"],candles[timestamp]["open"] )
        inputs["high"]=np.append(inputs["open"],candles[timestamp]["max"] )
        inputs["low"]=np.append(inputs["open"],candles[timestamp]["min"] )
        inputs["close"]=np.append(inputs["open"],candles[timestamp]["close"] )
        inputs["volume"]=np.append(inputs["open"],candles[timestamp]["volume"] )


    print("Show EMA")
    print(talib.EMA(inputs["close"], timeperiod=timeperiod))
    print("\n")
    time.sleep(1)
Iq.stop_candles_stream(goal,size)
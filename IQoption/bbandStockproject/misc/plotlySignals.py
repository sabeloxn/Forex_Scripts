import csv
import plotly.graph_objects as go
from plotly.subplots import make_subplots
import pandas as pd
import pandas_ta as ta
import talib
import numpy as np
from itertools import compress
#IMPORT IQ OPTIONS API
from iqoptionapi.api import IQOptionAPI
from iqoptionapi.stable_api import IQ_Option

#USER ACCOUNT CREDENTIALS AND LOG IN 
my_user = "teyasabelo@gmail.com"    #YOUR IQOPTION USERNAME
my_pass = "nhrFtXqQw@QCHi7"         #YOUR IQOTION PASSWORD
#CONNECT ==>:
Iq=IQ_Option(my_user,my_pass)
iqch1,iqch2 =   Iq.connect()
if iqch1    ==  True:
    print("Logged in.")
else:
    print("Log In Failed.")
#DONE

#CHOOSE BALANCE TYPE
balance_type    =   "PRACTICE"
if balance_type ==  'REAL':
    Iq.change_balance(balance_type)
print("Waiting for conditions to place position...")


#SET UP TRADE PARAMETERS 
Money               =   10                      #Amount for Option
goal                =   "EURUSD"                #Target Instrument
size                =   60                      #Timeframe In Secondsœ
period              =   100                       #Number of Bars to look back
expirations_mode    =   1                       #Option Expiration Time in Minutes

#GET OHLC DATA FROM IQOPTION
Iq.start_candles_stream(goal,size,period)
cc=Iq.get_realtime_candles(goal,size)

#second level keys 
fields = ['active_id','id','from','at','to','open','close','min','max','volume','min_at', 
          'ask', 'phase', 'max_at', 'bid', 'size']
#automatically add keys

#create csv
with open('students1.csv', 'w',newline='') as csvfile:     
    w = csv.DictWriter( csvfile, fieldnames =fields )
    w.writeheader()
    for key,val in sorted(cc.items()):
        row = {'active_id': key}
        row.update(val)
        w.writerow(row)

df = pd.read_csv('students1.csv', usecols= ['at','open','max','min','close'])

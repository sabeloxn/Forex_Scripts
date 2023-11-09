#IMPORT IQ OPTIONS API
from iqoptionapi.api import IQOptionAPI
from iqoptionapi.stable_api import IQ_Option
import os
import tempfile
from io import StringIO
import csv
#IMPORT NUMPY AND TALIB
import plotly.graph_objects as go
import  numpy as np 
import  pandas as pd
from    statistics import mean
#--IMPORT THREADING AND TIME (ESSENTIAL)
import  threading
import  time as t
#--END OF IMPORTS

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
period              =   4                       #Number of Bars to look back
expirations_mode    =   1                       #Option Expiration Time in Minutes

#GET OHLC DATA FROM IQOPTION
Iq.start_candles_stream(goal,size,period)
cc=Iq.get_realtime_candles(goal,size)

base_url = 'http://example.com'
data = [
    ['sku', 'url']
    , [1, '{}/path/to'.format(base_url)]
    , [2, '{}/path/to2'.format(base_url)]
]
f = StringIO()
w = csv.writer(f, delimiter='\t')
for row in data:
    w.writerow(row)
f.seek(0)        
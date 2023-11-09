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
from candle_rankings import candle_rankings

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

fig = make_subplots(rows=2, cols=1)
# Add title and format axes
fig.update_layout(
    title='Bitcoin price in the last 2 hours with Bollinger Bands',
    yaxis_title='BTC/USD')

candle_names = talib.get_function_groups()['Pattern Recognition']
# extract OHLC 
op = df['open']
hi = df['max']
lo = df['min']
cl = df['close']
# create columns for each pattern
for candle in candle_names:
    # below is same as;
    # df["CDL3LINESTRIKE"] = talib.CDL3LINESTRIKE(op, hi, lo, cl)
    df[candle] = getattr(talib, candle)(op, hi, lo, cl)

df['candlestick_pattern'] = np.nan
df['candlestick_match_count'] = np.nan
for index, row in df.iterrows():

        # no pattern found
        if len(row[candle_names]) - sum(row[candle_names] == 0) == 0:
            df.loc[index,'candlestick_pattern'] = "NO_PATTERN"
            df.loc[index, 'candlestick_match_count'] = 0
        # single pattern found
        elif len(row[candle_names]) - sum(row[candle_names] == 0) == 1:
            # bull pattern 100 or 200
            if any(row[candle_names].values > 0):
                pattern = list(compress(row[candle_names].keys(), row[candle_names].values != 0))[0] + '_Bull'
                df.loc[index, 'candlestick_pattern'] = pattern
                df.loc[index, 'candlestick_match_count'] = 1
                signal_index = df.loc[index]
            # bear pattern -100 or -200
            else:
                pattern = list(compress(row[candle_names].keys(), row[candle_names].values != 0))[0] + '_Bear'
                df.loc[index, 'candlestick_pattern'] = pattern
                df.loc[index, 'candlestick_match_count'] = 1
                signal_index = df.loc[index]
        # multiple patterns matched -- select best performance
        else:
            # filter out pattern names from bool list of values
            patterns = list(compress(row[candle_names].keys(), row[candle_names].values != 0))
            container = []
            for pattern in patterns:
                if row[pattern] > 0:
                    container.append(pattern + '_Bull')
                else:
                    container.append(pattern + '_Bear')
            rank_list = [candle_rankings[p] for p in container]
            if len(rank_list) == len(container):
                rank_index_best = rank_list.index(min(rank_list))
                df.loc[index, 'candlestick_pattern'] = container[rank_index_best]
                df.loc[index, 'candlestick_match_count'] = len(container)
    # clean up candle columns
df.drop(candle_names, axis = 1, inplace = True)

fig.append_trace = go.Candlestick(
            open=op,
            high=hi,
            low=lo,
            close=cl)

layout = {
    'title': '2019 Feb - 2020 Feb Bitcoin Candlestick Chart',
    'yaxis': {'title': 'Price'},
    'xaxis': {'title': 'Index Number'},

}

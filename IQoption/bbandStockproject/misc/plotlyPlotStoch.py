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
""" for k, v in cc.items():
    for k1, v1 in v.items():
        print(k1)
        fields.append(k1) """
#create csv
with open('students.csv', 'w',newline='') as csvfile:     
    w = csv.DictWriter( csvfile, fieldnames =fields )
    w.writeheader()
    for key,val in sorted(cc.items()):
        row = {'active_id': key}
        row.update(val)
        w.writerow(row)

df = pd.read_csv('students.csv', usecols= ['at','open','max','min','close'])

#calculate stochastic
# Define periods
k_period = 14
d_period = 3
    # Adds a "n_high" column with max value of previous 14 periods
df['n_max'] = df['max'].rolling(k_period).max()
    # Adds an "n_low" column with min value of previous 14 periods
df['n_min'] = df['min'].rolling(k_period).min()
    # Uses the min/max values to calculate the %k (as a percentage)
df['%K'] = (df['close'] - df['n_min']) * 100 / (df['n_max'] - df['n_min'])
    # Uses the %k to calculates a SMA over the past 3 values of %k
df['%D'] = df['%K'].rolling(d_period).mean()
df.columns = [x.lower() for x in df.columns]
fig = make_subplots(rows=2, cols=1)

# Add some indicators
df.ta.stoch(high='max', low='min', k=14, d=3, append=True)

# Create our Candlestick chart with an overlaid price line
fig.append_trace(
    go.Candlestick(
        x=df.index,
        open=df['open'],
                high=df['max'],
                low=df['min'],
                close=df['close'],
        increasing_line_color='#ff9900',
        decreasing_line_color='black',
        showlegend=False
    ), row=1, col=1  # <------------ upper chart
)

# price Line
""" fig.append_trace(
    go.Scatter(
        x=df.index,
        y=df['open'],
        line=dict(color='#ff9900', width=1),
        name='open',
    ), row=1, col=1  # <------------ upper chart
) """

# Fast Signal (%k)
fig.append_trace(
    go.Scatter(
        x=df.index,
        y=df['STOCHk_14_3_3'],
        line=dict(color='#ff9900', width=2),
        name='fast',
    ), row=2, col=1  #  <------------ lower chart
)

# Slow signal (%d)
fig.append_trace(
    go.Scatter(
        x=df.index,
        y=df['STOCHd_14_3_3'],
        line=dict(color='#000000', width=2),
        name='slow'
    ), row=2, col=1 # <------------ lower chart
)
#pattern marker

""" fig.append_trace(
    go.Scatter(
        mode="markers",
        x="index of candle",
        y= "high or low of candle",
        marker=dict(
            color='Green',
            size = 20,
            line=dict(
                color='MediumPurple',
                width=2
            )
        ),
        showlegend=False
        row=1, col=1  # <------------ upper chart
    )

) """
# Extend our y-axis a bit
fig.update_yaxes(range=[-10, 110], row=2, col=1)

# Add upper/lower bounds
fig.add_hline(y=0, col=1, row=2, line_color="#666", line_width=2)
fig.add_hline(y=100, col=1, row=2, line_color="#666", line_width=2)

# Add overbought/oversold
fig.add_hline(y=20, col=1, row=2, line_color='#336699', line_width=2, line_dash='dash')
fig.add_hline(y=80, col=1, row=2, line_color='#336699', line_width=2, line_dash='dash')

# Make it pretty
layout = go.Layout(
    plot_bgcolor='#efefef',
    # Font Families
    font_family='Monospace',
    font_color='#000000',
    font_size=20,
    xaxis=dict(
        rangeslider=dict(
            visible=False
        )
    )
)
fig.update_layout(layout)
# View our chart in the system default HTML viewer (Chrome, Firefox, etc.)

# Define the parameters for the Bollinger Band calculation
ma_size = 20
bol_size = 2

# Calculate the SMA
df.insert(0, 'moving_average', df['close'].rolling(ma_size).mean())

# Calculate the upper and lower Bollinger Bands
df.insert(0, 'bol_upper', df['moving_average'] + df['close'].rolling(ma_size).std() * bol_size)
df.insert(0, 'bol_lower', df['moving_average'] - df['close'].rolling(ma_size).std() * bol_size)

# Remove the NaNs -> consequence of using a non-centered moving average
df.dropna(inplace=True)

# Plot the three lines of the Bollinger Bands indicator
for parameter in ['moving_average', 'bol_lower', 'bol_upper']:
    fig.add_trace(go.Scatter(
        x = df.index,
        y = df[parameter],
        showlegend = False,
        line_color = 'gray',
        mode='lines',
        line={'dash': 'dash'},
        marker_line_width=2, 
        marker_size=10,
        opacity = 0.8))

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

candle_rankings = {
        "CDL3LINESTRIKE_Bull": 1,
        "CDL3LINESTRIKE_Bear": 2,
        "CDL3BLACKCROWS_Bull": 3,
        "CDL3BLACKCROWS_Bear": 3,
        "CDLEVENINGSTAR_Bull": 4,
        "CDLEVENINGSTAR_Bear": 4,
        "CDLTASUKIGAP_Bull": 5,
        "CDLTASUKIGAP_Bear": 5,
        "CDLINVERTEDHAMMER_Bull": 6,
        "CDLINVERTEDHAMMER_Bear": 6,
        "CDLMATCHINGLOW_Bull": 7,
        "CDLMATCHINGLOW_Bear": 7,
        "CDLABANDONEDBABY_Bull": 8,
        "CDLABANDONEDBABY_Bear": 8,
        "CDLBREAKAWAY_Bull": 10,
        "CDLBREAKAWAY_Bear": 10,
        "CDLMORNINGSTAR_Bull": 12,
        "CDLMORNINGSTAR_Bear": 12,
        "CDLPIERCING_Bull": 13,
        "CDLPIERCING_Bear": 13,
        "CDLSTICKSANDWICH_Bull": 14,
        "CDLSTICKSANDWICH_Bear": 14,
        "CDLTHRUSTING_Bull": 15,
        "CDLTHRUSTING_Bear": 15,
        "CDLINNECK_Bull": 17,
        "CDLINNECK_Bear": 17,
        "CDL3INSIDE_Bull": 20,
        "CDL3INSIDE_Bear": 56,
        "CDLHOMINGPIGEON_Bull": 21,
        "CDLHOMINGPIGEON_Bear": 21,
        "CDLDARKCLOUDCOVER_Bull": 22,
        "CDLDARKCLOUDCOVER_Bear": 22,
        "CDLIDENTICAL3CROWS_Bull": 24,
        "CDLIDENTICAL3CROWS_Bear": 24,
        "CDLMORNINGDOJISTAR_Bull": 25,
        "CDLMORNINGDOJISTAR_Bear": 25,
        "CDLXSIDEGAP3METHODS_Bull": 27,
        "CDLXSIDEGAP3METHODS_Bear": 26,
        "CDLTRISTAR_Bull": 28,
        "CDLTRISTAR_Bear": 76,
        "CDLGAPSIDESIDEWHITE_Bull": 46,
        "CDLGAPSIDESIDEWHITE_Bear": 29,
        "CDLEVENINGDOJISTAR_Bull": 30,
        "CDLEVENINGDOJISTAR_Bear": 30,
        "CDL3WHITESOLDIERS_Bull": 32,
        "CDL3WHITESOLDIERS_Bear": 32,
        "CDLONNECK_Bull": 33,
        "CDLONNECK_Bear": 33,
        "CDL3OUTSIDE_Bull": 34,
        "CDL3OUTSIDE_Bear": 39,
        "CDLRICKSHAWMAN_Bull": 35,
        "CDLRICKSHAWMAN_Bear": 35,
        "CDLSEPARATINGLINES_Bull": 36,
        "CDLSEPARATINGLINES_Bear": 40,
        "CDLLONGLEGGEDDOJI_Bull": 37,
        "CDLLONGLEGGEDDOJI_Bear": 37,
        "CDLHARAMI_Bull": 38,
        "CDLHARAMI_Bear": 72,
        "CDLLADDERBOTTOM_Bull": 41,
        "CDLLADDERBOTTOM_Bear": 41,
        "CDLCLOSINGMARUBOZU_Bull": 70,
        "CDLCLOSINGMARUBOZU_Bear": 43,
        "CDLTAKURI_Bull": 47,
        "CDLTAKURI_Bear": 47,
        "CDLDOJISTAR_Bull": 49,
        "CDLDOJISTAR_Bear": 51,
        "CDLHARAMICROSS_Bull": 50,
        "CDLHARAMICROSS_Bear": 80,
        "CDLADVANCEBLOCK_Bull": 54,
        "CDLADVANCEBLOCK_Bear": 54,
        "CDLSHOOTINGSTAR_Bull": 55,
        "CDLSHOOTINGSTAR_Bear": 55,
        "CDLMARUBOZU_Bull": 71,
        "CDLMARUBOZU_Bear": 57,
        "CDLUNIQUE3RIVER_Bull": 60,
        "CDLUNIQUE3RIVER_Bear": 60,
        "CDL2CROWS_Bull": 61,
        "CDL2CROWS_Bear": 61,
        "CDLBELTHOLD_Bull": 62,
        "CDLBELTHOLD_Bear": 63,
        "CDLHAMMER_Bull": 65,
        "CDLHAMMER_Bear": 65,
        "CDLHIGHWAVE_Bull": 67,
        "CDLHIGHWAVE_Bear": 67,
        "CDLSPINNINGTOP_Bull": 69,
        "CDLSPINNINGTOP_Bear": 73,
        "CDLUPSIDEGAP2CROWS_Bull": 74,
        "CDLUPSIDEGAP2CROWS_Bear": 74,
        "CDLGRAVESTONEDOJI_Bull": 77,
        "CDLGRAVESTONEDOJI_Bear": 77,
        "CDLHIKKAKEMOD_Bull": 82,
        "CDLHIKKAKEMOD_Bear": 81,
        "CDLHIKKAKE_Bull": 85,
        "CDLHIKKAKE_Bear": 83,
        "CDLENGULFING_Bull": 84,
        "CDLENGULFING_Bear": 91,
        "CDLMATHOLD_Bull": 86,
        "CDLMATHOLD_Bear": 86,
        "CDLHANGINGMAN_Bull": 87,
        "CDLHANGINGMAN_Bear": 87,
        "CDLRISEFALL3METHODS_Bull": 94,
        "CDLRISEFALL3METHODS_Bear": 89,
        "CDLKICKING_Bull": 96,
        "CDLKICKING_Bear": 102,
        "CDLDRAGONFLYDOJI_Bull": 98,
        "CDLDRAGONFLYDOJI_Bear": 98,
        "CDLCONCEALBABYSWALL_Bull": 101,
        "CDLCONCEALBABYSWALL_Bear": 101,
        "CDL3STARSINSOUTH_Bull": 103,
        "CDL3STARSINSOUTH_Bear": 103,
        "CDLDOJI_Bull": 104,
        "CDLDOJI_Bear": 104,
        "CDLSHORTLINE_Bear":105,
        "CDLSHORTLINE_Bull":105,
        "CDLLONGLINE_Bear":106,
        "CDLLONGLINE_Bull":106
    }

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

""" fig.append_trace = go.Candlestick(
            open=op,
            high=hi,
            low=lo,
            close=cl)

layout = {
    'title': '2019 Feb - 2020 Feb Bitcoin Candlestick Chart',
    'yaxis': {'title': 'Price'},
    'xaxis': {'title': 'Index Number'},

}
 """
fig.show()

# Overbought status
""" if k > 80 and d > 80 and k < d:
    sell
# Oversold status   
else if k < 20 and d < 20 and k > d:
    buy 
# Something in the middle
else:
    do nothing """
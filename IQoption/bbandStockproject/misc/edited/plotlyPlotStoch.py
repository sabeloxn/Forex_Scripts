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
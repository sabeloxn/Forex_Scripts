from createReadCSV import df
from calculateStoch import fig 
import plotly.graph_objects as go
import csv
import pandas as pd
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
    title='YT @yasabelo',
    yaxis_title='EUR/USD')

df.to_csv('my_parameters.csv')

""" #create full csv
fields =['CDL2CROWS', 'CDL3BLACKCROWS', 'CDL3INSIDE', 'CDL3LINESTRIKE', 'CDL3OUTSIDE', 'CDL3STARSINSOUTH', 'CDL3WHITESOLDIERS', 'CDLABANDONEDBABY', 'CDLADVANCEBLOCK', 'CDLBELTHOLD', 'CDLBREAKAWAY', 'CDLCLOSINGMARUBOZU', 'CDLCONCEALBABYSWALL', 'CDLDARKCLOUDCOVER', 'CDLDOJI', 'CDLDOJISTAR', 'CDLDRAGONFLYDOJI', 'CDLENGULFING', 'CDLEVENINGDOJISTAR', 'CDLEVENINGSTAR', 'CDLGAPSIDESIDEWHITE', 'CDLGRAVESTONEDOJI', 'CDLHAMMER', 'CDLHANGINGMAN', 'CDLHARAMI', 'CDLHARAMICROSS', 'CDLHIGHWAVE', 'CDLHIKKAKE', 'CDLHIKKAKEMOD', 'CDLHOMINGPIGEON', 'CDLIDENTICAL3CROWS', 'CDLINNECK', 'CDLINVERTEDHAMMER', 'CDLKICKING', 'CDLLADDERBOTTOM', 'CDLLONGLEGGEDDOJI', 'CDLMARUBOZU', 'CDLMATCHINGLOW', 'CDLMATHOLD', 'CDLMORNINGDOJISTAR', 'CDLMORNINGSTAR', 'CDLONNECK', 'CDLPIERCING', 'CDLRICKSHAWMAN', 'CDLRISEFALL3METHODS', 'CDLSEPARATINGLINES', 'CDLSHOOTINGSTAR', 'CDLSPINNINGTOP', 'CDLSTICKSANDWICH', 'CDLTAKURI', 'CDLTASUKIGAP', 'CDLTHRUSTING', 'CDLTRISTAR', 'CDLUNIQUE3RIVER', 'CDLUPSIDEGAP2CROWS', 'CDLXSIDEGAP3METHODS']

with open('allParams.csv', 'w',newline='') as csvfile:     
    w = csv.DictWriter( csvfile, fieldnames =fields )
    w.writeheader()
    for key,val in sorted(cc.items()):
        row = {'active_id': key}
        row.update(val)
        w.writerow(row)

df = pd.read_csv('students.csv', usecols= ['at','open','max','min','close'])

fig = go.Figure(data=[go.Candlestick(x=df['at'],
                open=df['open'],
                high=df['max'],
                low=df['min'],
                close=df['close'])]) """
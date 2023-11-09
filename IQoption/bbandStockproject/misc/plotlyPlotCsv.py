import plotly.graph_objects as go
import os

import pandas as pd
from datetime import datetime



df = pd.read_csv('students.csv', usecols= ['at','open','max','min','close'])

fig = go.Figure(data=[go.Candlestick(x=df['at'],
                open=df['open'],
                high=df['max'],
                low=df['min'],
                close=df['close'])])

fig.show()

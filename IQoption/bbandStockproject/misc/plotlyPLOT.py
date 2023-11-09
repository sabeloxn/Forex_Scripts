import plotly.graph_objects as go
import os
import tempfile
from io import StringIO
import csv
import pandas as pd
from datetime import datetime

df = pd.read_csv('https://raw.githubusercontent.com/plotly/datasets/master/finance-charts-apple.csv')

fig = go.Figure(data=[go.Candlestick(x=df['Date'],
                open=df['AAPL.Open'],
                high=df['AAPL.High'],
                low=df['AAPL.Low'],
                close=df['AAPL.Close'])])

fig.show()

def write_csv(csvfile):
    writer = csv.DictWriter(csvfile, fieldnames=['foo', 'bar'])
    writer.writeheader()
    writer.writerow({'foo': 1, 'bar': 2})

def test_write_csv():
    with tempfile.NamedTemporaryFile(mode='w', delete=False) as csvfile:
        write_csv(csvfile)
    with open(csvfile.name) as csvfile:
        reader = csv.DictReader(csvfile)
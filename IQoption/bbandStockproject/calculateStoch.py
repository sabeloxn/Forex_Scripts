from plotly.subplots import make_subplots
import plotly.graph_objects as go
from createReadCSV import df
import pandas_ta as ta
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
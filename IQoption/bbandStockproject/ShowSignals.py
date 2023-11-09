from calculateStoch import fig 
from calculateBband import fig,df
from identifyPattern import *
import pandas as pd
from calculateStoch import fig 
import plotly.graph_objects as go
is_overbought   = False
is_oversold     = False
above_bband     = False
below_bband     = False
is_pattern =False

def draw_signal():
     fig.append_trace(
            go.Scatter(
                mode="markers",
                x=df.index,
                y= df['max'],
                marker=dict(symbol='arrow-down',color='Green',size = 13),
                line=dict(color='#ff9900',width=2),
                name="signal",
                       ),
                row=1, col=1  # <------------ upper chart
)

recognize_candlestick(df)
new_df = pd.read_csv("./patterns.csv")

""" row_num = df[df['candlestick_pattern'] == 'CDLXSIDEGAP3METHODS_Bull'].index
print(row_num) """

row_num = df[df['candlestick_pattern'] != 'NO_PATTERN'].index
#print(row_num)

#find pattern
if any(df[df['candlestick_pattern'] != 'NO_PATTERN'].index):
        is_pattern=True
        draw_signal()


# Overbought status
k=0
d=0
k_list=list(df["%k"])
for i in k_list:
    if i > 80:
        k=i
d_list=list(df["%d"])
for i in d_list:
    if i > 80:
        d=i

# bband break
bol_upper=0
bol_lower=0
bol_upper_list=list(df["bol_upper"])
for j in bol_upper_list:
     bol_upper = j

bol_lower_list=list(df["bol_lower"])
for j in bol_lower_list:
     bol_lower = j


""" if k > 80 and d > 80 and k < d:
    sell
# Oversold status   
else if k < 20 and d < 20 and k > d:
    buy 
# Something in the middle
else:
    do nothing """


fig.show()
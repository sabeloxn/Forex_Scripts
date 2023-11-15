#IMPORT IQ OPTIONS API
from iqoptionapi.api import IQOptionAPI
from iqoptionapi.stable_api import IQ_Option

#IMPORT NUMPY AND TALIB
import  numpy as np 
import  talib
from    statistics import mean
#--IMPORT THREADING AND TIME (ESSENTIAL)
import  threading
import  time as t
#--END OF IMPORTS

#USER ACCOUNT CREDENTIALS AND LOG IN 
my_user = ""    #YOUR IQOPTION USERNAME
my_pass = ""         #YOUR IQOTION PASSWORD
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
goal                =   "EURUSD-OTC"            #Target Instrument
size                =   60                      #Timeframe In Secondsœ
period              =   14                      #Number of Bars to look back
k4d_period          =   3                       #Need for stoch calc(s)
expirations_mode    =   1                       #Option Expiration Time in Minutes

#GET OHLC DATA FROM IQOPTION
Iq.start_candles_stream(goal,size,period)
cc=Iq.get_realtime_candles(goal,size)
#DO IT 4 THE D
Iq.start_candles_stream(goal,size,k4d_period)
#STORE OHLC DATA
my_open   =[]
my_high   =[]
my_low    =[]
my_close  =[]
my_dClose =[]
#WHEN TO PLACE BET BEFORE EXPIRY TIME (TIME IN SECONDS)
place_at  = 0
def get_purchase_time():
    remaning_time   =   Iq.get_remaning(expirations_mode)   
    purchase_time   =   remaning_time
    return purchase_time

def get_expiration_time():
    exp         =   Iq.get_server_timestamp()
    time_to_buy =   (exp % size)
    return int(time_to_buy)

#THREAD TO RUN TIMER SIMULTANEOUSLY
def expiration_thread():
    global place_at
    while True:
        x       =   get_expiration_time()
        t.sleep(1)
        if x    ==  place_at:
            place_option(Money,goal,expirations_mode)
threading.Thread(target=expiration_thread).start()

#SET VALUES TO PLACE BET(S)
def set_values():

    global open_val
    global high_val
    global low_val
    global close_val
    global lower_band_val
    global stoch_status
    global upper_band_val

    global ma_close_val


    for price in list(cc.keys()):
        open        =(cc[price]['open'])
        high        =(cc[price]['high'])
        low         =(cc[price]['low'])
        close       =(cc[price]['open'])
        d_close     =(cc[price]['open'])
        
        my_open.append(open)
        open_size   =len(my_open)
        open_val    =my_open[open_size-2]

        my_high.append(high)
        high_size   =len(my_high)
        high_val    =my_high[high_size-2]

        my_low.append(low)
        low_size    =len(my_low)
        low_val     =my_low[low_size-2]

        my_close.append(close)
        close_size  =len(my_close)
        close_val   =my_close[close_size-1]

        d_close.append(d_close)
        d_close_size  =len(d_close)
        d_close_val   =d_close[d_close_size-1]

        #NECESSARY TO GET BOLLINGER VALUES
        my_ma_close         =np.array(my_close)
        ma_close_values     = talib.SMA(my_ma_close,timeperiod=14)
        my_ma_close_size    =len(ma_close_values)
        ma_close_val        = ma_close_values[my_ma_close_size-1]

        stoch_osc   =   talib.STOCH(my_high, my_low, my_close, fastk_period=period, slowk_period=3, slowd_period=3)
        k4d         =   (my_close-my_low)*100/(my_high-my_low)
        k           =   k4d
        d           =   mean(k4d)
        # Overbought status
        if k > 80 and d > 80 and k < d:
            stoch_status    =   "oversold"

        # Oversold status   
        elif k < 20 and d < 20 and k > d:
            stoch_status    =   "overbought"

        # Something in the middle
        else:
            stoch_status    =   "neutral"
        
        boll_band = talib.bbands(my_close, timeperiod=20, nbdevup=2, nbdevdn=2, matype=0)
        typical_price       =   (my_high+my_low+my_close)/3
        n   =period
        m   =2
        o   =m/n(typical_price)
        upper_band_val  =   ma_close_val(typical_price,n) + m * o
        lower_band_val  =   ma_close_val(typical_price,n) - m * o
        #INSERT PREFFERED REVERSAL PATTERN(S) OF THE AT LEAST 50(FIFTY) AVAILABLE
        old_reliable    =   talib.Doji(my_high, my_low, my_open, my_close)

#BET PLACEMENT CONDITIONS AND BET PLACEMENT
def place_option(Money,goal,expirations_mode):  
    
    set_values()

    #CALL OPTION
    if close_val or low_val<lower_band_val and stoch_status=="oversold":
        check,id    =   Iq.buy(Money,goal,"call",expirations_mode)
        if check:
            print("'CALL' Option  Placed Successfully.")
        else:
            print("'CALL' Option failed.")
    #PUT OPTION
    elif close_val or high_val>upper_band_val and stoch_status=="overbought":
        check,id=Iq.buy(Money,goal,"put",expirations_mode)
        if check:
            print("'PUT' Option  Placed Successfully.")
        else:
            print("'PUT' Option failed.")
#--END

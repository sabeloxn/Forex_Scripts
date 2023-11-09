#--
import numpy as np 
#--
import sys
import time as t
import time
from datetime import datetime
import datetime as dt
#IMPORT IQ OPTIONS API

#from iqoptionapi.api import IQOptionAPI 
from iqoptionapi.stable_api import IQ_Option
import getpass
import threading
#--
total_loss=0
total_win=0
Profit=0
loss=0
loss_streak=0

#LOG IN TO TRADE ACCOUNT
my_user = "teyasabelo@gmail.com"    #input("Username :") #YOUR IQOPTION USERNAME "teyasabelo@gmail.com"#
my_pass = "wzt79iczg.*Z.Z9"         #getpass.getpass("Password:") #YOUR IQOTION PASSWORD "nhrFtXqQw@QCHi7"#
#CONNECT ==>:

Iq=IQ_Option(my_user,my_pass)
old_now  =  datetime.now()
old_str_now=old_now.strftime("%D %H:%M:%S")
iqch1,iqch2 = Iq.connect()
if iqch1==True:
    print("\nStart: %s"%old_str_now)
else:
    print("Log In Failed.")
#--
now  = dt.datetime.fromtimestamp(  Iq.get_server_timestamp())
str_now=now.strftime("%D %H:%M:%S")

balance_type="PRACTICE"#input("Account Type: ")
if balance_type == 'REAL':
    Iq.change_balance(balance_type)
    
starting_blc=Iq.get_balance()
print("Starting balance: "+str(starting_blc))
start_amount=1
option_amount=start_amount
multiplier=3

#SET UP TRADE PARAMETERS 
Money               =   round(option_amount,2)           #AMOUNT IN CURRENCY FOR OPTION
goal                =   "EURUSD"#input("Instrument: ")                          #Target Instrument
size                =   60#int(input("Size: ") )                             #Timeframe In Seconds=[1,5,10,15,30,60,120,300,600,900,1800,3600,7200,14400,28800,43200,86400,604800,2592000,"all"]
maxdict             =   20#int(input("Max Dict: "))                                #Number of Bars
expirations_mode    =   1#int(input("Expiration: "))                              #Option Expiration Time in Minutes
#GET CANDLES
Iq.start_candles_stream(goal,size,maxdict)
#Set time variables
current_time = now.strftime("%H:%M:%S")                                 #CUSTOM FORMAT WITOUT THE DAY
start_time = now.replace(hour=7, minute=23, second=0)                  #TIME TO START TRADING
end_time = now.replace(hour=21, minute=45, second=0)                    #TIME TO STOP TRADING
session_close = end_time.strftime("%H:%M:%S")                           #CUSTOM FORMAT WITOUT THE DAY

#Get real time candles
cc=Iq.get_realtime_candles(goal,size)
skipped_bars=0
my_open = []
my_high = []
my_low  = []
my_close =[]
#--------------------------
place_at  = 0#int(input("Place at:  "))
def get_purchase_time():
    remaning_time=Iq.get_remaning(expirations_mode)   
    purchase_time=remaning_time
    return purchase_time

def get_expiration_time():
    exp=Iq.get_server_timestamp()
    # exp2 = time.time()
    # print(exp)
    # print(exp2)
    time_to_buy=(exp % size)
    return int(time_to_buy)

def expiration_thread():
    global place_at
    while True:
        #print(get_expiration_time())
        x=get_expiration_time()
        time.sleep(1)
        if x == place_at:
            place_option(Money,goal,expirations_mode)
threading.Thread(target=expiration_thread).start()
#-----------------
i=0
def count_trade():
    global i
    i+=1
    return i
#----------------------------------------------
def place_option(Money,goal,expirations_mode):  
    count_trade()
    get_time()
    get_prev_bar_direction()
    skip_bars()

#CALL OPTION
    if (up) and skip_bar==False:
        check,id=Iq.buy(Money,goal,"call",expirations_mode)
        if check:
            print("%s. %s  CALL $%s : (Open: %s || Close: %s ) "%(i,current_time,Money,open_val,close_val))
            print("Awaiting 'CALL' Option Result, Please wait...")

            option_result=round(Iq.check_win_v3(id),2)
#MARTINGALE
            martingale(option_result)
        else:
            print("'CALL' Option failed.")
#PUT OPTION
    elif (down) and skip_bar==False:
        check,id=Iq.buy(Money,goal,"put",expirations_mode)
        if check:
            print("%s. %s  PUT $%s : (Open: %s || Close: %s ) "%(i,current_time,Money,open_val,close_val))
            print("Awaiting 'PUT' Option Result, Please wait...")

            option_result=round(Iq.check_win_v3(id),2)
#MARTINGALE
            martingale(option_result)
        else:
            print("'PUT' Option failed.")
    else :
        print("%s. %s Trade Conditions Not Met. NO OPTION PLACED. (Open: %s || Close: %s ) "%(i,current_time,open_val,close_val))
        t.sleep(size)
    print("\n")
#--END

def get_prev_bar_direction():
    global  up
    global  down

    global open_val
    global high_val
    global low_val
    global close_val

    global open_size
    global high_size
    global low_size
    global close_size

    for k in list(cc.keys()):
        open=(cc[k]['open'])
        high=(cc[k]['max'])
        low=(cc[k]['min'])
        close=(cc[k]['open'])

        my_open.append(open)
        open_size=len(my_open)
        open_val=my_open[open_size-2]

        my_high.append(high)
        high_size=len(my_high)
        high_val=my_high[high_size-2]

        my_low.append(low)
        low_size=len(my_low)
        low_val=my_low[low_size-2]

        my_close.append(close)
        close_size=len(my_close)
        close_val=my_close[close_size-1]

    if abs(my_high[14] - open_val) < abs(my_low[14] - open_val):
        if (open_val > my_low[10]):
            up = True
            down = False

    elif abs(my_high[14] - open_val) > abs(my_low[14] - open_val):
        if (open_val < my_high[10]):
            down = True
            up  = False

def martingale(option_result):
    global Money
    global total_win
    global total_loss
    global loss
    global Profit
    global loss_streak
    global option_amount
    global skipped_bars
    #Win
    if option_result > 0.0:                    
        total_win+=1
        loss_streak=0
        win_amount=option_result
        Profit+=win_amount
        print("Option Result: +%s"%option_result)
        option_amount=start_amount
        Money=round(option_amount,2)
    #Lose
    elif option_result < 0.0:
        #Loss Streak Count
        total_loss+=1
        if loss_streak>4:
            loss_streak=0
            print("Loss streak was reset.")
        loss_streak+=1
        print("Option Result: %s"%option_result)
        loss_amount= option_amount
        loss+=loss_amount
        print("Loss streak: %s"%loss_streak)

        if option_amount==option_amount:
            option_amount=round(option_amount*multiplier,2)
            print("%s. Option amount = %s"%(loss_streak,option_amount))
            option_amount=option_amount
            Money=round(option_amount,2)

def skip_bars():
    global skip_bar
    global skipped_bars
    global Money
    global loss_streak
    global option_amount
    get_time()
    if loss_streak==3 or loss_streak==4:
        skip_bar = True
        print("%s. %s Skipping current bar due to loss streak."%(i,current_time))
        option_amount=start_amount #--Starts at $1
        Money=round(option_amount,2)
        #trade_countdown(get_purchase_time())
        skipped_bars+=1
        if skipped_bars==2:
            loss_streak=0
            skipped_bars=0
    else:
        skip_bar=False
    return skip_bar

def get_time():
    global current_time
    now = datetime.now()                                                    #WILL BE USED TO GET OUR OPTION PLACEMENT TIME
    current_time = now.strftime("%H:%M:%S")
    return current_time
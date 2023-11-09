import tkinter as tk
from tkinter import ttk
from tkinter.ttk import *
from tkinter import *
from tkinter import filedialog
#--
import numpy as np 
import pandas as pd 
#--
import sys
import threading
import time as t
from datetime import datetime
import datetime as dt
#IMPORT IQ OPTIONS API
from iqoptionapi.stable_api import IQ_Option
#--
#START--------------------------------------
main_window = tk.Tk()
main_window.title("IQ OPTIONS TRADING BOT")
# main_window.wm_iconphoto
# declaring string variable
# for storing input variables
name_var            =tk.StringVar()
passw_var           =tk.StringVar()
instrument_var      =tk.StringVar()
timeframe_var       =tk.IntVar()
start_amount_var   =tk.IntVar()
martingale_var      =tk.IntVar()
no_of_bars_var      =tk.IntVar()
expiration_var      =tk.IntVar()

Label(main_window, text="Username: ").grid(row=1, column=1)
user_name_entry=Entry(main_window,textvariable = name_var,width=25, borderwidth=1).grid(row=1, column=2)

Label(main_window, text="Password: ").grid(row=1, column=3)
password_entry=Entry(main_window,textvariable = passw_var, show = '*', width=25, borderwidth=1).grid(row=1, column=4)

Label(main_window, text="Login Status: ").grid(row=2, column=1)
status_label=Label(main_window, text="Waiting...")
status_label.grid(row=2, column=2) #log in status result

Label(main_window, text="Balance: ").grid(row=2, column=3)
blc_label=Label(main_window, text="Pending... ")
blc_label.grid(row=2, column=4)

Label(main_window, text="Instrument: ").grid(row=3, column=1)
target_instrument_entry= Entry(main_window,textvariable = instrument_var, width=5, borderwidth=1)
target_instrument_entry.grid(row=3, column=2)

Label(main_window, text="TimeFrame: ").grid(row=3, column=3)
instrument_tf_entry= Entry(main_window,textvariable = timeframe_var, width=5, borderwidth=1)
instrument_tf_entry.grid(row=3, column=4)

Label(main_window, text="Bars: ").grid(row=4, column=1)
option_amount_entry=Entry(main_window,textvariable = no_of_bars_var, width=5, borderwidth=1)
option_amount_entry.grid(row=4, column=2)

Label(main_window, text="Exp. Time: ").grid(row=4, column=3)
option_amount_entry=Entry(main_window,textvariable = expiration_var, width=5, borderwidth=1)
option_amount_entry.grid(row=4, column=4)

Label(main_window, text="Amount: ").grid(row=5, column=1)
option_amount_entry=Entry(main_window,textvariable = start_amount_var, width=5, borderwidth=1)
option_amount_entry.grid(row=5, column=2)

Label(main_window, text="MartinGale: ").grid(row=5, column=3)
mg_multiplier_entry=Entry(main_window,textvariable =martingale_var, width=5, borderwidth=1)
mg_multiplier_entry.grid(row=5, column=4)

Label(main_window, text="Option Time Remaining: ").grid(row=7, column=1)
rem_time_label=Label(main_window, text="00:00")
rem_time_label.grid(row=7, column=2)

Label(main_window, text="Trading Stop: ").grid(row=7, column=3)
stop_time_label=Label(main_window, text="00:00")
stop_time_label.grid(row=7, column=4)

Label(main_window, text="Previous Option: ").grid(row=8, column=1)
prev_option_label=Label(main_window, text="Put/Call")
prev_option_label.grid(row=8, column=2)

Label(main_window, text="Result: ").grid(row=8, column=3)
result_label=Label(main_window, text="P\L")
result_label.grid(row=8, column=4)

frame1 = Frame(main_window)
frame1.grid(row=9,columnspan=5, sticky=E+W)
listbox = Listbox(frame1, height = 10, 
                  width = 100, 
                  bg = "black",
                  activestyle = 'dotbox',
                  font=("Helvetica", 8),
                  fg = "white")
listbox.grid(row=0, column=0)

instrument=""
instrument_tf=0
option_amount=0
maxdict=0
expiration_time=0
martingale=0

def login_on_click():
    #LOG IN TO TRADE ACCOUNT
    global my_user              #YOUR IQOPTION USERNAME
    my_user = name_var.get()
    global my_pass              #YOUR IQOTION PASSWORD
    my_pass = passw_var.get()

    #CONNECT ==>:
    #global Iq
    Iq=IQ_Option(my_user,my_pass)
    iqch1,iqch2 = Iq.connect()
    if iqch1==True:
        status_label.config(text = "Logged In")
    else:
        status_label.config(text = "Log in Failed.")
    name_var    .set("")
    passw_var   .set("")

    my_blc = Iq.get_balance()
    blc_label.config(text = my_blc)
    #--

#def start_auto_trading():
    #SET UP TRADE PARAMETERS 
    global instrument
    instrument              =instrument_var.get()                   #Target Instrument
    global instrument_tf           
    instrument_tf           =timeframe_var.get()                    #Timeframe In Seconds=[1,5,10,15,30,60,120,300,600,900,1800,3600,7200,14400,28800,43200,86400,604800,2592000,"all"]
    global number_of_bars          
    number_of_bars          =no_of_bars_var.get()                   #Number of Bars
    global expiration_time         
    expiration_time         =expiration_var.get()                   #Option Expiration Time in Minutes
    global option_amount          
    option_amount           =start_amount_var.get()                #AMOUNT IN CURRENCY FOR OPTION
    global martingale              
    martingale              =martingale_var.get()                   #Martingale Multiplier

    #print(my_user,my_pass,"Goal: "+instrument,"Size: "+str(size,option_amount),"MaxDict: "+str(number_of_bars),expiration_time,martingale,sep="\n")
    global parameter_list
    parameter_list=["%s \n"%instrument,"%s \n"%instrument_tf,"%s \n"%number_of_bars,"%s \n"%expiration_time,"%s \n"%option_amount,"%s \n"%martingale] 
    #GET REAL TIME CANDLES
    #Iq=IQ_Option(my_user,my_pass)
    #cc=Iq.get_realtime_candles(instrument,60)
    now=datetime.now()
    #WILL BE USED TO GET OUR OPTION PLACEMENT TIME
    current_time= now.strftime("%H:%M:%S")
    #CUSTOM FORMAT WITHOUT THE DAY
    #add_to_list("Current time: = %s"%current_time)
    #GET CANDLES
    add_to_list("%s : Start candle stream/..."%current_time)
    Iq.start_candles_stream(instrument,instrument_tf,number_of_bars)
    #LET'S DO SOMETHING
    add_to_list("%s : Bot started/..."%current_time)

    end_time=now.replace(hour=23,minute=59,second=00,microsecond=00)    #TIME TO STOP TRADING

    #GET REAL TIME CANDLES
    cc=Iq.get_realtime_candles(instrument,instrument_tf)

    #PLACING AN OPTION
    remaining_time=Iq.get_remaning(expiration_time)                     #GET THE REMAINING TIME FOR THE CURRENT BAR
    purchase_time = remaining_time                                      #PURCHASE IN 'X' SECONDS i.e('remaining_time-30'= 30 seconds before current bar close)

    #WAIT FOR CURRENT BAR TO CLOSE
    for i in range(purchase_time,0,-1):
        rem_time_label.config(text=i)
        t.sleep(1)

    while now<end_time:                                                    
        now = datetime.now()
        current_time = now.strftime("%H:%M:%S")
        #add_to_list("Current Time = %s"%current_time )

        cc=Iq.get_realtime_candles(instrument,instrument_tf)

        for k in list( cc.keys()):
            open=(cc[k]['open'])
            close=(cc[k]['close'])

            print("Open: ",open,"|| Close: ", close)

#CALL OPTION
        if close>open:
           check,id=Iq.buy(option_amount,instrument,"call",expiration_time)
           if check:               
               add_to_list("%s - CALL: Open: %s || Close: %s"%(current_time,open,close))
               add_to_list("Awaiting 'CALL' Option Result, Please wait...")
               print(Iq.check_win_v3(id))
    #MARTINGALE
               if 'win' in Iq.check_win_v3(id):                    
                option_amount=start_amount_var
               else:
                    option_amount= option_amount*3
           else:
               print("'CALL' Option failed.")
#PUT OPTION
        if close<open:
            check,id=Iq.buy(option_amount,instrument,"put",expiration_time)
            if check:
                add_to_list("%s - PUT: Open: %s || Close: %s"%(current_time,open,close))
                add_to_list("Awaiting 'PUT' Option Result, Please wait...")
                print(Iq.check_win_v3(id))
    #MARTINGALE
                if 'win' in Iq.check_win_v3(id):                    
                    option_amount=start_amount_var
                else:
                    option_amount= option_amount*3
            else:
                print("'PUT' Option failed.")
#--
def add_to_list(item):
    num_of_items=listbox.size()
    listbox.insert(num_of_items+1, item)

def save_presets():
    mystring=instrument+str(instrument_tf)
    file_name="%s" %mystring
    print("Writing to text file.")
    text_file = open("%s.txt"%file_name, "a")
    text_file = open(r"C:\Users\RB LUTHERAN CHURCH\Desktop\Sabelo\Scripts tmp\%s.txt"%file_name,"w+")
    text_file.writelines(parameter_list)
    text_file.close()

def openfilename():
    filename = filedialog.askopenfilename(title ='Open',initialdir="C:/Users/RB LUTHERAN CHURCH/Desktop/Sabelo/Scripts tmp")
    return filename

def load_presets():
    x=openfilename()
    file1 = open(x,"r+") 
    content = file1.readlines()

    p1=content[0]
    instrument_var      .set(p1.strip())
    p2=content[1]
    timeframe_var       .set(p2)
    p3=content[2]
    no_of_bars_var      .set(p3)
    p4=content[3]
    expiration_var      .set(p4)
    p5=content[4]
    start_amount_var    .set(p5)
    p6=content[5]
    martingale_var      .set(p6)


def stop_auto_trading():
    exit()

Button(main_window, text="Start",           command=threading.Thread(target=login_on_click).start       ).grid(row=6, column=1)
Button(main_window, text="Stop",            command=threading.Thread(target=stop_auto_trading).start    ).grid(row=6, column=2)
Button(main_window, text="Save Presets",    command=save_presets    ).grid(row=6, column=3)
Button(main_window, text="Load Presets",    command=load_presets    ).grid(row=6, column=4)

main_window.mainloop()
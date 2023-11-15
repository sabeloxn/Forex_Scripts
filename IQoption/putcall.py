from iqoptionapi.api import IQOptionAPI
import logging
from iqoptionapi.stable_api import IQ_Option
import time as t
from datetime import datetime 
import datetime as dt
import os
#from pynput.mouse import Button, Controller
#--
#mouse=Controller()
#PLATFORM CREDENTIALS
my_user =""         #input("Username: ")
my_pass =""            #input("Password: ")

#LOG IN TO TRADE ACCOUNT
Iq=IQ_Option(my_user,my_pass)
iqch1,iqch2=Iq.connect()
if iqch1==True:
    print("Log In Successful.")
else:
    print("Log In failed.")

my_blc=Iq.get_balance()
print(f"Balance: {my_blc} ")

#SET TRADE PARAMETERS
Money=10                        #AMOUNT PER OPTION
goal =  "EURUSD"            #TARGET INSTRUMENT"APPLE" "APPLE"
size= 60                        #60 second bars(TIMEFRAME IN SECONDS)
maxdict=1                       #NUMBER OF BARS TO GET
expirations_mode=1              #EXPIRATION TIME IN MINUTES

#GET CANDLES
print("Start candle stream/...")
Iq.start_candles_stream(goal,size,maxdict)

#LET'S DO SOMETHING
print("Bot started ...")

now=datetime.now()
#WILL BE USED TO GET OUR OPTION PLACEMENT TIME
current_time= now.strftime("%H:%M:%S")
#CUSTOM FORMAT WITHOUT THE DAY
print("Current time: ",current_time)

end_time=now.replace(hour=23,minute=59,second=00,microsecond=00)    #TIME TO STOP TRADING

#GET REAL TIME CANDLES
cc=Iq.get_realtime_candles(goal,size)

#PLACING AN OPTION
remaining_time=Iq.get_remaning(expirations_mode)        #GET THE REMAINING TIME FOR THE CURRENT BAR
purchase_time = remaining_time                          #PURCHASE IN 'X' SECONDS i.e('remaining_time-30'= 30 seconds before current bar close)

#WAIT FOR CURRENT BAR TO CLOSE
for i in range(purchase_time,0,-1):
    print(f"{i}",end="\r",flush=True)
    t.sleep(1)

#PLACE ORDERS
while now < end_time:
    # now=datetime.now()
    # current_time= now.strftime("%H:%M:%S")
    # print("Current Time: ", current_time)

    for k in list(cc.keys()):
        open=(cc[k]['open'])
        close=(cc[k]['close'])
        print("Open: ",open,"|| Close: ", close)

        #CALL OPTION
        if close>open:
            print("Green")

            check,id=Iq.buy(Money,goal,"call",expirations_mode)
            # Set pointer position
            # mouse.position = (1309, 361)
            # #print('Now we have moved it to {0}'.format(
            #    # mouse.position))

            # # Press and release
            # mouse.press(Button.left)
            # mouse.release(Button.left)
            if check:
                print("'CALL' Option Placed.")
                print("Awaiting 'Call' Option Result...")
                #FUNCTION TO GET OPTION RESULT
                
                print(Iq.check_win_v3(id))
                if Iq.check_win_v3(id) > 0.0:
                    Money=10
                else:
                    Money=Money*3
            else:
                print("'Call' Option Failed.")
        else:
            #PUT OPTION
            print("Red")
            check,id=Iq.buy(Money,goal,"put",expirations_mode)
            # Set pointer position
            # mouse.position = (1317, 449)
            # #print('Now we have moved it to {0}'.format(
            #    # mouse.position))

            # # Press and release
            # mouse.press(Button.left)
            # mouse.release(Button.left)
            if check:
                print("'PUT' Option Placed.")
                print("Awaiting 'Put' Option Result...")
                #FUNCTION TO GET OPTION RESULT
                
                print(Iq.check_win_v3(id))
                if Iq.check_win_v3(id) > 0.0:
                    Money=10
                else:
                    Money=Money*3
            else:
                print("'Put' Option Failed.")
#END--



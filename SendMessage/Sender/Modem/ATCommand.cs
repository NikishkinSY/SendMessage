using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SendMessage
{
    public class ATCommand
    {
        public string ATRequest { get; set; }
        public ATResponse ATResponse { get; set; }
        public int TimesToRepeat { get; set; }
        public TimeSpan WaitForAnswer { get; set; }
        public bool LineBreak { get; set; }
        public TimeSpan DelayBetweenRepeats { get; set; }
        
        public ATCommand(string atRequest, ATResponse atResponse, TimeSpan waitForAnswer, int timesToRepeat)
        {
            this.Init();
            this.ATRequest = atRequest;
            this.ATResponse = atResponse;
            this.WaitForAnswer = waitForAnswer;
            this.TimesToRepeat = timesToRepeat;
        }

        public void Init()
        {
            this.ATRequest = "at";
            this.ATResponse = ATResponse.OK;
            this.TimesToRepeat = 1;
            this.WaitForAnswer = new TimeSpan(0, 0, 20);
            this.DelayBetweenRepeats = new TimeSpan(0, 0, 0);
            this.LineBreak = true;
        }

        #region AT Commands
        public static Queue<ATCommand> SMSMessage(string receiver, string messsage, TimeSpan waitForATCommand, int timesToRepeat)
        {
            Queue<ATCommand> atCommands = new Queue<ATCommand>();

            string atRequestSMS = String.Format("AT+CMGS={0}{1}", receiver, Environment.NewLine);
            ATCommand atCommandRequestSMS = new ATCommand(atRequestSMS, ATResponse.SMS, waitForATCommand, timesToRepeat);
            atCommands.Enqueue(atCommandRequestSMS);

            string atSendSMS = String.Format("{0}{1}", messsage, (char)26);
            ATCommand atCommandSendSMS = new ATCommand(atSendSMS, ATResponse.OK, waitForATCommand, timesToRepeat);
            atCommands.Enqueue(atCommandSendSMS);

            return atCommands;
        }
        public static ATCommand MessageMode(TimeSpan waitForATCommand, int timesToRepeat)
        {
            string atRequest = String.Format("AT+CMGF=1{0}", Environment.NewLine);
            return new ATCommand(atRequest, ATResponse.OK, waitForATCommand, timesToRepeat);
        }
        public static ATCommand Ping(TimeSpan waitForATCommand, int timesToRepeat)
        {
            string atRequest = String.Format("AT{0}", Environment.NewLine);
            return new ATCommand(atRequest, ATResponse.OK, waitForATCommand, timesToRepeat);
        }
        public static ATCommand ATResetModem(TimeSpan waitForATCommand, int timesToRepeat)
        {
            return new ATCommand("AT+CFUN=1,1", ATResponse.SYSSTART, waitForATCommand, timesToRepeat);
        }
        public static ATCommand ATResetSettings(TimeSpan waitForATCommand, int timesToRepeat)
        {
            return new ATCommand("AT&F", ATResponse.OK, waitForATCommand, timesToRepeat);
        }
        
        #endregion

        public override string ToString()
        {
            return String.Format("Request:\"{0}\", ExpResponse:\"{1}\"", ATRequest, ATResponse);
        }
    }

    public enum ATResponse
    {
        OK,
        CONNECT,
        RING,
        NO_CARRIER,
        ERROR,
        NO_DIALTONE,
        BUSY,
        NO_ANSWER,
        PLUSPLUSPLUS,
        SYSSTART,
        SMS,
        UNKNOWN
    }

    public enum StatusATCommand
    {
        Done,
        Wait,
        Abort,
        None
    }
}

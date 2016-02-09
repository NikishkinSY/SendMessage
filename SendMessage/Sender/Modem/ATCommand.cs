using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SendMessage
{
    public class ATCommand
    {
        public string ATRequest { get; set; }
        public ATResponse ExpectedATResponse { get; set; }
        public int TimesToRepeat { get; set; }
        public TimeSpan WaitForResponse { get; set; }
        public bool LineBreak { get; set; }
        public TimeSpan DelayBetweenRepeats { get; set; }
        public List<ATCommand> NestedATCommands { get; set; }

        #region help properties
        public bool ContainNestedATCommands { get { return NestedATCommands.Count > 0; } }
        #endregion

        public ATCommand()
        {
            this.Init();
        }

        public ATCommand(string atRequest, ATResponse atResponse, TimeSpan waitForAnswer, int timesToRepeat)
        {
            this.Init();
            this.ATRequest = atRequest;
            this.ExpectedATResponse = atResponse;
            this.WaitForResponse = waitForAnswer;
            this.TimesToRepeat = timesToRepeat;
        }

        public void Init()
        {
            this.ATRequest = "at";
            this.ExpectedATResponse = ATResponse.OK;
            this.TimesToRepeat = 1;
            this.WaitForResponse = new TimeSpan(0, 0, 20);
            this.DelayBetweenRepeats = new TimeSpan(0, 0, 0);
            this.LineBreak = true;
            this.NestedATCommands = new List<ATCommand>();
        }

        #region AT Commands
        public static ATCommand SMSMessage(string receiver, string messsage, TimeSpan waitForATCommand, int timesToRepeat)
        {
            string atRequestSMS = String.Format("AT+CMGS={0}", receiver);
            ATCommand atCommandRequestSMS = new ATCommand(atRequestSMS, ATResponse.SMS, waitForATCommand, timesToRepeat);

            string atSendSMS = String.Format("{0}{1}", messsage, (char)26);
            ATCommand atCommandSendSMS = new ATCommand(atSendSMS, ATResponse.OK, waitForATCommand, timesToRepeat);

            atCommandRequestSMS.NestedATCommands.Add(atCommandSendSMS);

            return atCommandRequestSMS;
        }
        public static ATCommand MessageMode(TimeSpan waitForATCommand, int timesToRepeat)
        {
            return new ATCommand("AT+CMGF=1", ATResponse.OK, waitForATCommand, timesToRepeat);
        }
        public static ATCommand Ping()
        {
            return new ATCommand("AT", ATResponse.OK, new TimeSpan(0,0,2), 2);
        }
        public static ATCommand ATResetModem()
        {
            return new ATCommand("AT+CFUN=1,1", ATResponse.SYSSTART, new TimeSpan(0,1,0), 1);
        }
        public static ATCommand ATResetSettings(TimeSpan waitForATCommand, int timesToRepeat)
        {
            return new ATCommand("AT&F", ATResponse.OK, waitForATCommand, timesToRepeat);
        }
        
        #endregion

        public override string ToString()
        {
            return String.Format("\"{0}\" \"{1}\"", ATRequest, ExpectedATResponse);
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

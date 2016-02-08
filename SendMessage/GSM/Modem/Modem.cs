using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SendMessage
{
    internal class Modem: Sender
    {
        public override string Name { get { return COMPort.ToString(); } }
        
        #region Status
        internal bool IsOpen { get { return COMPort != null && COMPort.IsOpen; } }
        internal bool IsFree { get { return IsOpen && !Initializing && currentATCommandStatus != StatusATCommand.Wait; } }
        #endregion

        //токен для асинхронного открытия порта
        bool cancellationTokenOpenAsync = false;
        //идет инициализация
        bool Initializing = false;
        //событие всех действий
        internal event EventHandler<SendMessageEventArgs> OnEvent;
        void RiseEvent(object sender, SendMessageEventArgs e)
        { if (OnEvent != null) OnEvent(sender, e); }

        #region Config
        COMPort comPort = new COMPort();
        public COMPort COMPort
        {
            get { return comPort; }
            set
            {
                comPort.PortName = value.PortName;
                comPort.BaudRate = value.BaudRate;
                comPort.Parity = value.Parity;
                comPort.DataBits = value.DataBits;
                comPort.StopBits = value.StopBits;
                comPort.Encoding = value.Encoding;
            }
        }
        
        public TimeSpan PauseBetweenOpen { get; set; }
        public string InitATCommands { get; set; }
        public TimeSpan WaitForATCommand { get; set; }
        public int TimesToRepeatATCommand { get; set; }
        #endregion

        #region helpers send at-command
        AutoResetEvent waitForATCommandResponse = new AutoResetEvent(false);
        StatusATCommand currentATCommandStatus;
        ATResponse currentATCommandResponse;
        ATResponse expectedATCommandResponse;
        #endregion

        #region locking Objects
        object lockSendATCommand = new object();
        object lockOpenAsync = new object();
        object lockOpenClose = new object();
        #endregion


        internal Modem()
        {
            this.COMPort.OnDataReceived += GSMModem_OnDataReceived;

            this.PauseBetweenOpen = new TimeSpan(0, 0, 10);
            this.InitATCommands = string.Empty;
            this.WaitForATCommand = new TimeSpan(0, 0, 20);
            this.TimesToRepeatATCommand = 1;
        }

        public override bool SendMessage(Notice notice)
        {
            //string UTF16Message = RussiaLanguage.ConvertRusToUCS2(notice.Message);
            SendATCommands(ATCommand.SMSMessage(notice.Receiver.Address, notice.Message, this.WaitForATCommand, this.TimesToRepeatATCommand));
            RiseEvent(this, new SendMessageEventArgs(EventType.Tx, MessageType.SMS, "message sent", notice));
            return true;
        }
        public override void Start()
        {
            OpenPeriodicAsync();
        }
        public override bool Stop()
        {
            return Close();
        }

        bool Init(bool WithReboot)
        {
            Initializing = true;
            Queue<ATCommand> atCommands = new Queue<ATCommand>();
            if (WithReboot)
                atCommands.Enqueue(ATCommand.ATResetSettings(WaitForATCommand, TimesToRepeatATCommand));
            
            foreach (string command in InitATCommands.Split(';'))
                if (!string.IsNullOrEmpty(command))
                    atCommands.Enqueue(new ATCommand(command, ATResponse.OK, this.WaitForATCommand, this.TimesToRepeatATCommand));

            SendATCommands(atCommands);
            Initializing = false;
            RiseEvent(this, new SendMessageEventArgs(EventType.Sys, MessageType.Note, "modem initialized"));
            return true;
        }

        #region Open
        bool Open()
        {
            try
            {
                COMPort.Open();
                Init(true);
                RiseEvent(this, new SendMessageEventArgs(EventType.Sys, MessageType.Note, "port opened"));
                return true;
            }
            catch (Exception ex)
            {
                RiseEvent(this, new SendMessageEventArgs(EventType.Sys, MessageType.Error, "failed to open the port", ex));
                return false;
            }
        }
        /// <summary>
        /// попытка синхронного открытия порта
        /// </summary>
        /// <returns></returns>
        bool OpenPeriodic()
        {
            lock (lockOpenClose)
            {
                cancellationTokenOpenAsync = false;
                while (!Open())
                {
                    Thread.Sleep(PauseBetweenOpen);
                    if (cancellationTokenOpenAsync)
                        return false;
                }
                return true;
            }
        }
        /// <summary>
        /// попытка асинхронного открытия порта
        /// </summary>
        void OpenPeriodicAsync()
        {
            lock (lockOpenAsync)
            {
                new Task(() =>
                {
                    OpenPeriodic();
                }).Start();
            }
        }
        #endregion

        #region Close
        bool Close()
        {
            try
            {
                //stop task open port
                if (!cancellationTokenOpenAsync)
                    cancellationTokenOpenAsync = true;

                lock (lockOpenClose)
                {
                    COMPort.Close();
                }
                RiseEvent(this, new SendMessageEventArgs(EventType.Sys, MessageType.Note, "port closed"));
                return true;
            }
            catch (Exception ex)
            {
                RiseEvent(this, new SendMessageEventArgs(EventType.Sys, MessageType.Error, "failed to close the port", ex));
                return false;
            }
        }

        void CloseAsync()
        {
            new Task(() =>
            {
                Close();
            }).Start();
        }
        #endregion
        private void GSMModem_OnDataReceived(object sender, DataEventArgs e)
        {
            if (e is DataEventArgs)
            {
                byte[] data = (e as DataEventArgs).GetData();
                IEnumerable<byte[]> ReceiveBytes = COMPort.BytesSplit(data, (byte)'\r', (byte)'\n');
                foreach (byte[] Bytes in ReceiveBytes)
                {
                    ReceiveData receiveData = RecognizeIncomingData(data);
                    if (receiveData is ReceiveATResponse)
                    {
                        currentATCommandResponse = (receiveData as ReceiveATResponse).ATResponse;

                        if (currentATCommandResponse != ATResponse.UNKNOWN && currentATCommandResponse == expectedATCommandResponse)
                            StopATCommand(true);

                        if (currentATCommandResponse == ATResponse.NO_ANSWER ||
                            currentATCommandResponse == ATResponse.NO_CARRIER ||
                            currentATCommandResponse == ATResponse.ERROR ||
                            currentATCommandResponse == ATResponse.BUSY ||
                            currentATCommandResponse == ATResponse.NO_DIALTONE)
                        {
                            StopATCommand(false);
                        }

                        RiseEvent(this, new SendMessageEventArgs(EventType.Rx, MessageType.AT, "incoming at-command", (receiveData as ReceiveATResponse).ATResponse));
                    }
                    else if (receiveData is ReceiveData)
                    {
                        RiseEvent(this, new SendMessageEventArgs(EventType.Rx, MessageType.Data, "other data", receiveData.String));
                    }
                }
            }
        }

        ReceiveData RecognizeIncomingData(byte[] IncomingBytes)
        {
            ReceiveData RecData = null;
            if (IncomingBytes is byte[] && IncomingBytes.Length > 0)
            {
                string IncomingString = Encoding.UTF8.GetString(IncomingBytes).ToLower();
                //обнуляем пришедшую АТ-команду
                ATResponse atResponse = ATResponse.UNKNOWN;

                if (IncomingString.Contains("ok"))
                    atResponse = ATResponse.OK;
                else if(IncomingString.Contains(">"))
                    atResponse = ATResponse.SMS;
                else if (IncomingString.Contains("connect"))
                    atResponse = ATResponse.CONNECT;
                else if (IncomingString.Contains("ring"))
                    atResponse = ATResponse.RING;
                else if (IncomingString.Contains("no carrier"))
                    atResponse = ATResponse.NO_CARRIER;
                else if (IncomingString.Contains("error"))
                    atResponse = ATResponse.ERROR;
                else if (IncomingString.Contains("no dialton"))
                    atResponse = ATResponse.NO_DIALTONE;
                else if (IncomingString.Contains("busy"))
                    atResponse = ATResponse.BUSY;
                else if (IncomingString.Contains("no answer"))
                    atResponse = ATResponse.NO_ANSWER;

                if (atResponse == ATResponse.UNKNOWN)
                {
                    if (IncomingString.Contains("+++"))
                    {
                        //atResponse = ATResponse.PLUSPLUSPLUS;
                    }
                    else if (IncomingString.Contains("^sysstart"))
                    {
                        //atResponse = ATResponse.SYSSTART;
                        Init(false);
                    }
                }

                if (atResponse != ATResponse.UNKNOWN)
                    RecData = new ReceiveATResponse(IncomingBytes, atResponse);
                else
                    RecData = new ReceiveData(IncomingBytes);
            }
            return RecData;
        }

        bool SendATCommands(Queue<ATCommand> atCommands)
        {
            while (atCommands.Count > 0)
            {
                ATCommand atCommand = atCommands.Dequeue();
                bool result = SendATCommand(atCommand);
                if (!result)
                    return false;
            }
            return true;
        }
        bool SendATCommand(ATCommand atCommand)
        {
            lock (lockSendATCommand)
            {
                currentATCommandStatus = StatusATCommand.Wait;
                currentATCommandResponse = ATResponse.UNKNOWN;
                expectedATCommandResponse = atCommand.ATResponse;

                int TimesToRepeat = atCommand.TimesToRepeat;
                bool result = false;
                while (IsOpen && TimesToRepeat-- > 0)
                {
                    COMPort.Write(atCommand.ATRequest, atCommand.LineBreak);
                    RiseEvent(this, new SendMessageEventArgs(EventType.Tx, MessageType.AT, "send at-command", atCommand));
                    //Ожидаем ответа
                    //ожидаем таймаута или события о получении верного ответа из метода Event_Note (выше)
                    waitForATCommandResponse.Reset();
                    waitForATCommandResponse.WaitOne(atCommand.WaitForAnswer);
                    if (currentATCommandStatus == StatusATCommand.Done)
                    {
                        result = true;
                        break;
                    }
                    else if (currentATCommandStatus == StatusATCommand.Abort)
                    {
                        result = false;
                        break;
                    }

                    if (atCommand.DelayBetweenRepeats.TotalMilliseconds > 0)
                    {
                        //пауза между повторной отправкой команды
                        waitForATCommandResponse.Reset();
                        waitForATCommandResponse.WaitOne(atCommand.DelayBetweenRepeats);
                        if (currentATCommandStatus == StatusATCommand.Done)
                        {
                            result = true;
                            break;
                        }
                        else if (currentATCommandStatus == StatusATCommand.Abort)
                        {
                            result = false;
                            break;
                        }
                    }
                }
                //обнуляем все регистры
                expectedATCommandResponse = ATResponse.UNKNOWN;
                currentATCommandResponse = ATResponse.UNKNOWN;
                currentATCommandStatus = StatusATCommand.None;
                return result;
            }
        }
        void StopATCommand(bool result)
        {
            if (currentATCommandStatus == StatusATCommand.Wait)
            {
                if (result)
                    currentATCommandStatus = StatusATCommand.Done;
                else
                    currentATCommandStatus = StatusATCommand.Abort;
                waitForATCommandResponse.Set();
            }
        }
    }
}

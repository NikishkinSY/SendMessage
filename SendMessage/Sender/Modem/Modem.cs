﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SendMessage
{
    internal class Modem: Sender
    {
        public override string Name { get { return COMPort.ToString(); } }
        
        #region Status
        internal bool IsOpen { get { return COMPort != null && COMPort.IsOpen; } }
        internal bool IsFree { get { return IsOpen && !Initializing && IsEnabled && currentATCommandStatus != StatusATCommand.Wait; } }
        #endregion

        //токен для асинхронного открытия порта
        bool cancellationTokenOpenAsync = false;
        //идет инициализация
        bool Initializing = false;
        //отвечате
        bool IsEnabled = false;
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
        public IEnumerable<ATCommand> InitATCommands { get; set; }
        public TimeSpan WaitForATCommand { get; set; }
        public int TimesToRepeatATCommand { get; set; }
        public TimeSpan PeriodReboot { get; set; }
        public TimeSpan PeriodPing { get; set; }
        #endregion

        #region helpers send at-command
        AutoResetEvent waitForATCommandResponse = new AutoResetEvent(false);
        StatusATCommand currentATCommandStatus;
        ATResponse currentATCommandResponse;
        ATResponse expectedATCommandResponse;
        #endregion

        #region locking Objects
        QueuedLock queuedLockSendATCommand = new QueuedLock();
        object _lockSendATCommand = new object();
        object lockOpenAsync = new object();
        object lockOpenClose = new object();
        #endregion


        internal Modem()
        {
            this.COMPort.OnDataReceived += GSMModem_OnDataReceived;

            this.PauseBetweenOpen = new TimeSpan(0, 0, 10);
            this.InitATCommands = new List<ATCommand>();
            this.WaitForATCommand = new TimeSpan(0, 0, 20);
            this.TimesToRepeatATCommand = 1;
            this.PeriodReboot = new TimeSpan(1, 0, 0, 0);
            this.PeriodPing = new TimeSpan(0, 10, 0);
        }

        public override bool SendMessage(Notice notice)
        {
            //string UTF16Message = RussiaLanguage.ConvertRusToUCS2(notice.Message);
            return SendATCommand(ATCommand.SMSMessage(notice.Contact.PhoneNumber, notice.Message, this.WaitForATCommand, this.TimesToRepeatATCommand));
        }
        public override void Start()
        {
            OpenPeriodicAsync();
            Scheduler.AddPingJob(this, DateTime.Now);
            Scheduler.AddRebootJob(this, DateTime.Now.Add(PeriodReboot));
        }
        public override bool Stop()
        {
            return Close();
        }

        internal bool RebootModem()
        {
            return SendATCommand(ATCommand.ATResetModem());
        }
        internal void PingModem()
        {
            this.IsEnabled = SendATCommand(ATCommand.Ping());
        }

        bool Init(bool WithReboot)
        {
            Initializing = true;
            
            foreach (ATCommand atCommand in InitATCommands)
                SendATCommand(atCommand);
            
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
                    ReceiveData receiveData = RecognizeIncomingData(Bytes);
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
                    else if (IncomingString.Contains("+csca"))
                    {
                        //phone number sms-center
                        //+CSCA: "+79168999100",145
                        //@"\+csca: ""\+(\d{11})"",\d+"
                        Regex rgxPhoneNumberSMSCenter = new Regex(@"\+(\d{11})");
                        Match match = rgxPhoneNumberSMSCenter.Match(IncomingString);
                        if (match.Success)
                        {
                            string test = match.Value;
                        }
                    }
                }

                if (atResponse != ATResponse.UNKNOWN)
                    RecData = new ReceiveATResponse(IncomingBytes, atResponse);
                else
                    RecData = new ReceiveData(IncomingBytes);
            }
            return RecData;
        }

        List<ATCommand> GetATCommands(ATCommand atCommand)
        {
            List<ATCommand> atCommands = new List<ATCommand>();
            if (atCommand != null)
            {
                atCommands.Add(atCommand);
                if (atCommand.ContainNestedATCommands)
                {
                    foreach (ATCommand _atCommand in atCommand.NestedATCommands)
                        atCommands.AddRange(GetATCommands(_atCommand));
                }
            }
            return atCommands;
        }

        internal bool SendATCommand(ATCommand atCommand)
        {
            try
            {
                queuedLockSendATCommand.Enter();

                List<bool> results = new List<bool>();
                foreach (ATCommand _atCommand in GetATCommands(atCommand))
                {
                    bool result = _SendATCommand(_atCommand);
                    if (!result)
                        return false;
                }
                return true;
            }
            finally
            {
                queuedLockSendATCommand.Exit();
            }
        }
        bool _SendATCommand(ATCommand atCommand)
        {
            lock (_lockSendATCommand)
            {
                currentATCommandStatus = StatusATCommand.Wait;
                currentATCommandResponse = ATResponse.UNKNOWN;
                expectedATCommandResponse = atCommand.ExpectedATResponse;

                int TimesToRepeat = atCommand.TimesToRepeat;
                bool result = false;
                while (IsOpen && TimesToRepeat-- > 0)
                {
                    COMPort.Write(atCommand.ATRequest, atCommand.LineBreak);
                    RiseEvent(this, new SendMessageEventArgs(EventType.Tx, MessageType.AT, "send at-command", atCommand));
                    //Ожидаем ответа
                    //ожидаем таймаута или события о получении верного ответа из метода Event_Note (выше)
                    waitForATCommandResponse.Reset();
                    waitForATCommandResponse.WaitOne(atCommand.WaitForResponse);
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

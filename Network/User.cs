using FFmpegGUI.Windows;
using Riptide;
using Riptide.Utils;
using System;
using System.Threading;

namespace FFmpegGUI.Network
{
    internal class User
    {
        public static User Instance { get; private set; }

        private Client client;
        private bool isRunning;
        private string key;

        public void SendKey(string key)
        {
            Instance = this;

            this.key = key;
            RiptideLogger.Initialize(Console.WriteLine, true);
            isRunning = true;

            new Thread(new ThreadStart(Loop)).Start();
        }

        private void Loop()
        {
            client = new Client()
            {
                TimeoutTime = ushort.MaxValue // Max value timeout to avoid getting timed out for as long as possible when testing with very high loss rates (if all heartbeat messages are lost during this period of time, it will trigger a disconnection)
            };
            client.Connected += (s, e) => Connected();
            client.Disconnected += (s, e) => Disconnected();
            client.ConnectionFailed += (s, e) => { Authorization.Current.ProcessStatus(3); isRunning = false; };
            client.Connect("127.0.0.1:7777", 2);

            while (isRunning)
            {
                client.Update();

                Thread.Sleep(10);
            }

            client.Disconnect();;
        }

        private void Connected()
        {
            RequestAuthorization(key);
        }

        private void Disconnected()
        {

        }

        private void RequestAuthorization(string key)
        {
            Message message = Message.Create(MessageSendMode.Reliable, MessageId.RequestAuthorization);
            message.AddString(System.Security.Principal.WindowsIdentity.GetCurrent().Name);
            message.AddString(key);

            client.Send(message);
        }

        [MessageHandler((ushort)MessageId.ResponseAuthorization)]
        private static void ResponseAuthorization(Message message)
        {
            ushort status = message.GetUShort();
            Authorization.Current.ProcessStatus(status);

            Instance.client.Disconnect();
            Instance.isRunning = false;
        }
    }

    public enum ConnectionStatus : ushort
    {
        Connected = 0,
        WrongKey = 1,
        AlreadyUsed = 2,
        ErrorOnServer = 3
    }

    public enum MessageId : ushort
    {
        RequestAuthorization = 0,
        ResponseAuthorization
    }
}

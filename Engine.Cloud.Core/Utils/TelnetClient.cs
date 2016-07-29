using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Engine.Cloud.Core.Utils
{
    enum Verbs
    {
        WILL = 251,
        WONT = 252,
        DO = 253,
        DONT = 254,
        IAC = 255
    }

    enum Options
    {
        SGA = 3
    }

    class TelnetClient
    {
        public TelnetClient()
        {
            TimeOutMs = 1000;
        }

        TcpClient tcpSocket;

        
        public int TimeOutMs { get; set; }

        public TelnetClient(string Hostname, int Port)
        {
            tcpSocket = new TcpClient(Hostname, Port);

        }

        public string Login(string Username, string Password, int LoginTimeOutMs)
        {
            int oldTimeOutMs = TimeOutMs;
            TimeOutMs = LoginTimeOutMs;


            string s = Read();
            
            WriteLine(Username);
            
            s += Read();

            
            WriteLine(Password);
            
            s += Read();

            TimeOutMs = oldTimeOutMs;
            return s;
        }

        

        public void WriteLine(string cmd)
        {
            Write(cmd + "\r\n");
        }

        public void Write(string cmd)
        {
            if (!tcpSocket.Connected) return;
            byte[] buf = System.Text.ASCIIEncoding.ASCII.GetBytes(cmd.Replace("\0xFF", "\0xFF\0xFF"));
            tcpSocket.GetStream().Write(buf, 0, buf.Length);
        }

        public string Read()
        {
            if (!tcpSocket.Connected) return null;
            StringBuilder sb = new StringBuilder();
            do
            {
                ParseTelnet(sb);
                System.Threading.Thread.Sleep(TimeOutMs);
            } while (tcpSocket.Available > 0);
            return sb.ToString();
        }

        public string WaitFor(string value)
        {
            StringBuilder sb = new StringBuilder();

            long endTime = DateTime.Now.AddSeconds(60).Ticks;

            do
            {
                long currentTime = DateTime.Now.Ticks;

                if (currentTime > endTime)
                {
                    sb.Append(("timed out of client"));
                    
                    throw new Exception(sb.ToString());
                }

                ParseTelnet(sb);

                if (sb.ToString().Contains(value))
                    return sb.ToString();

                Thread.Sleep(2000);

            } while (true);
            
        }

        public bool IsConnected
        {
            get { return tcpSocket.Connected; }
        }

        void ParseTelnet(StringBuilder sb)
        {
            while (tcpSocket.Available > 0)
            {
                int input = tcpSocket.GetStream().ReadByte();
                switch (input)
                {
                    case -1:
                        break;
                    case (int)Verbs.IAC:
                        // interpret as command
                        int inputverb = tcpSocket.GetStream().ReadByte();
                        if (inputverb == -1) break;
                        switch (inputverb)
                        {
                            case (int)Verbs.IAC:
                                //literal IAC = 255 escaped, so append char 255 to string
                                sb.Append(inputverb);
                                break;
                            case (int)Verbs.DO:
                            case (int)Verbs.DONT:
                            case (int)Verbs.WILL:
                            case (int)Verbs.WONT:
                                // reply to all commands with "WONT", unless it is SGA (suppres go ahead)
                                int inputoption = tcpSocket.GetStream().ReadByte();
                                if (inputoption == -1) break;
                                tcpSocket.GetStream().WriteByte((byte)Verbs.IAC);
                                if (inputoption == (int)Options.SGA)
                                    tcpSocket.GetStream().WriteByte(inputverb == (int)Verbs.DO ? (byte)Verbs.WILL : (byte)Verbs.DO);
                                else
                                    tcpSocket.GetStream().WriteByte(inputverb == (int)Verbs.DO ? (byte)Verbs.WONT : (byte)Verbs.DONT);
                                tcpSocket.GetStream().WriteByte((byte)inputoption);
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        sb.Append((char)input);
                        break;
                }
            }
        }
    }
}


//http://www.codeproject.com/Articles/19071/Quick-tool-A-minimalistic-Telnet-library
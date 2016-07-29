//using System;
//using Mandic.Cloud.Core.Utils.Extensions;

//namespace Utils
//{
//    public class LogAdapterElmah 
//    {
//        public void Log(string message)
//        {
//            new MessageLog().LogToElmah(message);
//        }

//        public void Log(Exception e)
//        {
//            e.LogToElmah();
//        }
//        public void Log(string message, Exception e)
//        {
//            Log(message);
//            e.LogToElmah();
//        }
//    }

//    public class MessageLog : Exception
//    {

//    }
//}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LibSumo.Net.Logger
{
    public class LOGGER
    {
        private static readonly log4net.ILog L4NET = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly LOGGER instance = new LOGGER();

        private LOGGER()
        {
            MessageLevel = log4net.Core.Level.Info;
            MessageEnabled = true;
            LogEnabled = true;
        }
        /// <summary>
        /// Request singleton Logger Instance
        /// By default Log level is : Info
        /// </summary>
        public static LOGGER GetInstance
        {
            get
            {
                return instance;
            }
        }

        /*
         *   ALL     DEBUG   INFO    WARN    ERROR   FATAL   OFF
            •All                        
            •DEBUG  •DEBUG                  
            •INFO   •INFO   •INFO               
            •WARN   •WARN   •WARN   •WARN           
            •ERROR  •ERROR  •ERROR  •ERROR  •ERROR      
            •FATAL  •FATAL  •FATAL  •FATAL  •FATAL  •FATAL  
            •OFF    •OFF    •OFF    •OFF    •OFF    •OFF    •OFF
         */

        public log4net.Core.Level MessageLevel { get; set; }

        public bool MessageEnabled { get; set; }
        public bool LogEnabled { get; set; }
        public void Info(string v)
        {
            if(LogEnabled) L4NET.Info(v);
            if (MessageEnabled)
            {
                if (MessageLevel <= log4net.Core.Level.Info)
                    OnMessage(new MessageEventArgs(v, "Info"));
            }
        }

        public void Debug(string v, [CallerMemberName] string callingMethod = "",
                                    [CallerFilePath] string callingFilePath = "",
                                    [CallerLineNumber] int callingFileLineNumber = 0)        
        {
            if (MessageLevel <= log4net.Core.Level.Debug)
                v = String.Format("{0} in {1} at line {2} in source {3}", v, callingMethod, callingFileLineNumber, callingFilePath);

            if (LogEnabled) L4NET.Debug(v);
            if (MessageEnabled)
            {
                if (MessageLevel <= log4net.Core.Level.Debug)
                    OnMessage(new MessageEventArgs(v, "Debug"));
            }
        }
        public void Warn(string v)
        {

            if (LogEnabled) L4NET.Warn(v);
            if (MessageEnabled)
            {
                if (MessageLevel <= log4net.Core.Level.Warn)
                    OnMessage(new MessageEventArgs(v, "Warn"));
            }
        }

        public void Error(string v)
        {

            if (LogEnabled) L4NET.Error(v);
            if (MessageEnabled)
            {
                if (MessageLevel <= log4net.Core.Level.Error)
                    OnMessage(new MessageEventArgs(v, "Error"));
            }
        }
#region Handler
        public delegate void MessageEventHandler(object sender, MessageEventArgs e);
        public event MessageEventHandler MessageAvailable;
        protected virtual void OnMessage(MessageEventArgs e)
        {
            MessageAvailable?.Invoke(this, e);
        }
#endregion

    }
    public class MessageEventArgs : System.EventArgs
    {
        public string Message { get; set; }
        public string Level { get; set; }
        public MessageEventArgs(string msg, string level)
        {
            this.Message = msg;
            this.Level = level;
        }
    }
}

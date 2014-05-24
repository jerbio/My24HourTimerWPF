using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace My24HourTimerWPF
{
    static class ErrorManager
    {

        static public void WriteToEventLog(string message)
        {
            string Current = File.ReadAllText("ErrorFile.txt");
            Current += "\r\n|||\r\n" + message;
            File.WriteAllText("ErrorFile.txt", Current);
            /*string cs = "WagTap";
            EventLog elog = new EventLog();
            if (!EventLog.SourceExists(cs))
            {
                EventLog.CreateEventSource(cs, cs);
            }
            elog.Source = cs;
            elog.EnableRaisingEvents = true;
            elog.WriteEntry(message);
            File.WriteAllText("ErrorFile.txt", message);*/
        }


    }

    
    

}

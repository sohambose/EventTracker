using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace EventTracker
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();
            bool isUserFailedtoLoad = false;
            Console.WriteLine("Enter Date (mm/dd/yyyy): ");
            string strDate = Console.ReadLine();
            if (string.IsNullOrEmpty(strDate))
                strDate = DateTime.Now.ToString();
            Console.WriteLine("___________________________________________________________________________");
            DateTime lookupdate = Convert.ToDateTime(strDate);

            string logType = "Microsoft-Windows-TerminalServices-RemoteConnectionManager/Operational";
            string query = "*[System/EventID=1149]";

            var elQuery = new EventLogQuery(logType, PathType.LogName, query);
            var elReader = new EventLogReader(elQuery);

            if (elReader.ReadEvent() == null)
            {
                Console.WriteLine("Warning!! User Profile details Failed to Load..");
                isUserFailedtoLoad = true;
                logType = "Microsoft-Windows-TerminalServices-RemoteConnectionManager/Admin";
                query = "*[System/EventID=1158]";

                elQuery = new EventLogQuery(logType, PathType.LogName, query);
                elReader = new EventLogReader(elQuery);
            }


            for (EventRecord eventInstance = elReader.ReadEvent(); eventInstance != null; eventInstance = elReader.ReadEvent())
            {
                if (eventInstance.TimeCreated >= lookupdate && eventInstance.TimeCreated <= lookupdate.AddDays(1))
                {
                    //Console.WriteLine(eventInstance.Id);
                    //Console.WriteLine(eventInstance.TimeCreated);
                    DateTime dtTimeStamp = new DateTime();
                    if (eventInstance.TimeCreated.HasValue)
                        dtTimeStamp = eventInstance.TimeCreated.Value;

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(eventInstance.ToXml());

                    if (isUserFailedtoLoad)
                    {
                        string IPAddress = doc.GetElementsByTagName("Param1")[0].InnerText;
                        string hostName = GetHostName(IPAddress);
                        string TimeCreated = dtTimeStamp.ToString("dddd, dd MMMM yyyy HH:mm:ss");
                        Console.WriteLine("\nHost: " + hostName + " (IP: " + IPAddress + ") \nTimestamp: " + TimeCreated);
                    }
                    else
                    {
                        string domain = doc.GetElementsByTagName("Param2")[0].InnerText;
                        string userName = doc.GetElementsByTagName("Param1")[0].InnerText;
                        string IPAddress = doc.GetElementsByTagName("Param3")[0].InnerText;
                        string hostName = GetHostName(IPAddress);
                        string TimeCreated = dtTimeStamp.ToString("dddd, dd MMMM yyyy HH:mm:ss");

                        Console.WriteLine("User: " + domain + "\\" + userName + "\nHost: " + hostName + " (IP: " + IPAddress + ") \nTimestamp: " + TimeCreated);
                    }

                    Console.Write("Press any key to fetch next entry>> ");
                    Console.ReadKey();
                    Console.WriteLine("___________________________________________________________________________");
                }
            }
            Console.WriteLine("All Records fetched..Press any key to exit.....");
            Console.ReadKey();
        }

        public static string GetHostName(string ipAddress)
        {
            try
            {
                IPHostEntry entry = Dns.GetHostEntry(ipAddress);
                if (entry != null)
                {
                    return entry.HostName;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }
    }
}

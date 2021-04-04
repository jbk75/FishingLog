using System;
using System.IO;

namespace FishingLogApi.DAL
{
    public static class Logger
    {
        //ERROR is expected to be an Exception object
        public static int EXCEPTION = 1;
        //STRING is expected to be a String object
        public static int STRING = 2;
        //Default directory for log files
        public static string DEFAULTDIRECTORY = "c:\\temp\\"; //WebConfigurationManager.AppSettings["LogFilePath"];

        /// <summary>
        /// Overrides Logg to use default directory for log files
        /// </summary>
        /// <param name="oLogObject">Object of type Exception or String</param>
        /// <param name="iLogType">Witch Type to convert object to</param>
        //public static void Logg(Object oLogObject, int iLogType)
        //{
        //    Logg(oLogObject, iLogType, DEFAULTDIRECTORY);
        //}

        //public static void Logg(Exception ex)
        //{
        //    Object oLogObject = ex as Object;
        //    Logg(oLogObject, EXCEPTION, DEFAULTDIRECTORY);
        //}

        public static void Logg(String sString)
        {
            Object oLogObject = sString as Object;
            Logg(oLogObject, STRING, DEFAULTDIRECTORY);
        }
        /// <summary>
        /// Writes a text log
        /// </summary>
        /// <param name="oLogObject">Object of type Exception or String</param>
        /// <param name="iLogType">Witch Type to convert object to</param>
        /// <param name="sLogDirectory">Physical location of log file</param>
        private static void Logg(object oLogObject, int iLogType, string sLogDirectory)
        {
            String message = "";
            //
            //Retrieve a message from oLogObject depending on the iLogType
            try
            {
                if (iLogType == EXCEPTION)
                {
                    Exception ex = oLogObject as Exception;
                    message = ex.Message
                    + Environment.NewLine + " * " + ex.InnerException + Environment.NewLine + " * " + ex.StackTrace
                    + Environment.NewLine + " * " + ex.Message
                    + Environment.NewLine + " * " + ex.Source;
                    //message = ex.Message + " StackTrace: " + ex.StackTrace.ToString();
                }
                else if (iLogType == STRING)
                {
                    var ex = oLogObject as string;
                    message = ex;
                }
                else
                {
                    message = "Undefined error object used: " + oLogObject.GetType();
                }
            }
            catch (Exception ex)
            {
                //In case the wrong LogType is being used for the object and typecast causes an error
                message = "Error occured while creating text message for logg : " + ex.Message + " StackTrace: " + ex.StackTrace.ToString();
            }

            DateTime dt = DateTime.Now;
            String filePath = sLogDirectory + "Log"+dt.ToString("yyyyMMdd") + ".log";

            //Create the logDirectory if it does not exist
            if (!Directory.Exists(sLogDirectory))
            {
                Directory.CreateDirectory(sLogDirectory);
            }
            //Create the filePath if it does not exist
            if (!File.Exists(filePath))
            {
                FileStream fs = File.Create(filePath);
                fs.Close();
            }
            try
            {
                //Append message to file
                StreamWriter sw = File.AppendText(filePath);
                var dateString = dt.ToString("dd.MM.yyyy HH:mm:ss");
                sw.WriteLine(dateString + " | " + message);
                //sw.WriteLine(dt.ToString("dd.MM.yyyy HH:mm:ss" + " | " + message));
                //sw.WriteLine(dt.ToString("HH:mm:ss") + " | " + message);
                //sw.WriteLine("#######################################");
                sw.Flush();
                sw.Close();
            }
            catch
            {
                //TODO: Send email?
            }
        }
 
    }
}

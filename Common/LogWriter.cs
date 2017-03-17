using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{  /// <summary>
    /// 直接操作当前文件夹下对应文件的日志类。用最简单的办法来写日志，免得出现log4net的莫名其妙的问题。
    /// </summary>
    /// <remarks>Author:FengZiLi;Date:2012.07.28 13:14</remarks>
    public class LogWriter
    {
        public readonly long MaxFileLength = 1024 * 1024 * 10;

        public bool RecordStackInformationInLog
        {
            get;
            set;
        }

        public bool FlushEachTime
        {
            get;
            set;
        }

        protected bool CacheLogFileHandler
        {
            get;
            set;
        }

        protected bool ShowThreadId
        {
            get;
            set;
        }

        public LogWriter(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException("fileName");
            }
            string strConfigSizeValue = ConfigurationManager.AppSettings["LogFileSizeMb"];
            int iConfigLogSize;
            if (int.TryParse(strConfigSizeValue, out iConfigLogSize) && iConfigLogSize > 0)
            {
                this.MaxFileLength = iConfigLogSize * 1024 * 1024;
            }
            this._FilePath = Path.Combine(this.GetLogDirectory(), fileName);
            this.RecordStackInformationInLog = "true".Equals(ConfigurationManager.AppSettings["RecordStackInformationInLog"], StringComparison.InvariantCultureIgnoreCase);
            this.FlushEachTime = "true".Equals(ConfigurationManager.AppSettings["LogFileFlushEachTime"], StringComparison.InvariantCultureIgnoreCase);
            this.CacheLogFileHandler = "true".Equals(ConfigurationManager.AppSettings["CacheLogFileHandler"], StringComparison.InvariantCultureIgnoreCase);
            this.ShowThreadId = "true".Equals(ConfigurationManager.AppSettings["LogWriterShowThreadId"], StringComparison.InvariantCultureIgnoreCase);
            this.InitializeFileHandler();
        }

        protected virtual string GetLogDirectory()
        {
            string strLogDirectory = ConfigurationManager.AppSettings["LogDirectory"];
            if (string.IsNullOrWhiteSpace(strLogDirectory) == false)
            {
                if (Directory.Exists(strLogDirectory))
                {
                    return strLogDirectory;
                }
                try
                {
                    Directory.CreateDirectory(strLogDirectory);
                    return strLogDirectory;
                }
                catch (Exception expError)
                {
                    Console.WriteLine(expError.ToString());
                }
            }
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        private void InitializeFileHandler()
        {
            if (File.Exists(this._FilePath) == false)
            {
                File.Create(this._FilePath).Dispose();
            }
            if (this.CacheLogFileHandler)
            {
                this.FileHandler = new FileStream(this._FilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
            }
        }

        #region RealWorker

        private string _FilePath;
        private Semaphore _WriterLocker = new Semaphore(1, 1);

        protected FileStream FileHandler
        {
            get;
            set;
        }

        public void WriteInformationAsynchronize(string information)
        {
            ThreadPool.QueueUserWorkItem(o => { this.WriteInformation(o as string); }, information);

            ThreadPool.QueueUserWorkItem(l => { this.WriteInformation(l as string); });
        }

        public void WriteInformation(Exception error)
        {
            string strException = error == null ? "Null Exception Object" : error.ToString();
            this.WriteInformation(strException);
        }

        public void WriteInformation(params string[] informationList)
        {
            this.WriteInformationWithSplitor(string.Empty, informationList);
        }

        public void WriteInformation(params object[] informationList)
        {
            this.WriteInformation(string.Concat(informationList));
        }

        public void WriteInformationWithSplitor(string splitor, params object[] informationList)
        {
            StringBuilder sbContent = ObjectExtension.ConcatValue(informationList, splitor, 200);
            if (sbContent.Length == 0)
            {
                return;
            }
            this.WriteInformation(sbContent.ToString());
        }

        public void WriteInformationWithSplitor(string splitor, params string[] informationList)
        {
            StringBuilder sbContent = ObjectExtension.ConcatValue(informationList, splitor, 200);
            if (sbContent.Length == 0)
            {
                return;
            }
            this.WriteInformation(sbContent.ToString());
        }

        public void WriteInformation(string information)
        {
            this._WriterLocker.WaitOne();
            try
            {
                string strThreadInfor = this.ShowThreadId ? (System.Threading.Thread.CurrentThread.ManagedThreadId.ToString() + "::") : string.Empty;
                string strInformation = string.Concat(strThreadInfor, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "\t", information, "\r\n");
                long lngFileSize;
                byte[] bytContent = System.Text.Encoding.UTF8.GetBytes(strInformation);
                if (this.CacheLogFileHandler)
                {
                    this.FileHandler.Write(bytContent, 0, bytContent.Length);
                    if (this.FlushEachTime)
                    {
                        this.FileHandler.Flush();
                    }
                    lngFileSize = this.FileHandler.Length;
                }
                else
                {
                    using (FileStream fsm = File.Open(this._FilePath, FileMode.OpenOrCreate))
                    {
                        fsm.Seek(0, SeekOrigin.End);
                        fsm.Write(bytContent, 0, bytContent.Length);
                        lngFileSize = fsm.Length;
                    }
                }
                if (this.MaxFileLength <= lngFileSize)
                {
                    this.RestoreLogFile();
                }
            }
            catch (Exception expError)
            {
                try
                {
                    using (FileStream fsm = new FileStream(Path.Combine(this.GetLogDirectory(), "LogWriterError.txt"), FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        fsm.Seek(0, SeekOrigin.End);
                        byte[] bytContent = Encoding.UTF8.GetBytes(string.Concat(DateTime.Now.ToString(), "\t", expError.ToString(), "\r\n"));
                        fsm.Write(bytContent, 0, bytContent.Length);
                    }
                }
                catch
                {
                }

            }
            finally
            {
                this._WriterLocker.Release();
            }

        }

        private void RestoreLogFile()
        {
            if (this.CacheLogFileHandler)
            {
                this.FileHandler.Close();
            }
            string strNewPath = string.Concat(this._FilePath, ".at", DateTime.Now.ToString("yyyyMMddHHmmss"), ".txt");
            if (File.Exists(strNewPath) == false)
            {
                File.Move(this._FilePath, strNewPath);
            }
            this.InitializeFileHandler();
        }


        #endregion

        #region Static Interface

        private static LogWriter _RealWriter = new LogWriter("log.txt");

        public static void WriteLogAsynchronize(string information)
        {
            ThreadPool.QueueUserWorkItem((infor) => { LogWriter.WriteLog(infor as string); }, information);
        }

        public static void WriteLogWithSplitor(string splitor, params string[] contents)
        {
            if (contents.Length == 0)
            {
                return;
            }
            StringBuilder sbContent = ObjectExtension.ConcatValue(contents, splitor, 200);
            if (sbContent.Length == 0)
            {
                return;
            }
            LogWriter.WriteLog(sbContent.ToString());
        }

        public static void WriteLog(params string[] contents)
        {
            LogWriter.WriteLogWithSplitor(string.Empty, contents);
        }

        public static void WriteLog(Exception error)
        {
            if (error == null)
            {
                return;
            }
            LogWriter.WriteLog(error.ToString());
        }

        public static void WriteLog(string information)
        {
            if (LogWriter._RealWriter.RecordStackInformationInLog)
            {
                StringBuilder sbContainerFrame = new StringBuilder(1024 * 6);
                sbContainerFrame.Append(information);
                StackTrace stc = new StackTrace(true);
                foreach (StackFrame sfr in stc.GetFrames())
                {
                    if (string.IsNullOrEmpty(sfr.GetFileName()))
                    {
                        continue;
                    }
                    MethodBase mif = sfr.GetMethod();
                    sbContainerFrame.Append(mif.DeclaringType.FullName).Append(".").Append(mif.Name).Append("     at    ").Append(sfr.GetFileName()).Append("    ").AppendLine(sfr.GetFileLineNumber().ToString());
                }
                LogWriter._RealWriter.WriteInformation(sbContainerFrame.ToString());
                return;
            }
            LogWriter._RealWriter.WriteInformation(information);
        }

        #endregion

        #region LoggerPool

        private static Semaphore _InstanceLocker = new Semaphore(1, 1);

        private static IDictionary<string, LogWriter> _LoggerPool = new Dictionary<string, LogWriter>(32);

        public static LogWriter GetPooledLogWriter(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentOutOfRangeException("fileName", "必须指定确定的文件名。");
            }
            string strKey = fileName.ToLower();
            if (LogWriter._LoggerPool.ContainsKey(strKey))
            {
                return LogWriter._LoggerPool[strKey];
            }
            try
            {
                LogWriter._InstanceLocker.WaitOne();
                if (LogWriter._LoggerPool.ContainsKey(strKey))
                {
                    return LogWriter._LoggerPool[strKey];
                }
                LogWriter lwt = new LogWriter(fileName);
                LogWriter._LoggerPool.Add(strKey, lwt);
                return lwt;
            }
            finally
            {
                LogWriter._InstanceLocker.Release();
            }
        }

        #endregion
    }
}

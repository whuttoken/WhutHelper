using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace Helper
{
    /// <summary>
    /// 本地文件日志。将日志记录在本地文件中。默认级别为DEBUG（即输出所有信息）。
    /// 可能对性能有点影响，可以通过延迟写入技术提高性能。
    /// @2007-06-26 by wbh 已经加入延迟写入技术
    /// @2007-12-13 by wbh 优化了写入逻辑，使用范型List作为缓冲区，并将其定义为静态。
    /// </summary>
    public class FileLogger : AbstractLogger, IDisposable
    {
        protected readonly Conf conf = Conf.CreateFromWebConfig();
        private readonly string _configedFilepath = null;
        private readonly bool _daily = true;
        private readonly long _fileSizeInBytes = 1024 * 1024L;

        // for interlocking ...
        int _flushing = 0;
        static List<string> _msgList;

        /// <summary>
        /// 为某个Type提供Logging服务
        /// </summary>
        /// <param name="t">Type，可以为null</param>
        public FileLogger(Type t)
        {
            // get the file to log
            _configedFilepath = conf.get("logging.filelogger.filepath") as string;
            if (string.IsNullOrEmpty(_configedFilepath))
                throw new Exception("无法创建" + this.GetType().FullName + "的实例。未指定log文件路径，请在AppSettings中指定logging.filepath");
            // 将虚拟路径~转化为服务器端物理路径
            if (_configedFilepath.StartsWith("~/"))
                _configedFilepath = _configedFilepath.Replace("~/", HttpContext.Current.Server.MapPath("~/")).Replace('/', '\\');
            // 是否每天一个日志文件？
            // 如果是，则_filepath应该指定一个文件夹，最后的日志文件名格式为yyyy-mm-dd_n.txt，其中yyyy-mm-dd为当前日期，n为计数器。
            // 如果不是，则_filepath应该指定为一个文件。
            _daily = bool.Parse(conf.get("logging.filelogger.daily", bool.TrueString) as string);
            // 文件大小限制
            _fileSizeInBytes = Function.IntSafeConvert(conf.get("logging.filelogger.filesize"), 1024 * 1024);
            TypeLogFor = t;
            // 缓冲区大小,默认为1。也就是不缓冲。
            int size = ( Function.IntSafeConvert(conf.get("logging.filelogger.buffersize"), 1) );
            if (size <= 0 || size > 1024)
                throw new Exception("无效的缓冲大小" + size + " 。 请为logging.filelogger.buffersize指定一个1~1024之间的整数。");
            _msgList = new List<string>(size);
        }
        /// <summary>
        /// 指定文件的绝对路径
        /// </summary>
        /// <param name="filepath">文件的绝对路径。可能是文件或者文件夹。</param>
        /// <param name="t">Type，可以为null</param>
        public FileLogger(string filepath, Type t)
        {
            _configedFilepath = filepath;
            // 将虚拟路径~转化为服务器端物理路径
            if (_configedFilepath.StartsWith("~/"))
                _configedFilepath = _configedFilepath.Replace("~/", HttpContext.Current.Server.MapPath("~/")).Replace('/', '\\');
            TypeLogFor = t;
            // dailly or not
            _daily = bool.Parse(conf.get("logging.filelogger.daily", bool.TrueString) as string);
            // 文件大小限制
            _fileSizeInBytes = Function.IntSafeConvert(conf.get("logging.filelogger.filesize"), 1024 * 1024);
            if (string.IsNullOrEmpty(filepath))
                throw new ArgumentNullException("filepath");
            // 缓冲区大小,默认为1。也就是不缓冲。
            int size = (Function.IntSafeConvert(conf.get("logging.fileloger.buffersize"), 1));
            if (size <= 0 || size > 1024)
                throw new Exception("无效的缓冲大小" + size + " 。 请为logging.fileloger.buffersize指定一个1~1024之间的整数。");
            _msgList = new List<string>(size);
        }
        /// <summary>
        /// 获得当前的全局日志级别（从web.config中读取）
        /// </summary>
        /// <returns></returns>
        private LoggingLevel.EnumLoggingLevel GetLogLevel()
        {
            return LoggingLevel.ParseLevelString(conf.get("logging.filelogger.level", "DEBUG") as string);
        }

        void flush()
        {
            using (StreamWriter w = File.AppendText(LocalFilePath))
            {
                foreach (string msg in _msgList)
                    w.WriteLine(msg);
                // Close the writer and underlying file.
                w.Close();
            }
            // clear
            _msgList.Clear();
        }

        void WriteInternal(string strMsg)
        {
            // attempt to acquire the arraylist ...
            // somebody else is saving, and we keep trying..
            while (Interlocked.Exchange(ref _flushing, 1) == 1) ;

            try
            {
                // add the new item
                _msgList.Add(strMsg);
                if (_msgList.Count >= _msgList.Capacity)
                {
                    // flush
                    flush();
                }
            }
            finally
            {
                // free the lock
                while (Interlocked.Exchange(ref _flushing, 0) == 0) ;
            }
        }
        /// <summary>
        /// 用于保存本地文件的路径。
        /// </summary>
        string LocalFilePath
        {
            get 
            {
                string _localFilepath = Path.GetFullPath(_configedFilepath);
                // daily ?
                if (_daily)
                {
                    // the localfilePath MUST BE a directory
                    if (_localFilepath.EndsWith(@"\") == false)
                        _localFilepath += @"\";
                    if (Directory.Exists(_localFilepath) == false)
                        Directory.CreateDirectory(_localFilepath);
                    _localFilepath += DateTime.Today.ToString("yyyy-MM-dd");
                }
                const int MAX_RETRY = 1024;
                int i = 2;
                string _temp = _localFilepath;
                if (_daily)
                    _temp = _localFilepath + ".txt";
                for (; i < MAX_RETRY && File.Exists(_temp); i++)
                {
                    // file already exists ...
                    FileInfo fi = new FileInfo(_temp);
                    if (fi.Length >= _fileSizeInBytes)
                    {
                        if (Path.HasExtension(_localFilepath))
                            _temp = _localFilepath.Remove(_localFilepath.LastIndexOf(Path.GetExtension(_localFilepath))) + "_" + i + Path.GetExtension(_localFilepath);
                        else
                            _temp = _localFilepath + "_" + i;

                        if(_daily)
                            _temp = string.Format("{0}_{1}.txt", _localFilepath, i);
                    }
                    else break;
                }
                if (i >= MAX_RETRY)
                {
                    // too many logs today ... is our site under attack ?
                }
                _localFilepath = _temp;

                return _localFilepath;
            }
        }
        virtual protected void Write(string msg)
        {
            //
            //File.AppendAllText(local, msg);
            WriteInternal(msg);
            // 如果有异常则抛出。为了不影响效率和运行可以关闭Logger来检查问题。
        }

        #region AbstractLogger 成员

        public override void Debug(string msg)
        {
            if (GetLogLevel() <= LoggingLevel.EnumLoggingLevel.DEBUG)
            {
                // log this
                try
                {
                    Write(string.Format("[DEBUG][{0}]-[{1}]-{2}", DateTime.Now, TypeLogFor is Type ? TypeLogFor.FullName : "n/a", msg));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Assert(false, ex.Message);
                }
            }
        }


        public override void Warn(string msg)
        {
            if (GetLogLevel() <= LoggingLevel.EnumLoggingLevel.WARN)
            {
                // log this
                try
                {
                    Write(string.Format("[WARN][{0}]-[{1}]-{2}", DateTime.Now, TypeLogFor is Type ? TypeLogFor.FullName : "n/a", msg));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Assert(false, ex.Message);
                }
            }
        }

        public override void Error(string msg)
        {
            if (GetLogLevel() <= LoggingLevel.EnumLoggingLevel.ERROR)
            {
                // log this
                try
                {
                    Write(string.Format("[ERROR][{0}]-[{1}]-{2}", DateTime.Now, TypeLogFor is Type ? TypeLogFor.FullName : "n/a", msg));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Assert(false, ex.Message);
                }
            }
        }

        #endregion

        #region IDisposable 成员

        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // Take yourself off the Finalization queue 
            // to prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }
        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the 
        // runtime from inside the finalizer and you should not reference 
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
            Write(string.Empty);
            flush();
            if (disposing)
            {
                // Dispose managed resources.
                // no managed resource need to be freed here ..
            }
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method 
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~FileLogger()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}
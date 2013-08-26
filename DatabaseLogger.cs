using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace Helper
{
    /// <summary>
    /// DatabaseLogger 的摘要说明
    /// </summary>
    public class DatabaseLogger : AbstractLogger
    {
        Conf conf = Conf.CreateFromWebConfig();
        string _ConnectionName = string.Empty;
        public DatabaseLogger()
        {
            _ConnectionName = Conf.CreateFromWebConfig().get("logging.dblogger.connection") as string;
            if (string.IsNullOrEmpty(_ConnectionName))
                throw new Exception("无法实例化DatabaseLogger对象，请在web.config中指定使用的数据库连接字符串的名称logging.dblogger.connection。");
        }

        /// <summary>
        /// 将登录字符串拆分之后插入数据库
        /// </summary>
        /// <param name="msg"></param>
        void Write(string msg)
        {
            throw new Exception("DatabaseLogger.Write() 还未实现。");
        }
        /// <summary>
        /// 获得当前的全局日志级别（从web.config中读取）
        /// </summary>
        /// <returns></returns>
        private LoggingLevel.EnumLoggingLevel GetLogLevel()
        {
            return LoggingLevel.ParseLevelString(conf.get("logging.filelogger.level", "DEBUG") as string);
        }

        public override void Debug(string msg)
        {
            if (GetLogLevel() <= LoggingLevel.EnumLoggingLevel.DEBUG)
            {
                Write(msg);
            }
        }

        public override void Warn(string msg)
        {
            if (GetLogLevel() <= LoggingLevel.EnumLoggingLevel.WARN)
            {
                Write(msg);
            }
        }

        public override void Error(string msg)
        {
            if (GetLogLevel() <= LoggingLevel.EnumLoggingLevel.ERROR)
            {
                Write(msg);
            }
        }
    }
}
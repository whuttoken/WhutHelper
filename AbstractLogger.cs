using System;
using System.Data;
using System.Configuration;

using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting;

namespace Helper
{
    /// <summary>
    /// 日志级别。数字越小，记录越详细。
    /// </summary>
    public static class LoggingLevel
    {
        /// <summary>
        /// 级别枚举
        /// </summary>
        public enum EnumLoggingLevel
        {
            DEBUG,      // 记录debug信息
            WARN,       // 只记录WARNING信息
            ERROR,      // 只记录ERROR信息
            OFF         // 关闭logger
        }

        /// <summary>
        /// 将级别名称转换为其对应的枚举
        /// </summary>
        /// <param name="level">string级别描述，必须大写。DEBUG, WARN, ERROR, OFF</param>
        /// <returns>0-3，或者－1</returns>
        public static EnumLoggingLevel ParseLevelString(string level)
        {
            if (string.IsNullOrEmpty(level))
                throw new Exception("invalid 'level' " + level);
            if (level.ToLower() == "off")
                return EnumLoggingLevel.OFF;
            int i;
            if ((i = "DWE".IndexOf(level[0])) < 0)
                throw new Exception("invalid 'level' " + level);
            return (EnumLoggingLevel) i; 
        }
    }


    /// <summary>
    /// 抽象logger，它提供实例化具体logger的方法和所有logger必须实现的接口。
    /// </summary>
    public abstract class AbstractLogger
    {
        static Hashtable _htInstances = new Hashtable();
        Type _typeLogFor;
        public Type TypeLogFor
        {
            get { return _typeLogFor; }
            set { _typeLogFor = value; }
        }
        /// <summary>
        /// 获得实现AbstractLogger的类的实例，类的名称在Web.config中指定（默认返回Helper.FileLogger的实例）。此类必须在Helper命名空间下，可以参考Helper.FileLogger。
        /// </summary>
        /// <param name="tLogFor">为其提供Logging服务的Type，一般是某个页面的this.GetType()。可以为null。</param>
        /// <returns>成功时返回Logger实例，否则null。可能有异常抛出。</returns>
        public static AbstractLogger GetLogger(Type tLogFor)
        {
            Conf conf = Conf.CreateFromWebConfig();
            string typename = conf.get("logging.logger") as string;
            if (string.IsNullOrEmpty(typename))
                typename = "Helper.FileLogger"; // default to the FileLogger class
            return GetLogger(tLogFor, typename);
        }
        /// <summary>
        /// 获得实现AbstractLogger的类的实例，类的名称在Web.config中指定（默认返回Helper.FileLogger的实例）。此类必须在Helper命名空间下，可以参考Helper.FileLogger。
        /// </summary>
        /// <param name="tLogFor">为其提供Logging服务的Type，一般是某个页面的this.GetType()</param>
        /// <param name="strClassFullName">实现AbstractLogger的类的全名</param>
        /// <returns>成功时返回Logger实例，否则null。可能有异常抛出。</returns>
        public static AbstractLogger GetLogger(Type typeLogFor, string strLoggerClassFullName)
        {
            AbstractLogger _l = null;
            lock (_htInstances)
            {
                if (_htInstances.ContainsKey(typeLogFor.FullName))
                {
                    _l = _htInstances[typeLogFor.FullName] as AbstractLogger;
                    _l.TypeLogFor = typeLogFor;
                    return _l;
                }
                return (_htInstances[typeLogFor.FullName] = Activator.CreateInstance(Type.GetType(strLoggerClassFullName, true), new object[] { typeLogFor }))
                    as AbstractLogger;
            }
        }
        /// <summary>
        /// 输出Debug信息
        /// </summary>
        /// <param name="msg">要输出的信息</param>
        abstract public void  Debug(string msg);

        /// <summary>
        /// 产生警告信息
        /// </summary>
        /// <param name="msg">信息</param>
        abstract public void  Warn(string msg);

        /// <summary>
        /// 产生错误信息
        /// </summary>
        /// <param name="msg">要输出的信息</param>
        abstract public void  Error(string msg);
    }


}
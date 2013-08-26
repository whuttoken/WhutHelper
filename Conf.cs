using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections;

/// 创建者：wubinghua
/// 创建时间：2007-06-02
/// 版本：1.0
/// 
/// 修改历史
/// 修改者/时间：wubinghua/2007-11-28
namespace Helper
{
    /// <summary>
    /// 管理配置数据。数据来源可以指定为任何实现IConfigurationSourceProvider的类。
    /// </summary>
    public class Conf
    {
        private IConfigurationSourceProvider _SourceProvider = null;
        /// <summary>
        /// 获取或者设置配置来源
        /// </summary>
        public IConfigurationSourceProvider ISourceProvider
        {
            get { return this._SourceProvider; }
            set
            {
                if (value is IConfigurationSourceProvider == false)
                    throw new Exception("'ISourceProvider' 必须引用实现了IConfigurationSourceProvider的类的实例。value=" + value);

                this._SourceProvider = value;
            }
        }
        /// <summary>
        /// 从指定的配置源创建Conf的实例。
        /// 可能抛出异常。
        /// </summary>
        public Conf(IConfigurationSourceProvider _provider)
        {
            this.ISourceProvider = _provider;
        }

        /// <summary>
        /// 构造默认的配置数据，数据来源为 Session > Application > Web.config > Database
        /// </summary>
        public Conf()
        {
            // instantiate a new instance with the source provider specified in web.config
            //string typename = CreateFromWebConfig().get("site.conf.source") as string;
            //if (string.IsNullOrEmpty(typename))
            //    throw new Exception("无法创建Conf的实例，未制定配置来源。请在web.config的AppSettings中制定site.conf.source属性。");
            //this._SourceProvider = Activator.CreateInstance(Type.GetType(typename)) as IConfigurationSourceProvider;
            //if (this._SourceProvider == null)
            //    throw new Exception("无法创建Conf的实例，名为" + typename + "的类无法被实例化。请确认指定了正确的类名以及该类提供了默认构造函数。");
        }

        /// <summary>
        /// 创建从web.config读取和设置参数的对象
        /// </summary>
        /// <returns>Conf的实例。</returns>
        public static Conf CreateFromWebConfig()
        {
            return new Conf(new ConfiguraionManagerProvider());
        }

        /// <summary>
        /// 创建从当前Session读取和设置参数的对象
        /// </summary>
        /// <returns>Conf的实例</returns>
        public static Conf CreateFromCurrentSession()
        {
            return new Conf(new SessionStateProvider());
        }

        /// <summary>
        /// 创建从当前的Cookie读取和设置参数的对象
        /// </summary>
        /// <returns>Conf的实例</returns>
        public static Conf CreateFromHttpCookie()
        {
            return new Conf(new HttpCookieProvider());
        }

        public static Conf CreateFromDatabase()
        {
            return new Conf(new DatabaseConfigurationSource(DBAccess.Common.GetDefaultDBConnection()));
        }

        public static Conf CreateFromCurrentApplication()
        {
            return new Conf(new ApplicationConfigurationProvider());
        }
        /// <summary>
        /// 获得参数的值（参数可能保存在Session，Application，Web.config，数据库中）并且在参数不存在的情况下提供一个默认返回值。
        /// </summary>
        /// <param name="key">参数名</param>
        /// <param name="def">当参数名不存在的时候返回的默认值</param>
        /// <returns>参数值，或者指定的默认值。</returns>
        public object get(object key, object def)
        {
            if (ISourceProvider != null)
                return this.ISourceProvider.get(key, def);
            // Session > Application > Web.config > Database
            object value = def;
            try { value = CreateFromCurrentSession().get(key); }
            catch { value = null; }
            if (value == null)
                value = CreateFromCurrentApplication().get(key);
            if (value == null)
                value = CreateFromWebConfig().get(key);
            if (value == null)
                value = CreateFromDatabase().get(key);
            return value == null ? def : value;
        }
        /// <summary>
        /// 获得参数的值，如果参数不存在则返回null。
        /// </summary>
        /// <param name="key">参数</param>
        /// <returns>参数的值或者null</returns>
        public object get(object key)
        {
            return get(key, null);
        }

        /// <summary>
        /// 设置参数的值
        /// </summary>
        /// <param name="key">参数</param>
        /// <param name="value">值</param>
        public void set(object key, object value)
        {
            if (ISourceProvider == null) throw new ApplicationException("未制定配置来源");
            this.ISourceProvider.set(key, value);
        }


        public void remove(object key)
        {
            if (ISourceProvider == null) throw new ApplicationException("未制定配置来源");
            this.ISourceProvider.remove(key);
        }
    }
}
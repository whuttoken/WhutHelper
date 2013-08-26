using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Reflection;
using System.Runtime.Remoting;

namespace Helper
{
    /// <summary>
    /// 提供字符串的加密和解密功能
    /// </summary>
    public abstract class AbstractStringEncryptor
    {
        /// <summary>
        /// 获得默认的字符串加密、解密对象。在Web.config的AppSetting中通过site.security.encryption.provider指定类的全名。
        /// </summary>
        /// <returns>AbstractStringEncryptor对象实例。可能有异常发生。</returns>
        public static AbstractStringEncryptor GetDefaultEncryptor()
        {
            Conf conf =  Conf.CreateFromWebConfig();
            string type = conf.get("site.security.encryption.provider") as string;
            if (string.IsNullOrEmpty(type))
            {
                type = "Helper.DES";
                // log ...
                AbstractLogger.GetLogger(typeof(AbstractStringEncryptor)).Warn("未配置site.security.encryption.provider，将使用“Helper.DES”类作为字符串加密、解密工具。");
            }
            return GetEncryptor(type);
        }
        /// <summary>
        /// 获得指定的实现了AbstractStringEncryptor的类的实例
        /// </summary>
        /// <param name="strFullClassname">实现AbstractStringEncryptor的类的全名</param>
        /// <returns>对象的实例。可能有异常发生。</returns>
        public static AbstractStringEncryptor GetEncryptor(string strFullClassname)
        {
            return Activator.CreateInstance(Type.GetType(strFullClassname, true)) as AbstractStringEncryptor;
        }
        /// <summary>
        /// 将字符串加密
        /// </summary>
        /// <param name="strInput">要加密的字符串（明文）</param>
        /// <returns>加密之后的字符串（密文）</returns>
        abstract public string Encode(string strInput);

        /// <summary>
        /// 将字符串加密,自定义密钥
        /// </summary>
        /// <param name="strInput">要加密的字符串（明文）</param>
        /// <returns>加密之后的字符串（密文）</returns>
        abstract public string Encode(string strInput,string key);

        /// <summary>
        /// 将字符串解密
        /// </summary>
        /// <param name="strInput">要解密的字符串（密文）</param>
        /// <returns>解密之后的字符串（明文）</returns>
        abstract public string Decode(string strInput);
    }
}
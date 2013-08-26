using System;
using System.Web;
using System.Web.UI.WebControls;

using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Data;
using System.Data.SqlTypes;



namespace Helper
{
    /// <summary>
    /// useful funtions wrapper
    /// </summary>
    public sealed partial class Function
    {
        private Function() { }

        /// <summary>
        /// return the MD5 hash of a string
        /// </summary>
        /// <param name="strToHash">the string to hash</param>
        /// <returns>the hashed string</returns>
        public static string MD5(string strToHash)
        {
            //	Get Bytes
            byte[] byteToHash = Encoding.Unicode.GetBytes(strToHash);
            //	Hash
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] byteHashed = md5.ComputeHash(byteToHash);
            StringBuilder sb = new StringBuilder();
            foreach (byte bt in byteHashed)
                sb.AppendFormat("{0:X2}", bt);
            return sb.ToString();
        }

        /// <summary>
        /// verify the given email is valid or not
        /// </summary>
        /// <param name="strIn">the email</param>
        /// <returns>true if it is, or false.</returns>
        public static bool IsValidEmail(string strIn)
        {
            // Return true if strIn is in valid e-mail format.
            const string strPattern = @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            return Regex.IsMatch(strIn, strPattern);
        }

        /// <summary>
        /// 替换字符串中的脚本标记"<>"为对应的HTML标记&lt;&gt;
        /// </summary>
        /// <param name="strInput">输入字符串</param>
        /// <returns>输出字符串</returns>
        public static string NoScript(string strInput)
        {
            if (string.IsNullOrEmpty(strInput))
                return string.Empty;
            if (strInput.IndexOf('<') >= 0)
                strInput = strInput.Replace("<", "&lt;");
            if (strInput.IndexOf('>') >= 0)
                strInput = strInput.Replace(">", "&gt;");
            return strInput;
        }

        public static string NormalizeJSString(string strInput)
        {
            if (string.IsNullOrEmpty(strInput))
                return string.Empty;
            if (strInput.IndexOf('\'') >= 0)
                strInput = strInput.Replace("'", @"\'");
            if (strInput.IndexOf('\"') >= 0)
                strInput = strInput.Replace("\"", "\\\"");
            return strInput;
        }

        /// <summary>
        /// 将字符串中的URL文本转化为HTML标记"<a></a>"
        /// </summary>
        /// <param name="strInput"></param>
        /// <returns></returns>
        public static string DecorateTextWithHyperLinkAndBR(string strInput)
        {
            // Create a regular expression that matches a series of one 
            // or more digits plus @.
            //m.Replace(strInput, @"<a href='\1'>\1</a>");
            const string strPattern = @"(http\://[^ \n]+)";
            MatchCollection c = new Regex(strPattern, RegexOptions.IgnoreCase).Matches(strInput);
            foreach (Match m in c)
            {
                strInput = strInput.Replace(m.Value, m.Result("<a href='$1' target='_blank'>$1</a>"));
            }
            return strInput.Replace("\r", "<br/>");
        }

        /// <summary>
        ///  convert an object to int32 value, never throws an exception
        /// </summary>
        /// <param name="o">the object trying to convert, may be null</param>
        /// <returns>the int32 representation of the object, or 0 when it cannot be converted </returns>
        /// 
        public static int IntSafeConvert(object o, int iDefault)
        {
            if (o == null)
                return iDefault;
            string _s = o.ToString().Trim();
            if (_s.Length <= 0)
                return iDefault;
            int res = iDefault;
            return int.TryParse(_s, out res) ? res : iDefault;
        }
        public static int IntSafeConvert(object o)
        {
            return IntSafeConvert(o, 0);
        }
        /// <summary>
        /// 将object（通常为string）转换为bool类型，失败时返回默认值bDefault。
        /// </summary>
        /// <param name="o"></param>
        /// <param name="bDefault"></param>
        /// <returns></returns>
        public static bool BoolSafeConvert(object o, bool bDefault)
        {
            if (o == null)
                return bDefault;
            string _s = o.ToString().Trim();
            if (_s.Length <= 0)
                return bDefault;
            bool res = bDefault;
            return bool.TryParse(_s, out res) ? res : bDefault;
        }
        /// <summary>
        /// 默认返回false
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static bool BoolSafeConvert(object o)
        {
            return BoolSafeConvert(o, false);
        }
        /// <summary>
        /// convert an object (usually a string) into its DateTime presentation
        /// </summary>
        /// <param name="o">the object to parse ( using its ToString() method )</param>
        /// <param name="dDefault">a default DateTime value to return when failed</param>
        /// <returns>a DateTime struct, either parsed or provided default</returns>
        public static DateTime DateTimeSafeConvert(object o, DateTime dDefault)
        {
            // i.e., 830727
            if (o == null || o.ToString().Trim().Length < 6)
                return dDefault;
            DateTime res = DateTime.Today;
            return DateTime.TryParse(o.ToString().Trim(), out res) ? res : dDefault;
        }
        /// <summary>
        /// convert an object (usually a string) into its DateTime format
        /// </summary>
        /// <param name="o">the object to convert</param>
        /// <returns>right datetime object, or SqlDateTime.MinValue.Value when the operation failed</returns>
        public static DateTime DateTimeSafeConvert(object o)
        {
            return DateTimeSafeConvert(o, SqlDateTime.MinValue.Value);
        }

        /// <summary>
        /// 将相对URL转化为绝对URL，用于无法访问Page对象的情形。
        /// </summary>
        /// <param name="strRelativeURL">以~/开有的URI地址。</param>
        /// <returns></returns>
        public static string ResolveRelURL(string strRelativeURL)
        {
            strRelativeURL = strRelativeURL.TrimStart("~".ToCharArray());
            if(strRelativeURL.StartsWith("/"))
                return GetAppRoot() + strRelativeURL.Substring(1);
            return new Literal().ResolveClientUrl(strRelativeURL);
        }
        /// <summary>
        /// 获得应用程序的根路径URL。
        /// 如果在web.config中定义了"site.domain", 则使用之。
        /// </summary>
        /// <returns>the root path of this web app</returns>
        public static string GetAppRoot()
        {
            string _app_root = Conf.CreateFromWebConfig().get("site.domain") as string;
            if (string.IsNullOrEmpty(_app_root) == false)
                return _app_root.EndsWith("/") ? _app_root : _app_root + "/";
            return new Literal().ResolveClientUrl("~/");
        }

        /// <summary>
        /// 计算两个DateTime的差值的绝对值，然后以适当的文字描述返回。
        /// 例如：2年，3个月，1个星期等。
        /// </summary>
        /// <param name="t1">第一个时间</param>
        /// <param name="t2">第二个时间</param>
        /// <param name="bShortFormat">当相差只有几秒的时候是否返回详细的秒数。一般指定为false。</param>
        /// <returns>时间差的字符串表示。</returns>
        public static string TimeDuration2String(DateTime t1, DateTime t2, bool bShortFormat)
        {
            TimeSpan ts = (t1 - t2);
            ts = ts.Duration();
            int iDays = Convert.ToInt32(ts.TotalDays);
            int iHours = Convert.ToInt32(ts.TotalHours);
            int iMinutes = Convert.ToInt32(ts.TotalMinutes);
            if (iDays > 0)
            {
                // how many years ?
                if (iDays / 365 >= 1)
                {
                    return (iDays / 365) + "年";
                }
                // months ?
                if (iDays / 30 >= 1)
                {
                    return (iDays / 30) + "个月";
                }
                // weeks ?
                if (iDays / 7 >= 1)
                {
                    return (iDays / 7) + "星期";
                }
                // days ?
                return iDays + "天";
            }
            StringBuilder sbSpan = new StringBuilder();
            if (iHours > 0)
            {
                sbSpan.Append(iHours % 24).Append("小时");
                return sbSpan.ToString();
            }
            if (iMinutes > 0)
            {
                sbSpan.Append(iMinutes % 60).Append("分钟");
                return sbSpan.ToString();
            }
            if (bShortFormat && ts.TotalSeconds <= 60.0)
                return "约1分钟";
            return ts.TotalSeconds + "秒";
        }

        /// <summary>
        /// 将字符串按照指定的长度靠左对齐，如果指定的长度大于字符串本身的长度，则截断并将填充字符串追加到末尾；否则返回原字符串。
        /// </summary>
        /// <param name="strInput">输入字符串</param>
        /// <param name="iTotalWidth">输出字符串的长度（包含填充字符串计算在内）</param>
        /// <param name="strFill">填充字符串</param>
        /// <returns>原输入字符串的一部分+填充字符串；或者原字符串+填充字符串</returns>
        public static string StringLeftAlign(string strInput, int iTotalWidth, string strFill)
        {
            if (string.IsNullOrEmpty(strInput))
                return strInput;
            int _fillLength = 0;
            foreach (char ch in strFill)
                if (ch < 256) _fillLength++; else _fillLength += 2;
            iTotalWidth *= 2;
            int i;
            for (i = 0, iTotalWidth -= _fillLength; i < strInput.Length; i++)
            {
                if (strInput[i] < 256) iTotalWidth--; else iTotalWidth -= 2;
                if (iTotalWidth == 0) break; else if (iTotalWidth < 0) { i--; break; }
            }
            if (i + 1 >= strInput.Length) return strInput;
            return strInput.Substring(0, Math.Min(strInput.Length, i + 1)) + strFill;
        }
        /// <summary>
        /// 同StringLeftAlign(input, totalwidth, fill)，只是它不使用填充字符串。
        /// </summary>
        /// <param name="oInput"></param>
        /// <param name="iTotalWidth"></param>
        /// <returns></returns>
        public static string StringLeftAlign(object oInput, int iTotalWidth)
        {
            if (oInput == null)
                throw new ArgumentNullException("oInput");
            return StringLeftAlign(oInput.ToString(), iTotalWidth, String.Empty);
        }

        /// <summary>
        /// 以DEBUG级别将信息输出到文件日志
        /// </summary>
        /// <param name="strMsg">要输出的信息</param>
        /// <param name="req">当前的请求对象，可以为null</param>
        public static void LogError(string strMsg, HttpContext context)
        {
            try
            {
                // get the default FileLogger instance
                AbstractLogger logger = AbstractLogger.GetLogger(typeof(Function));
                if (logger == null)
                    return;
                logger.Error(
                    string.Format("[{0}] by [{1}]\r\n{2}\r\n",
                    context == null ? "N/A" : context.Request.RawUrl,
                    context == null ? "N/A" : context.Request.UserHostAddress,
                    strMsg));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Assert(false, ex.Message);
            }
        }
        /// <summary>
        /// 以DEBUG级别将异常信息输出到文件日志
        /// </summary>
        /// <param name="e">要输出的异常</param>
        /// <param name="req">当前请求对象</param>
        public static void LogError(Exception e, HttpContext context)
        {
            LogError(string.Format("{0}{1}{2}", e.Message, Environment.NewLine, e.StackTrace), context);
        }


        /// <summary>
        /// 页面成功提交并定义转向
        /// </summary>
        /// <param name="redirectUrl">要转向的页面</param>
        /// <param name="second">页面停留时间</param>
        /// <returns></returns>
        public static void SucessSubmitAndRedirect(string redirectUrl, int second)
        {
            System.Web.HttpContext.Current.Response.Redirect("~/admin/sucess.aspx?rUrl=" + System.Web.HttpContext.Current.Server.UrlEncode(redirectUrl) + "&tSec=" + second.ToString());
        }
        public static void SucessSubmitAndRedirect(string redirectUrl)
        {
            SucessSubmitAndRedirect(redirectUrl, 3);
        }

        /// <summary>
        /// 将字符串中的HTML标记去掉"<>"
        /// </summary>
        /// <param name="strInput"></param>
        /// <returns></returns>
        public static string RemoveHtmlTags(string strInput)
        {
            const string pattern = "<[^>]*>|<[\\w\\d]*";
            return Regex.Replace(strInput, pattern, string.Empty, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }


        /// <summary>
        /// if( a is null or string.Empty ) return b;
        /// else return a;
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static string AorB(string a, string b)
        {
            return string.IsNullOrEmpty(a) ? b : a;
        }

        /// <summary>
        /// 根据选择条件构造datatable
        /// </summary>
        /// <param name="dt">原始的datatable</param>
		/// <param name="top">选择多少个</param>
        /// <param name="filter">过滤条件</param>
        /// <param name="sort">排序</param>
        /// <returns>生成的新的datatable</returns>
		public static DataTable selectFromTable(DataTable dt, int top, string filter, string sort)
		{
			int i;
			DataTable _dt = null;
			_dt = dt.Clone();
			DataRow[] rows = dt.Select(filter, sort);
			for (i = 0; i < top && i < rows.Length; i++)
			{
				_dt.ImportRow(rows[i]);
			}
			return _dt;
		}

        public static DataTable selectFromTable(DataTable dt, string filter, string sort)
        {
			return selectFromTable(dt, 1000000, filter, sort);
        }

        public static DataTable selectFromTable(DataTable dt, string filter)
        {
            return selectFromTable(dt, filter, "");
        }

		public static DataTable selectFromTable(DataTable dt, int top, string filter)
		{
			 return selectFromTable(dt, top, filter, "");
		}

    }

}
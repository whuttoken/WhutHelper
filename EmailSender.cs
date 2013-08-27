using System;
using System.Data;
using System.Collections.Generic;
using Dimac.JMail;
namespace Helper
{
    /// <summary>
    /// Smtp 的摘要说明
    /// </summary>
    public sealed class EmailSender
    {
        static Conf webconf = Conf.CreateFromWebConfig();
        /// <summary>
        /// cannot create an instance of this class
        /// </summary>
        private EmailSender() { }

        /// <summary>
        /// 发送Email到多个接受人
        /// </summary>
        /// <param name="strSubject"></param>
        /// <param name="strContent"></param>
        /// <param name="strFrom"></param>
        /// <param name="listReceivers"></param>
        /// <returns></returns>
        public static void SendEmail(string strSubject, string strContent, List<string> listReceivers)
        {
            Message message = new Message();
            message.From.Email = webconf.get("smtp.site.email", "busy_1@163.com") as string;
            message.From.FullName = webconf.get("site.name", "武汉理工大学经纬网") as string;
            foreach (string rcv in listReceivers)
                message.To.Add(rcv);

            message.Subject = strSubject;
            message.Charset = System.Text.Encoding.UTF8;
            message.BodyHtml = strContent;

            string _server = webconf.get("smtp.server", "wutnews.net") as string;
            short _port = (short)Function.IntSafeConvert(webconf.get("smtp.port"), 25);
            string _username = webconf.get("smtp.username", string.Empty) as string;
            string _pwd = webconf.get("smtp.password", string.Empty) as string;

            if (string.IsNullOrEmpty(_username))
                Smtp.Send(message, _server, _port);
            else
                Smtp.Send(message, _server, _port, GetDomain(message.From.Email), SmtpAuthentication.Any, _username, _pwd);
        }

        private static string GetDomain(string email)
        {
            if (string.IsNullOrEmpty(email)) return email;
            int index = email.IndexOf('@');
            return email.Substring(index + 1);
        }
        /// <summary>
        /// Send an email to somebody
        /// </summary>
        /// <param name="strSubject">the subject</param>
        /// <param name="strContent">the content which supports HTML</param>
        /// <param name="strFrom">the email address from which this email will be delivered</param>
        /// <param name="strToEmail">the receipient</param>
        public static void SendEmail(string strSubject, string strContent, string strToEmail)
        {
            List<string> list = new List<string>(1);
            list.Add(strToEmail);
            SendEmail(strSubject, strContent, list);
        }
    }
}
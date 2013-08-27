using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using OpenSmtp.Mail;
using System.Collections;

namespace Helper
{
    /// <summary>
    /// email 的摘要说明
    /// </summary>
    public class EmailMan
    {
        Conf conf = Conf.CreateFromWebConfig();
        public EmailMan() { }
        /// <summary>
        /// a simple way to send a single email using OpenSmtp.
        /// the sender is configured in Web.config
        /// </summary>
        /// <param name="strTo">the recipicent</param>
        /// <param name="strSubject">the subject</param>
        /// <param name="strContent">the content</param>
        /// <returns></returns>
        public string SendEmail(string strTo, string strSubject, string strContent)
        {
            ArrayList al = new ArrayList(new EmailAddress[] { new EmailAddress(strTo) });
            return SendEmail(conf.get("email") as string, al, strSubject, strContent);
        }
        /// <summary>
        /// send one or more emails
        /// </summary>
        /// <param name="from">sender's email</param>
        /// <param name="arTo">an array of EmailAddress objects to receive</param>
        /// <param name="subject">the subject</param>
        /// <param name="body">the body</param>
        /// <returns>null on success, or an error description when failed</returns>
        public string SendEmail(string from, ArrayList arTo, string subject, string body)
        {
            try
            {
                // no email format verification
                SmtpConfig.VerifyAddresses = false;
                // the message to send
                MailMessage msg = new MailMessage();
                msg.Charset = "gbk";
                msg.From = new EmailAddress(from, Conf.CreateFromWebConfig().get("AppNameShort") as string);
                msg.To = arTo;
                msg.Subject = subject;
                msg.Body = body;
                // send it
                new Smtp
                    (conf.get("primarySmtpHost") as string,
                    conf.get("emailAccountName") as string,
                    conf.get("emailAccountPwd") as string,
                    Helper.Function.IntSafeConvert(conf.get("primarySmtpPort")))
                    .SendMail(msg);

                return null;
            }
            catch (Exception e)
            {
                return e.Message + e.StackTrace;
            }

        }
    }

}
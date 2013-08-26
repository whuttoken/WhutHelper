using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Security.Cryptography;
using System.IO;
using System.Text;


namespace Helper
{
    /// <summary>
    /// 64位的DES加密
    /// </summary>
    public sealed class DES : AbstractStringEncryptor
    {
        // key size in bits, default to 64
        int _KeySize = 64;
        DESCryptoServiceProvider des = new DESCryptoServiceProvider();
        AbstractLogger log = AbstractLogger.GetLogger(typeof(DES));
        Conf conf = Conf.CreateFromWebConfig();
        /// <summary>
        /// 构造DES对象，使用在Web.config中指定的加密/解密KEY。
        /// </summary>
        public DES()
        {
            Key = conf.get("site.security.des.key") as string;
            if (string.IsNullOrEmpty(_Key))
            {
                log.Warn("未在Web.config中定义DES的加KEY，请使用site.security.des.key定义。将使用默认KEY=123!");
                Key = "123!";
            }
            // check the key size
            CheckKey();
        }
        /// <summary>
        /// 构筑DES对象，使用指定的加密、解密KEY。
        /// </summary>
        /// <param name="key"></param>
        public DES(string key) 
        {
            Key = key;
            // check key size
            CheckKey();
        }

        void CheckKey()
        {
            _KeySize = _Key.Length * 2 * 8;
            bool bFound = false;
            foreach (KeySizes s in des.LegalKeySizes)
            {
                for (int i = s.MinSize; !bFound && i <= s.MaxSize; i += s.SkipSize)
                {
                    if (i == _KeySize)
                        bFound = true;
                }
                if (bFound) break;
            }
            if (!bFound)
                throw new Exception("指定的key无效。其长度必须合法。");
            des.KeySize = _KeySize;
        }

        // Des Encode & Decode
        //private static         // Create a new DES _key.
        //DESCryptoServiceProvider DES_Key = new DESCryptoServiceProvider();

        /// <summary>
        /// 加密和解密字符串
        /// </summary>
        /// <param name="strText">要加密或者解密的字符串</param>
        /// <param name="bEncrypt">true-加密，false-解密</param>
        /// <returns>如果成功，则返回结果字符串；否则为null。</returns>
        public string EncodeDecode(string strText, bool bEncrypt)
        {
            try
            {
                if (bEncrypt)
                {
                    return Encrypt(strText);
                }
                else
                {
                    return Decrypt(strText);
                }
            }
            catch
            {
                log.Error("无法完成加密或者解密操作。" + strText);
                return null;
            }
        }
        // Encrypt the string.
        //private  byte[] Encrypt(string PlainText, SymmetricAlgorithm _key)
        //{
        //    // Create a memory stream.
        //    MemoryStream ms = new MemoryStream();

        //    // Create a CryptoStream using the memory stream and the 
        //    // CSP DES _key.  
        //    CryptoStream encStream = new CryptoStream(ms, _key.CreateEncryptor(), CryptoStreamMode.Write);

        //    // Create a StreamWriter to write a string
        //    // to the stream.
        //    StreamWriter sw = new StreamWriter(encStream);

        //    // Write the plaintext to the stream.
        //    sw.WriteLine(PlainText);

        //    // Close the StreamWriter and CryptoStream.
        //    sw.Close();
        //    encStream.Close();

        //    // Get an array of bytes that represents
        //    // the memory stream.
        //    byte[] buffer = ms.ToArray();

        //    // Close the memory stream.
        //    ms.Close();

        //    // Return the encrypted byte array.
        //    return buffer;
        //}
        private string _Key = @"qwer\=->";
        public string Key
        {
            set { _Key = value; }
        }
        // Decrypt the byte array.
        //private  string Decrypt(byte[] CypherText, SymmetricAlgorithm _key)
        //{
        //    // Create a memory stream to the passed buffer.
        //    MemoryStream ms = new MemoryStream(CypherText);

        //    // Create a CryptoStream using the memory stream and the 
        //    // CSP DES _key. 
        //    CryptoStream encStream = new CryptoStream(ms, _key.CreateDecryptor(), CryptoStreamMode.Read);

        //    // Create a StreamReader for reading the stream.
        //    StreamReader sr = new StreamReader(encStream);

        //    // Read the stream as a string.
        //    string val = sr.ReadLine();

        //    // Close the streams.
        //    sr.Close();
        //    encStream.Close();
        //    ms.Close();

        //    return val;
        //}

        //加密方法  
        private  string Encrypt(string strToEncrypt)
        {
            //把字符串放到byte数组中  
            byte[] inputByteArray = Encoding.Unicode.GetBytes(strToEncrypt);

            //建立加密对象的密钥和偏移量  
            des.Key = Encoding.Unicode.GetBytes(_Key);
            des.IV = Encoding.Unicode.GetBytes(_Key);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            //Write  the  byte  array  into  the  crypto  stream  
            //(It  will  end  up  in  the  memory  stream)  
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            //Get  the  data  back  from  the  memory  stream,  and  into  a  string  
            StringBuilder ret = new StringBuilder();
            foreach (byte b in ms.ToArray())
            {
                //Format  as  hex  
                ret.AppendFormat("{0:X2}", b);
            }
            return ret.ToString();
        }

        //解密方法  
        private  string Decrypt(string pToDecrypt)
        {
            //Put  the  input  string  into  the  byte  array  
            byte[] inputByteArray = new byte[pToDecrypt.Length / 2];
            for (int x = 0; x < pToDecrypt.Length / 2; x++)
            {
                int i = (Convert.ToInt32(pToDecrypt.Substring(x * 2, 2), 16));
                inputByteArray[x] = (byte)i;
            }

            //建立加密对象的密钥和偏移量，此值重要，不能修改  
            des.Key = Encoding.Unicode.GetBytes(_Key);
            des.IV = Encoding.Unicode.GetBytes(_Key);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            //Flush  the  data  through  the  crypto  stream  into  the  memory  stream  
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();

            //Get  the  decrypted  data  back  from  the  memory  stream  
            //建立StringBuild对象，CreateDecrypt使用的是流对象，必须把解密后的文本变成流对象  

            return Encoding.Unicode.GetString(ms.ToArray());
        }

        //自定义密钥
        public override string Encode(string strInput, string key)
        {
            Key = key;
            return Encrypt(strInput);
        }

        #region  AbstractStringEncryptor 成员

        public override string Encode(string strInput)
        {
            return Encrypt(strInput);
        }

        public override string Decode(string strInput)
        {
            return this.Decrypt(strInput);
        }

        #endregion
    
    }
}
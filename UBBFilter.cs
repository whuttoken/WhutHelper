using System;
using System.Data;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Text;

namespace Helper
{
    /// <summary>
    /// 为UBB标签提供支持。
    /// 将UBB标签转换为对应的HTML标签。
    /// </summary>
    public class UBBFilter : IContentFilter
    {
        /// <summary>
        /// 支持的UBB标签。排列顺序对替换结果有影响。
        /// </summary>
        static readonly string[] _patterns = new string[] 
        { 
            // 粗体
            @"\[b\]([^\[]+?)\[/b\]",
            // 斜体
            @"\[i\]([^\[]+?)\[/i\]",
            // 下划线
            @"\[u\]([^\[]+?)\[/u\]",
            // 字体颜色
            @"\[color=([0-9a-z]{1,15})\]([^\[]+?)\[/color\]",
            // 任意style
            @"\[style=([^\]]{1,80})\]([^\[]+?)\[/style\]",
            // 水平对齐
            @"\[align=([a-z]{1,15})\]([^\[]+?)\[/align\]",
            // 超链接
            @"\[url=(?<href>(http|ftp|https|rtsp|mms|mailto)://[^\]]+)\](?<title>[^\[]+?)\[/url\]"
        };
        /// <summary>
        /// 对应的HTML替换标签，必须和上面的UBB Pattern对应。
        /// </summary>
        static readonly string[] _replacements = new string[] 
        { 
            "<span style=\"font-weight:bold;\">$1</span>",
            "<span style=\"font-style:italic;\">$1</span>",
            "<span style=\"text-decoration:underline;\">$1</span>",
            "<span style=\"color:$1;\">$2</span>",
            "<span style=\"$1\">$2</span>",
            "<div style=\"text-align:$1;\">$2</div>",
            "<a target=\"_blank\" href=\"${href}\" title=\"${title}\">${title}</a>"
        };

        static readonly object[][] _regex_replacement = new object[][]
        {
            new object[]{ _replacements[0], new Regex(_patterns[0], RegexOptions.IgnoreCase | RegexOptions.Compiled) },
            new object[]{ _replacements[1], new Regex(_patterns[1], RegexOptions.IgnoreCase | RegexOptions.Compiled) },
            new object[]{ _replacements[2], new Regex(_patterns[2], RegexOptions.IgnoreCase | RegexOptions.Compiled) },
            new object[]{ _replacements[3], new Regex(_patterns[3], RegexOptions.IgnoreCase | RegexOptions.Compiled) },
            new object[]{ _replacements[4], new Regex(_patterns[4], RegexOptions.IgnoreCase | RegexOptions.Compiled) },
            new object[]{ _replacements[5], new Regex(_patterns[5], RegexOptions.IgnoreCase | RegexOptions.Compiled) },
            new object[]{ _replacements[6], new Regex(_patterns[6], RegexOptions.IgnoreCase | RegexOptions.Compiled) }
        };

        /// <summary>
        /// 
        /// </summary>
        public class UBBTagEvent : EventArgs
        {
            string _tag;
            public string Tag
            {
                get { return _tag; }
                set { _tag = value; }
            }
            bool _skip = false;
            public bool Skip
            {
                get { return _skip; }
                set { _skip = value; }
            }
            string _rep = string.Empty;
            public string Replacement
            {
                get { return _rep; }
                set { _rep = value; }
            }
        }

        public UBBFilter() { }

        #region IContentFilter 成员

        public string Filter(string strInput)
        {
            if (string.IsNullOrEmpty(strInput)) return string.Empty;
            string input = strInput;
            for (int i = 0; i < _regex_replacement.Length; i ++)
            {
                InternalReplace(ref input, _regex_replacement[i][1] as Regex, _regex_replacement[i][0] as string);
            }
            return input;
        }

        private void InternalReplace(ref string strInput, Regex reg, string replace)
        {
            //if (!reg.IsMatch(strInput))
            //    return;
            // 如果没有响应事件则直接替换
            if (HandleTag == null)
                strInput = reg.Replace(strInput, replace);
            else
            {
                // 替换一个个单独的match
                StringBuilder _result = new StringBuilder(strInput);
                Match m = reg.Match(strInput);
                for (int delta = 0; m.Success; m = m.NextMatch())
                {
                    // fire the event before replacing ...
                    UBBTagEvent e = new UBBTagEvent();
                    e.Tag = m.Value;
                    // 此处要先替换在模式中定义的组
                    e.Replacement = m.Result(replace);  
                    HandleTag(this, e);
                    if (!e.Skip)
                    {
                        _result.Replace(m.Value, e.Replacement, m.Index + delta, m.Value.Length);
                        delta += e.Replacement.Length - m.Value.Length;
                    }
                }
                strInput = _result.ToString();
            }
            // recursively ...
            //InternalReplace( ref strInput, reg, replace);
        }

        public event EventHandler HandleTag;

        #endregion
    }
}
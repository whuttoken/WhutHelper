WhutHelper
==========

a Helper lib in C# used in wutnews.net

本文档说明Helper命名空间中的各个类/CS文件的用途。

AbstractLogger.cs 
-------------
定义了可以提供日志功能的类的接口。

DatabaseLogger.cs 
-------------
继承于AbstractLogger，使用数据库实现日志功能。

FileLogger.cs
-------------
继承于AbstractLogger，将日志写入指定的文件或者文件夹。

Conf.cs
-------------
为网站参数配置提供统一接口，并且封装了各个实现类的方法。

AbstractStringEncryptor.cs
-------------
定义了可以对字符串进行加密的类的接口。

DES.cs 
-------------
继承于AbstractStringEncryptor，使用64位的DES对字符串进行加密和解密。

Function.cs 
-------------
提供通用的、常用的方法，例如检查email格式是否正确，以指定长度输出字符串等。数字转换，时间和日期转换，MD5等。

Copying
-------------
Copyright (C) Wuhan Universiy of Technology. wutnews.net, Powered By Token Team



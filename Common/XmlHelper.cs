using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Common
{
    public static class XmlHelper
    {
        /// <summary>
        /// Converts a single XML tree to the type of T
        /// </summary>
        /// <typeparam name="T">Type to return</typeparam>
        /// <param name="xml">XML string to convert</param>
        /// <returns></returns>
        public static T XmlToObject<T>(string xml)
        {
            using (var xmlStream = new StringReader(xml))
            {
                var serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(XmlReader.Create(xmlStream));
            }
        }

        public static T XmlToObject<T>(string xml, XmlReaderSettings s)
        {
            using (var xmlStream = new StringReader(xml))
            {
                var serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(XmlReader.Create(xmlStream, s));
            }
        }

        public static T XmlToObject<T>(string xmlPath, string nodePath)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(xmlPath);

            var returnItemsList = new List<T>();

            var xmlNodeList = xmlDocument.SelectNodes(nodePath);
            if (xmlNodeList != null)
            {
                foreach (XmlNode xmlNode in xmlNodeList)
                {
                    return XmlToObject<T>(xmlNode.OuterXml);
                }
            }
            return default(T);
        }

        /// <summary>
        /// xml字符串转换成指定类型
        /// </summary>
        /// <typeparam name="T">需要转换的类型</typeparam>
        /// <param name="xmlStr">xml字符串</param>
        /// <param name="nodePath">需转换的节点</param>
        /// <returns>T类型实例</returns>
        public static T XmlStrToObject<T>(string xmlStr, string nodePath)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlStr);

            var returnItemsList = new List<T>();

            var xmlNodeList = xmlDocument.SelectNodes(nodePath);
            if (xmlNodeList != null)
            {
                foreach (XmlNode xmlNode in xmlNodeList)
                {
                    return XmlToObject<T>(xmlNode.OuterXml);
                }
            }
            return default(T);
        }

        /// <summary>
        /// 将XM节点转换为对应的实体
        /// 忽略节点名称大小写
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityNode"></param>
        public static T XmlToObjectByNode<T>(XmlNode entityNode) where T : new()
        {
            var obj = new T();
            foreach (var p in typeof(T).GetProperties())
            {
                foreach (XmlNode n in entityNode.ChildNodes)
                {
                    if (p.Name.ToUpper() == n.Name.ToUpper() && !string.IsNullOrEmpty(n.InnerText))
                    {
                        p.SetValue(obj, Convert.ChangeType(n.InnerText, p.PropertyType), null);
                        break;
                    }
                }
            }
            return obj;
        }

        /// <summary>
        /// Converts the XML to a list of T
        /// </summary>
        /// <typeparam name="T">Type to return</typeparam>
        /// <param name="xml">XML string to convert</param>
        /// <param name="nodePath">XML Node path to select <example>//People/Person</example></param>
        /// <returns></returns>
        public static List<T> XmlToObjectList<T>(string xml, string nodePath)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);

            var returnItemsList = new List<T>();

            var xmlNodeList = xmlDocument.SelectNodes(nodePath);
            if (xmlNodeList != null)
            {
                foreach (XmlNode xmlNode in xmlNodeList)
                {
                    returnItemsList.Add(XmlToObject<T>(xmlNode.OuterXml));
                }
            }
            return returnItemsList;
        }

        /// <summary>
        /// 转换xml文件为T类型集合
        /// </summary>
        /// <typeparam name="T">转换后的类型</typeparam>
        /// <param name="xmlPath">xml文件路径</param>
        /// <param name="nodePath">节点名</param>
        /// <returns>T类型集合</returns>
        public static List<T> XmlFileToObjectList<T>(string xmlPath, string nodePath)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(xmlPath);

            var returnItemsList = new List<T>();

            var xmlNodeList = xmlDocument.SelectNodes(nodePath);
            if (xmlNodeList != null)
            {
                foreach (XmlNode xmlNode in xmlNodeList)
                {
                    returnItemsList.Add(XmlToObject<T>(xmlNode.OuterXml));
                }
            }
            return returnItemsList;
        }

        private static void XmlSerializeInternal(Stream stream, object o, Encoding encoding)
        {
            if (o == null)
                throw new ArgumentNullException("o");
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            XmlSerializer serializer = new XmlSerializer(o.GetType(), string.Empty);

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineChars = "\r\n";
            settings.Encoding = encoding;
            settings.IndentChars = "    ";

            using (XmlWriter writer = XmlWriter.Create(stream, settings))
            {
                var ns = new XmlSerializerNamespaces();
                ns.Add(string.Empty, string.Empty);
                serializer.Serialize(writer, o, ns);
                writer.Close();
            }
        }

        /// <summary>
        /// 将一个对象序列化为XML字符串
        /// </summary>
        /// <param name="o">要序列化的对象</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>序列化产生的XML字符串</returns>
        public static string XmlSerialize(object o, Encoding encoding)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                XmlSerializeInternal(stream, o, encoding);

                stream.Position = 0;
                using (StreamReader reader = new StreamReader(stream, encoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// 将一个对象按XML序列化的方式写入到一个文件
        /// </summary>
        /// <param name="o">要序列化的对象</param>
        /// <param name="path">保存文件路径</param>
        /// <param name="encoding">编码方式</param>
        public static void XmlSerializeToFile(object o, string path, Encoding encoding)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            using (FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                XmlSerializeInternal(file, o, encoding);
            }
        }

        /// <summary>
        /// 从XML字符串中反序列化对象
        /// </summary>
        /// <typeparam name="T">结果对象类型</typeparam>
        /// <param name="s">包含对象的XML字符串</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>反序列化得到的对象</returns>
        public static T XmlDeserialize<T>(string s, Encoding encoding)
        {
            if (string.IsNullOrEmpty(s))
                throw new ArgumentNullException("s");
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            XmlSerializer mySerializer = new XmlSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream(encoding.GetBytes(s)))
            {
                using (StreamReader sr = new StreamReader(ms, encoding))
                {
                    return (T)mySerializer.Deserialize(sr);
                }
            }
        }

        /// <summary>
        /// 读入一个文件，并按XML的方式反序列化对象。
        /// </summary>
        /// <typeparam name="T">结果对象类型</typeparam>
        /// <param name="path">文件路径</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>反序列化得到的对象</returns>
        public static T XmlDeserializeFromFile<T>(string path, Encoding encoding)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            string xml = File.ReadAllText(path, encoding);
            return XmlDeserialize<T>(xml, encoding);
        }

        /// <summary>
        /// 将字符串转换为DataTable数据
        /// </summary>
        public static DataTable ExtendInfoDt(string xml)
        {
            DataSet ds = new DataSet();
            if (xml != "")
            {
                StringReader sr = new StringReader(xml);
                XmlDocument xd = new XmlDocument();
                ds.ReadXml(sr);
            }
            if (ds.Tables.Count > 0)
            {
                return ds.Tables[0];
            }
            else
                return null;
        }

        /// <summary>
        /// 读取本地XML文件并转成DataSet[只支持服务器文件]
        /// </summary>
        /// <param name="xmlPath">文件路径</param>
        /// <returns>ds结构数据</returns>
        public static DataSet GetXmlData(string xmlPath)
        {
            DataSet ds = new DataSet();
            try
            {
                if (File.Exists(xmlPath))
                {
                    ds.ReadXml(xmlPath);
                }
            }
            catch (Exception ex)
            {
                ds = null;
            }
            return ds;
        }

        /// <summary>
        /// 读取本地XML文件并转成DataSet[只支持服务器文件]
        /// </summary>
        /// <param name="xmlPath">文件路径</param>
        /// <param name="isFileDependCache">是否缓存</param>
        /// <returns>ds结构数据</returns>
        //public static DataSet GetXmlData(string xmlPath, bool isFileDependCache)
        //{
        //    DataSet ds = null;
        //    if (isFileDependCache)
        //    {
        //        //xmlPath也作为了缓存名称

        //        ds = CacheManager.Get(xmlPath, CacheType.NetCache) as DataSet;
        //        if (ds == null)
        //        {
        //            ds = GetXmlData(xmlPath);
        //            if (ds != null)
        //            {
        //                CacheManager.Set(xmlPath, ds, new System.Web.Caching.CacheDependency(xmlPath));
        //            }
        //        }
        //    }
        //    else
        //    {
        //        ds = GetXmlData(xmlPath);
        //    }
        //    return ds;
        //}

        /// <summary>
        /// 读取本地XML文件并转成实体类
        /// </summary>
        /// <typeparam name="T">实体类类型</typeparam>
        /// <param name="xmlPath">文件路径</param>
        /// <returns>实体类实例</returns>
        public static T DeserializeXmlFile<T>(string xmlPath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (FileStream stream = new FileStream(xmlPath, FileMode.Open, FileAccess.Read)) return (T)serializer.Deserialize(stream);
        }

        /// <summary>
        /// 将XML字符串转成实体类
        /// </summary>
        /// <typeparam name="T">实体类类型</typeparam>
        /// <param name="xml">XML字符串</param>
        /// <returns>实体类实例</returns>
        public static T DeserializeXml<T>(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (TextReader textReader = new StringReader(xml)) return (T)serializer.Deserialize(textReader);
        }

        /// <summary>
        /// Byte[]转换成字符串
        /// </summary>
        private static string Byte2String(byte[] bt)
        {
            Encoding encoding = Encoding.GetEncoding("gb2312");
            string str = encoding.GetString(bt);
            return str;
        }
    }

    /// <summary>
    /// Config文件操作
    /// </summary>
    public static class ConfigHelper
   {
     #region

        static ConcurrentDictionary<string, string> configDictionary = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// 取出配置文件
        /// </summary>
        /// <param name="xmlstr"></param>
        /// <returns></returns>
        public static string GetConfigData(string xmlstr)
        {
            return configDictionary.GetOrAdd(xmlstr, ConfigurationManager.AppSettings[xmlstr]);
        }

        /// <summary>
        /// 取出配置文件(带默认值)
        /// </summary>
        /// <param name="xmlstr">节点名</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>
        public static string GetConfigData(string xmlstr, string defaultValue = "")
        {
            var configStr = configDictionary.GetOrAdd(xmlstr, ConfigurationManager.AppSettings[xmlstr]);
            return string.IsNullOrEmpty(configStr) ? defaultValue : configStr;
        }

        #endregion
     }
}

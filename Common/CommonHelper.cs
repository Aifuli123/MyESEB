using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class CommonHelper
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

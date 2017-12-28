using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFileWpfFileExplorer
{
  public static class AppSettings
  {
      private static readonly Configuration Config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        /// <summary>
        /// Gets the string value from the key in config file
        /// </summary>
        /// <param name="key">app config key</param>
        /// <returns></returns>
      public static string GetKeysValue(string key)
      {
          return Config.AppSettings.Settings[key].Value;
      }
        /// <summary>
        /// Sets the string value from the key in config file to the provided value
        /// </summary>
        /// <param name="key">app config key</param>
        /// <param name="value">value to set to</param>
        public static void SetKeysValue(string key, string value)
      {
          Config.AppSettings.Settings[key].Value = value;
          Config.Save(ConfigurationSaveMode.Modified);
      }

    }
}

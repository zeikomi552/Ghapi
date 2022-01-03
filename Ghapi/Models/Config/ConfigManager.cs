using Ghapi.Models.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ghapi.Models.Config
{
    class ConfigManager : PathManager
    {
        #region Configファイル用フォルダパス
        /// <summary>
        /// Configファイル用フォルダパス
        /// </summary>
        public static string ConfigDir
        {
            get
            {
                string path = GetApplicationFolder();
                string config_dir = Path.Combine(path, "Config");
                CreateDirectory(config_dir);
                return config_dir;
            }
        }
        #endregion
    }
}

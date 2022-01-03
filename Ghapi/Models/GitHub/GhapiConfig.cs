using Ghapi.Models.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ghapi.Models.GitHub
{
    public class GhapiConfig : GhapiBase
    {
        #region キーファイルパス
        /// <summary>
        /// キーファイルパス
        /// </summary>
        public static string KeysFile { get; set; } = Path.Combine(ConfigManager.ConfigDir, "ghapi.keys");
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GhapiConfig()
        {
        }

        /// <summary>
        /// アクセストークン
        /// </summary>
        public string AccessToken { get; set; }

    }
}

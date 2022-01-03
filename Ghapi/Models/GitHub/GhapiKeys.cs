using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ghapi.Models.GitHub
{
    public class GhapiKeys : GhapiBase
    {
        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GhapiKeys()
        {

        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="access_token">アクセストークン</param>
        public GhapiKeys(string access_token, string user_id, string password)
        {
            this.AccessToken = access_token;
        }
        #endregion

        #region アクセストークン
        /// <summary>
        /// アクセストークン
        /// </summary>
        public string AccessToken { get; set; }
        #endregion

    }
}

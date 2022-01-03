using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ghapi.Models.GitHub
{
    public class GhapiCommandOptions : GhapiBase
    {
        /// <summary>
        /// アクセストークン
        /// </summary>
        [EndpointParam("-at")]
        public string AccessToken { get; set; }


        /// <summary>
        /// キーファイルパス
        /// </summary>
        [EndpointParam("-keysfile")]
        public string KeysFile { get; set; }

        /// <summary>
        /// 開発言語指定
        /// </summary>
        [EndpointParam("-language")]
        public string Language { get; set; }

        /// <summary>
        /// ディレクトリ指定
        /// </summary>
        [EndpointParam("-d")]
        public string Directory { get; set; }

        /// <summary>
        /// ファイル名指定
        /// </summary>
        [EndpointParam("-f")]
        public string FileName { get; set; }

        /// <summary>
        /// 検索開始日
        /// </summary>
        [EndpointParam("-fromdate")]
        public string FromDate { get; set; }

        /// <summary>
        /// 検索終了日
        /// </summary>
        [EndpointParam("-todate")]
        public string ToDate { get; set; }

    }
}

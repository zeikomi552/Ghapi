using Ghapi.Models.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ghapi.Models.GitHub
{
    [AttributeUsage(AttributeTargets.Property)]
    class EndpointParamAttribute : Attribute
    {
        public EndpointParamAttribute(string key)
        {
            this.Key = key;
        }
        public string Key { get; set; }
    }


    public class GhapiArgs : GhapiBase
    {

        #region 引数リスト
        /// <summary>
        /// 引数リスト
        /// </summary>
        public static Dictionary<string, string> Args { get; set; } = new Dictionary<string, string>();
        #endregion

        #region コマンドオプション
        /// <summary>
        /// コマンドオプション
        /// </summary>
        public static GhapiCommandOptions CommandOptions { get; set; } = new GhapiCommandOptions();
        #endregion

        #region コマンドライン引数を分解して値を設定する
        /// <summary>
        /// コマンドライン引数を分解して値を設定する
        /// </summary>
        /// <param name="key">コマンド</param>
        /// <param name="value">セットする値</param>
        public static void SetCommandParameter(string key, string value)
        {
            // Get instance of the attribute.
            EndpointParamAttribute myAttribute =
                (EndpointParamAttribute)Attribute.GetCustomAttribute(typeof(GhapiCommandOptions), typeof(EndpointParamAttribute));

            //プロパティ一覧を取得
            var properties = CommandOptions.GetType().GetProperties();

            foreach (var prop in properties)
            {
                // プロパティに付いている属性を取得する
                var endpoint = (Attribute.GetCustomAttributes(
                            typeof(GhapiCommandOptions).GetProperty(prop.Name),
                            typeof(EndpointParamAttribute)) as EndpointParamAttribute[]).FirstOrDefault();

                // 取得内容の確認
                if (endpoint != null)
                {
                    if (endpoint.Key.Equals(key))
                    {
                        prop.SetValue(CommandOptions, (string)value);
                        break;
                    }
                }
            }
        }
        #endregion

        #region Optionかどうかを判断する
        /// <summary>
        /// Optionかどうかを判断する
        /// </summary>
        /// <param name="option">オプション文字列</param>
        /// <returns>true:オプション false:オプションでない</returns>
        public static bool IsOption(string option)
        {
            // Get instance of the attribute.
            EndpointParamAttribute myAttribute =
                (EndpointParamAttribute)Attribute.GetCustomAttribute(typeof(GhapiCommandOptions), typeof(EndpointParamAttribute));

            //プロパティ一覧を取得
            var properties = CommandOptions.GetType().GetProperties();

            foreach (var prop in properties)
            {
                // プロパティに付いている属性を取得する
                var endpoint = (Attribute.GetCustomAttributes(
                            typeof(GhapiCommandOptions).GetProperty(prop.Name),
                            typeof(EndpointParamAttribute)) as EndpointParamAttribute[]).FirstOrDefault();

                // 取得内容の確認
                if (endpoint != null && endpoint.Key.Equals(option))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region コマンドを分解してセットする
        /// <summary>
        /// コマンドを分解してセットする
        /// </summary>
        /// <param name="args">コマンドライン引数</param>
        public static void SetCommand(string[] args)
        {
            // 引数分ループする
            for (int i = 0; i < args.Length; i++)
            {
                var check = (from x in GhapiActions.Actions
                             where x.ActionName.Equals(args[i].ToLower())
                             select x).FirstOrDefault();

                // nullチェック
                if (check != null)
                {
                    // アクションキーが既に登録されていないことを確認する
                    if (!Args.ContainsKey("action"))
                    {
                        // キーの登録
                        Args.Add("action", args.Length > i ? args[i] : string.Empty);
                    }
                }
                else
                {
                    string key = args[i++];
                    string value = args.Length > i ? args[i] : "-";

                    if (GhapiArgs.IsOption(key))
                    {
                        // 同じキーが既に登録されていないことを確認する
                        if (!Args.ContainsKey(key))
                        {
                            // valueが次のオプションになっていないことを確認する
                            if (!GhapiArgs.IsOption(value))
                            {
                                Args.Add(key, value);
                            }
                            else
                            {
                                // valueが次のオプションになっているならハイフンをセットして次へ
                                Args.Add(key, "-");
                                i--;
                            }
                        }
                        else
                        {
                            // 同じキー(オプション)が登録されている場合は無視
                            ;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            // コマンド + オプションリストにセットする
            foreach (var param in GhapiArgs.Args)
            {
                // エンドポイントコマンドに使用するパラメータのセット
                SetCommandParameter(param.Key, param.Value);
            }


            #region キーファイルパス指定
            // オプションにキーファイルパスが指定されている場合は指定されたファイルを優先する
            if (!string.IsNullOrEmpty(GhapiArgs.CommandOptions.KeysFile))
            {
                // キーファイル情報のセット
                GhapiConfig.KeysFile = GhapiArgs.CommandOptions.KeysFile;

            }
            // キーファイルの存在確認
            if (File.Exists(GhapiConfig.KeysFile))
            {
                // キーの読み込み
                GitHubAPI.GhapiKeys = XMLUtil.Deserialize<GhapiKeys>(GhapiConfig.KeysFile);
            }
            #endregion

            #region 各種キー設定
            /* デフォルトファイル → キーファイル → キーコマンドの順で上書きする
             * 何も指定しなければデフォルトのファイルが使用され
             * -keysfileが指定されていればそのファイルパスのキーが使用される
             * 更に-atオプションが指定されていればオプションを優先する
             */
            // URLが指定されている場合
            if (!string.IsNullOrEmpty(GhapiArgs.CommandOptions.AccessToken))
            {
                GitHubAPI.GhapiKeys.AccessToken = GhapiArgs.CommandOptions.AccessToken;
            }

            #endregion
        }
        #endregion
    }
}

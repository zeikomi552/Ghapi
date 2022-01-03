using Ghapi.Models.GitHub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ghapi
{
    public class GhapiCommand : GhapiBase
    {
        #region Action
        /// <summary>
        /// Action
        /// </summary>
        static string _Action = string.Empty;
        #endregion

        #region コマンドライン引数
        /// <summary>
        /// コマンドライン引数
        /// </summary>
        public static GhapiArgs GhapiArgs { get; set; }
        #endregion

        #region コマンドの実行
        /// <summary>
        /// コマンドの実行
        /// </summary>
        /// <param name="args">パラメータ</param>
        public static void ExecuteCommand(string[] args)
        {
            try
            {
                // 引数のセット
                GhapiArgs.SetCommand(args);

                string action = string.Empty;

                if (GhapiArgs.Args.ContainsKey("action"))
                {
                    action = GhapiArgs.Args["action"];
                }
                else
                {
                    Console.WriteLine("不正なコマンドです。\r\n");
                    action = "/?";
                }

                // アクションの実行
                foreach (var tmp in GhapiActions.Actions)
                {
                    if (action.Equals(tmp.ActionName))
                    {
                        tmp.Action(tmp.ActionName);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                Console.WriteLine(e.Message);
                throw;
            }
        }
        #endregion
    }
}

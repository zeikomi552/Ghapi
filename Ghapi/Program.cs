using log4net.Config;
using System;
using System.Text;

namespace Ghapi
{
    class Program : GhapiBase
    {
        static int Main(string[] args)
        {
            // log4netの設定ファイル読み込み処理
            XmlConfigurator.Configure(new System.IO.FileInfo("log4net.config"));

            try
            {
                GhapiCommand.ExecuteCommand(args);
                return 0;
            }
            catch (Exception e)
            {
                StringBuilder msg = new StringBuilder();
                foreach (var arg in args) msg.Append(arg + " ");

                Logger.Error("wpapi " + msg.ToString());
                Logger.Error(e.Message);

                Console.WriteLine("wpapi " + msg.ToString());
                Console.WriteLine(e.Message);
                return -1;
            }
        }
    }
}

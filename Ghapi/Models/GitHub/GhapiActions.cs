using ClosedXML.Excel;
using Ghapi.Models.Config;
using Ghapi.Models.Utilities;
using Markdig;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ghapi.Models.GitHub
{
    public class GhapiActions : GhapiBase
    {
        #region アクション一覧
        /// <summary>
        /// アクション一覧
        /// </summary>
        public static List<GhapiAction> Actions = new List<GhapiAction>()
        {
            new GhapiAction("/?", "ヘルプを表示します",
                Help),
            new GhapiAction("/h", "ヘルプを表示します\r\n",
                Help),
            new GhapiAction("/regist", "各種キーの保存処理" + "\r\n"
                + "\t\t-at アクセストークン(必須)" + "\r\n"
                + "\t\t-keysfile キーファイルの保存先(省略時はデフォルトのパス)" + "\r\n"
                , Regist),
            new GhapiAction("/search", "スター獲得数ランキング100を取得" + "\r\n"
                + "\t\t-lang プログラミング言語を指定(ex.CSharp, CPlusPlus, Python)" + "\r\n"
                , Search),

        };
        #endregion

        #region ヘルプ
        /// <summary>
        /// ヘルプ
        /// </summary>
        /// <param name="action">アクション名</param>
        public static void Help(string action)
        {
            try
            {
                Console.WriteLine("使用方法：");
                Console.WriteLine("\ttwapi /actioncommand [-options]");

                Console.WriteLine("");
                Console.WriteLine("actioncommand :");

                foreach (var tmp in GhapiActions.Actions)
                {
                    Console.WriteLine($"\t{tmp.ActionName}\t...{tmp.Help}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Logger.Error(e.Message);
                throw;
            }
        }
        #endregion

        #region キーの登録処理
        /// <summary>
        /// キーの登録処理
        /// </summary>
        /// <param name="action">アクション名</param>
        public static void Regist(string action)
        {
            try
            {
                XMLUtil.Seialize<GhapiKeys>(GhapiConfig.KeysFile, GitHubAPI.GhapiKeys);
                Console.WriteLine("各種キーを保存しました");
                Console.WriteLine("==>" + GhapiConfig.KeysFile);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        #endregion

        #region 検索結果
        /// <summary>
        /// 検索結果
        /// </summary>
        static List<SearchRepositoryResult> GitHubResult { get; set; } = new List<SearchRepositoryResult>();
        #endregion

        #region 検索言語
        /// <summary>
        /// 検索言語
        /// </summary>
        static Language? SearchLanguage { get; set; }
        #endregion

        #region 検索処理
        /// <summary>
        /// 検索処理
        /// </summary>
        /// <param name="language">対象言語（言語指定無しの場合は何が入っても無視）</param>
        /// <param name="start">開始日</param>
        /// <param name="end">終了日</param>
        /// <param name="page_no">読み込むページ</param>
        /// <returns>Task</returns>
        public static async Task SearchSub(
            Language? language, 
            DateTime start, DateTime end,
            int page_no)
        {
            // GitHub Clientの作成
            var client = new GitHubClient(new ProductHeaderValue("Ghapi"));

            // トークンの取得
            var tokenAuth = new Credentials(GitHubAPI.GhapiKeys.AccessToken);
            client.Credentials = tokenAuth;

            SearchRepositoriesRequest request = new SearchRepositoriesRequest();

#pragma warning disable CS0618 // 型またはメンバーが旧型式です

            request.Created = new DateRange(start, end);
                    //start.HasValue ? start.Value : new DateTime(1970, 1, 1),
                    //start.HasValue ? end.Value : new DateTime(2970, 1, 1));

            // スターの数
            request.Stars = new Octokit.Range(100, int.MaxValue);

            // 読み込むページ
            request.Page = page_no;

            // スターの数でソート
            request.SortField = RepoSearchSort.Stars;

            // 全言語指定の場合は言語指定無し
            if (language != null)
            {
                // 指定された言語をセット
                request.Language = language;
            }

#pragma warning restore CS0618 // 型またはメンバーが旧型式です
            var tmp = await client.Search.SearchRepo(request);

            // リストに結果を追加
            GitHubResult.Add(tmp);
        }

        /// <summary>
        /// 検索処理
        /// </summary>
        /// <param name="action">アクション名</param>
        public static void Search(string action)
        {
            try
            {
                string lang = GhapiArgs.CommandOptions.Language;
                string from_date_str = GhapiArgs.CommandOptions.FromDate;
                string to_date_str = GhapiArgs.CommandOptions.ToDate;

                DateTime from_date = DateTime.TryParse(from_date_str, out from_date) ? from_date : new DateTime(1970, 1, 1);
                DateTime to_date = DateTime.TryParse(to_date_str, out to_date) ? to_date : new DateTime(2970, 1, 1);

                string ftype = "html";

                // ファイルタイプの確認
                if (!string.IsNullOrEmpty(GhapiArgs.CommandOptions.FileType))
                    ftype = GhapiArgs.CommandOptions.FileType;

                // 言語指定されていない場合は全言語対象
                // 言語指定されている場合は指定言語を使用して検索
                SearchLanguage = null;
                if (!string.IsNullOrWhiteSpace(lang))   // 言語指定されていないので全言語とみなす
                {
                    // 指定された言語がGitHubの該当する言語かを確認する
                    foreach (Language value in Enum.GetValues(typeof(Language)))
                    {
                        if (lang.ToLower().Equals(value.ToString().ToLower()))
                        {
                            SearchLanguage = value; // 開発言語をセット
                            break;
                        }
                    }

                    // 言語指定されているが存在しない言語
                    if (!SearchLanguage.HasValue)
                    {
                        Console.WriteLine("指定された開発言語が見つかりませんでした。");
                        return;
                    }
                }

                // ページ数最大値を取得
                int pagemax = int.TryParse(GhapiArgs.CommandOptions.PageMax, out pagemax) ? pagemax : 1;

                // ページ数を満たすまでループ
                for (int page = 1; page <= pagemax; page++)
                {
                    Console.WriteLine($"開発言語:{SearchLanguage} 開始日:{from_date} 終了日:{to_date} ページ番号{page}で検索中");
                    // GitHubランキングの検索
                    SearchSub(SearchLanguage, from_date, to_date, page).Wait();

                    if(GitHubResult.Any())
                    {
                        // トータル数を超えた場合
                        if (GitHubResult.ElementAt(0).TotalCount < page * 100)
                        {
                            break;
                        }
                    }
                }

                // 出力ファイル名を確認
                if (!string.IsNullOrEmpty(GhapiArgs.CommandOptions.FileName))
                {
                    // ディレクトリの作成処理
                    PathManager.CreateCurrentDirectory(GhapiArgs.CommandOptions.FileName);

                    // ファイルタイプ
                    switch (ftype)
                    {
                        case "html":
                        default:
                            {
                                // マークダウンの取得
                                string markdown = GetMarkdown();
                                Markdig.MarkdownPipeline markdownPipeline = new MarkdownPipelineBuilder().UsePipeTables().Build();
                                // Markdown → HTMLへの変換
                                string html = Markdown.ToHtml(markdown.ToString(), markdownPipeline);
                                File.WriteAllText(GhapiArgs.CommandOptions.FileName, html);
                                break;
                            }
                        case "markdown":
                            {
                                // マークダウンの取得
                                string markdown = GetMarkdown();
                                File.WriteAllText(GhapiArgs.CommandOptions.FileName, markdown);
                                break;
                            }
                        case "csv":
                            {
                                // CSVで出力
                                string csv = GetCSV();
                                // SJISで出力
                                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                                File.WriteAllText(GhapiArgs.CommandOptions.FileName, csv, Encoding.GetEncoding("shift_jis"));
                                break;
                            }
                    }
                }
                else
                {
                    Console.WriteLine("ファイル名の指定は必須です");
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        #endregion

        #region 除外するURLのリストを取得する
        /// <summary>
        /// 除外するURLのリストを取得する
        /// </summary>
        /// <param name="path">Excelファイルパス</param>
        /// <returns>URLのリスト</returns>
        private static List<string> GetExcludeURL(string path)
        {
            try
            {
                List<string> urls = new List<string>();
                // 読み取り専用で開く
                using (FileStream fs = new FileStream(path, System.IO.FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    // Bookの操作
                    using (XLWorkbook book = new XLWorkbook(fs, XLEventTracking.Disabled))
                    {
                        // シートの一番目を取得
                        var sheet = book.Worksheets.ElementAt(0);
                        int row = 2;
                        // ループ
                        while (true)
                        {

                            string url = sheet.Cell(row, 1).Value != null ? sheet.Cell(row, 1).Value.ToString() : string.Empty; // メッセージの取得

                            // 空白なら処理を停止
                            if (string.IsNullOrWhiteSpace(url)) { break; }
                            // 除外するURLを追加する
                            else { urls.Add(url); }

                            row++;  // 行をインクリメント
                        }
                    }
                }
                return urls;
            }
            catch
            {
                return new List<string>();
            }
        }
        #endregion

        #region GitHubの記事取得処理
        /// <summary>
        /// GitHubの記事取得処理
        /// </summary>
        /// <returns>GitHub記事(HTML形式)</returns>
        private static string GetMarkdown()
        {
            string from_date_str = GhapiArgs.CommandOptions.FromDate;
            string to_date_str = GhapiArgs.CommandOptions.ToDate;
            string exclusion = GhapiArgs.CommandOptions.Exclusion;

            var list = GetExcludeURL(exclusion);

            DateTime tmp;
            DateTime? from_date = DateTime.TryParse(from_date_str, out tmp) ? tmp : null;
            DateTime? to_date = DateTime.TryParse(to_date_str, out tmp) ? tmp : null;

            StringBuilder markdown = new StringBuilder();

            markdown.AppendLine($"## GitHubサーベイ 調査日{DateTime.Today.ToString("yyyy/MM/dd")}");
            markdown.AppendLine($"### 検索条件");
            string from_text = from_date.HasValue ? from_date.Value.ToString("yyyy/MM/dd") : "指定なし";
            string to_text = to_date.HasValue ? to_date.Value.ToString("yyyy/MM/dd") : "指定なし";
            markdown.AppendLine($"- リポジトリ作成日 {from_text} - {to_text}");
            if (SearchLanguage.HasValue)
            {
                markdown.AppendLine($"- 開発言語 {SearchLanguage.ToString()}");
            }
            else
            {
                markdown.AppendLine($"- 開発言語 ALL");
            }
            markdown.AppendLine();
            markdown.AppendLine($"### 検索結果");
            markdown.AppendLine($"|スター<br>(順位)|リポジトリ名<br>説明|使用言語|検索|");
            markdown.AppendLine($"|----|----|----|----|");

            int rank = 1;
            foreach (var result in GitHubResult)
            {
                // 要素をテキストに変換していく
                foreach (var repo in result.Items)
                {
                    string description = repo.Description.EmptyToText("-").CutText(300).EscapeText();
                    string language = repo.Language.EmptyToText("-").CutText(20);

                    string homepage_url = !string.IsNullOrWhiteSpace(repo.Homepage)                                         // ホームページがセットされているか確認
                        && (repo.Homepage.ToLower().Contains("http://") || repo.Homepage.ToLower().Contains("https://"))    // http://もしくはhttps://ではじまるかを確認
                        && !(from x in list where x.ToLower().Contains(repo.Homepage.ToLower()) select x).Any()              // 除外リストに含まれていないことを確認
                        ? $" [[Home Page]({repo.Homepage})]" : string.Empty;

                    // 行情報の作成
                    markdown.AppendLine($"|<center>{repo.StargazersCount}<br>({rank++}位)</center>|" +
                        $"[{repo.FullName}]({repo.HtmlUrl}){homepage_url}<br>{description}|" +
                        $"{language}|" +
                        $"[[Google](https://www.google.com/search?q={repo.Name})] " +
                        $"[[Qiita](https://qiita.com/search?q={repo.Name})]|");
                }
            }
            return markdown.ToString();
        }
        #endregion

        #region CSVデータの作成処理
        /// <summary>
        /// CSVデータの作成処理
        /// </summary>
        /// <returns>CSVデータ</returns>
        private static string GetCSV()
        {
            string from_date_str = GhapiArgs.CommandOptions.FromDate;
            string to_date_str = GhapiArgs.CommandOptions.ToDate;

            StringBuilder csvtext = new StringBuilder();

            // ヘッダの作成
            csvtext.AppendLine("FullName, description, HtmlUrl, rank, StargazersCount,Homepage, Language, search start, search end");

            int rank = 1;
            foreach (var result in GitHubResult)
            {
                // データ数分ループする
                foreach (var repo in result.Items)
                {
                    // コンテンツの作成
                    csvtext.AppendLine($"{repo.FullName},{CSVUtil.EscapeText(repo.Description)},{repo.HtmlUrl},{rank}," +
                        $"{repo.StargazersCount},{repo.Homepage},{repo.Language}, " +
                        $"{CSVUtil.EscapeText(from_date_str)},{CSVUtil.EscapeText(to_date_str)}");
                    rank++;
                }
            }

            // CSVデータの返却
            return csvtext.ToString();
        }
        #endregion
    }
}

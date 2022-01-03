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

        /// <summary>
        /// 検索結果
        /// </summary>
        static SearchRepositoryResult GitHubResult { get; set; }

        /// <summary>
        /// 検索言語
        /// </summary>
        static Language? SearchLanguage { get; set; }

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
            request.Stars = new Octokit.Range(1, int.MaxValue);

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
            GitHubResult = await client.Search.SearchRepo(request);

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
                bool is_all = string.IsNullOrWhiteSpace(lang);
                string from_date_str = GhapiArgs.CommandOptions.FromDate;
                string to_date_str = GhapiArgs.CommandOptions.ToDate;

                DateTime from_date = DateTime.TryParse(from_date_str, out from_date) ? from_date : new DateTime(1970, 1, 1);
                DateTime to_date = DateTime.TryParse(to_date_str, out to_date) ? to_date : new DateTime(2970, 1, 1);

                foreach (Language value in Enum.GetValues(typeof(Language)))
                {
                    // 言語指定されていない場合は全言語対象
                    // 言語指定されている場合は指定言語を使用して検索
                    if (is_all || lang.ToLower().Equals(value.ToString().ToLower()))
                    {
                        // 検索言語
                        SearchLanguage = is_all ? null : value;

                        // GitHubランキングの検索
                        SearchSub(SearchLanguage, from_date, to_date, 1).Wait();

                        // マークダウンの取得
                        string markdown = GetMarkdown();

                        Markdig.MarkdownPipeline markdownPipeline = new MarkdownPipelineBuilder().UsePipeTables().Build();
                        // Markdown → HTMLへの変換
                        string html = Markdown.ToHtml(markdown.ToString(), markdownPipeline);

                        //Console.WriteLine(markdown);

                        if (!string.IsNullOrEmpty(GhapiArgs.CommandOptions.FileName))
                        {
                            // ディレクトリの作成処理
                            PathManager.CreateCurrentDirectory(GhapiArgs.CommandOptions.FileName);
                            File.WriteAllText(GhapiArgs.CommandOptions.FileName + ".md", markdown);
                            File.WriteAllText(GhapiArgs.CommandOptions.FileName + ".html", html);
                        }

                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        #endregion

        /// <summary>
        /// GitHubの記事取得処理
        /// </summary>
        /// <returns>GitHub記事(HTML形式)</returns>
        private static string GetMarkdown()
        {
            string from_date_str = GhapiArgs.CommandOptions.FromDate;
            string to_date_str = GhapiArgs.CommandOptions.ToDate;

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
            foreach (var repo in GitHubResult.Items)
            {
                string description = repo.Description.EmptyToText("-").CutText(50).Replace("|", "\\/");
                string language = repo.Language.EmptyToText("-").CutText(20);

                string homepage_url = !string.IsNullOrWhiteSpace(repo.Homepage)
                    && (repo.Homepage.ToLower().Contains("http://") || repo.Homepage.ToLower().Contains("https://"))
                    ? $" [[Home Page]({repo.Homepage})]" : string.Empty;

                // 行情報の作成
                markdown.AppendLine($"|<center>{repo.StargazersCount}<br>({rank++}位)</center>|" +
                    $"[{repo.FullName}]({repo.HtmlUrl}){homepage_url}<br>{description}|" +
                    $"{language}|" +
                    $"[[google](https://www.google.com/search?q={repo.Name})] " +
                    $"[[Qiita](https://qiita.com/search?q={repo.Name})]|");
            }

            return markdown.ToString();
        }
    }
}

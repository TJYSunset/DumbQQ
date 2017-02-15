using System;
using System.IO;
using DumbQQ.Client;

namespace DumbQQConsoleDemo
{
    public class Program
    {
        private static readonly DumbQQClient Client = new DumbQQClient();
        private static string _qrCodePath;

        public static void Main(string[] args)
        {
            Client.QrCodeDownloaded += (sender, s) =>
            {
                System.Diagnostics.Process.Start(s);
                _qrCodePath = s;
            };
            Client.QrCodeExpired += (sender, s) => {
                try
                {
                    File.Delete(s);
                }
                catch
                {
                    // ignore
                }
                Client.Start();
            };
            Client.LoginFailed += (sender, ex) =>
            {
                Console.WriteLine("按任意键以重新尝试登陆。");
                Console.ReadKey(true);
                Client.Start();
            };
            Client.LoginCompleted += (sender, s) =>
            {
                try
                {
                    File.Delete(_qrCodePath);
                }
                catch
                {
                    // ignore
                }
                Console.WriteLine("是否要导出cookie以便下次登录？[y/n]");
                var response = Console.ReadLine();
                if (response.IsMatch(@"^\s*(y|yes)\s*$", System.Text.RegularExpressions.RegexOptions.IgnoreCase)) Client.Start();
                {
                    try
                    {
                        File.WriteAllText("cookies.json", Client.DumpCookies());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("未成功导出cookie，抛出异常：" + ex);
                    }
                }
                Console.WriteLine("欢迎，" + Client.GetInfoAboutMe().Nickname + "！");
            };
            Client.GroupMessageReceived += (sender, message) =>
            {
                Console.WriteLine("[私聊消息]" + message.Content);
            };
            Client.GroupMessageReceived += (sender, message) =>
            {
                Console.WriteLine("[群消息]" + message.Content);
                if (message.Content.IsMatch(@"^Knock knock[\.!]?$",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    Client.ReplyTo(message, "Who's there?");
            };
            Client.GroupMessageReceived += (sender, message) =>
            {
                Console.WriteLine("[讨论组消息]" + message.Content);
            };
            if (File.Exists("cookies.json"))
            {
                Console.WriteLine("检测到有导出的cookie，是否要通过cookie登录？[y/n]");
                var response = Console.ReadLine();
                if (!response.IsMatch(@"^\s*(y|yes)\s*$", System.Text.RegularExpressions.RegexOptions.IgnoreCase)) Client.Start();
                try
                {
                    var save = File.ReadAllText("cookies.json");
                    Client.Start(save);
                }
                catch
                {
                    Client.Start();
                }
            }
            else
            {
                Client.Start();
            }
        }
    }
}
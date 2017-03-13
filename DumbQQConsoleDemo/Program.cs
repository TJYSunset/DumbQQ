using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using DumbQQ.Client;

namespace DumbQQConsoleDemo
{
    public class Program
    {
        private const string CookiePath = "dump.json";
        private static readonly DumbQQClient Client = new DumbQQClient {CacheTimeout = TimeSpan.FromDays(1)};

        public static void Main(string[] args)
        {
            // 好友消息回调
            Client.FriendMessageReceived += (sender, message) =>
            {
                var s = message.Sender;
                Console.WriteLine($"{s.Alias ?? s.Nickname}:{message.Content}");
            };
            // 群消息回调
            Client.GroupMessageReceived += (sender, message) =>
            {
                var s = message.Sender;
                Console.WriteLine($"[{message.Group.Name}]{s.Alias ?? s.Nickname}:{message.Content}");
                if (message.Content.IsMatch(@"^\s*Knock knock\s*$"))
                    message.Reply("Who's there?");
            };
            // 讨论组消息回调
            Client.DiscussionMessageReceived +=
                (sender, message) =>
                {
                    Console.WriteLine($"[{message.Discussion.Name}]{message.Sender.Nickname}:{message.Content}");
                };
            // 消息回显
            Client.MessageEcho += (sender, e) => { Console.WriteLine($"{e.Target.Name}>{e.Content}"); };
            if (File.Exists(CookiePath))
            {
                // 尝试使用cookie登录
                if (Client.Start(File.ReadAllText(CookiePath)) != DumbQQClient.LoginResult.Succeeded)
                    QrLogin();
            }
            else
            {
                QrLogin();
            }
            Console.WriteLine($"欢迎，{Client.Nickname}!");
            // 导出cookie
            try
            {
                File.WriteAllText(CookiePath, Client.DumpCookies());
            }
            catch
            {
                // Ignored
            }
            // 防止程序终止
            while (Client.Status == DumbQQClient.ClientStatus.Active)
            {
            }
        }

        private static void QrLogin()
        {
            while (true)
                switch (Client.Start(path => Process.Start(path)))
                {
                    case DumbQQClient.LoginResult.Succeeded:
                        return;
                    case DumbQQClient.LoginResult.QrCodeExpired:
                        continue;
                    default:
                        Console.WriteLine("登录失败，需要重试吗？(y/n)");
                        var response = Console.ReadLine();
                        if (response.IsMatch(@"^\s*y(es)?\s*$", RegexOptions.IgnoreCase))
                            continue;
                        Environment.Exit(1);
                        return;
                }
        }
    }
}
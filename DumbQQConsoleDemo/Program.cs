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
                Console.WriteLine("欢迎，" + Client.GetInfoAboutMe().Nickname + "！");
            };
            Client.GroupMessageReceived += (sender, message) =>
            {
                if (message.Content.IsMatch(@"^Knock knock[\.!]?$",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    Client.ReplyTo(message, "Who's there?");
            };
            Client.Start();
        }
    }
}
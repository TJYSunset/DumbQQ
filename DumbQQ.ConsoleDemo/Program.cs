using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using DumbQQ.Models.Utilities;
using MoreLinq;

namespace DumbQQ.ConsoleDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ((string, string, ulong, string), CookieContainer) ReadSession()
            {
                using (var stream = new FileStream(@"session.tmp", FileMode.Open))
                {
                    return (((string, string, ulong, string), CookieContainer)) new BinaryFormatter().Deserialize(
                        stream);
                }
            }

            ((string, string, ulong, string), CookieContainer) Authenticate()
            {
                var session = DumbQQClient.QrAuthenticate(x =>
                {
                    File.WriteAllBytes(@"qrcode.png", x);
                    Process.Start(@"qrcode.png");
                    Console.WriteLine("Waiting for authentication...");
                });

                using (var stream = new FileStream(@"session.tmp", FileMode.OpenOrCreate))
                {
                    new BinaryFormatter().Serialize(stream, session);
                }

                return session;
            }

            var client = new DumbQQClient();
            try
            {
                client.Session = File.Exists(@"session.tmp") ? ReadSession() : Authenticate();
            }
            catch
            {
                client.Session = Authenticate();
            }

            Console.WriteLine(@"Logged in!");

            client.FriendCategories.Values.ForEach(x => Console.WriteLine(x.Name));
            client.Friends.Values.ForEach(x => Console.WriteLine(x.Name));
            client.Groups.Values.ForEach(x => Console.WriteLine(x.Name));
            client.Groups.Values.FirstOrDefault()?.Members.ForEach(x => Console.WriteLine(x.Value.NameAlias ?? x.Value.Name));
            client.Discussions.Values.ForEach(x => Console.WriteLine(x.Name));
            while (true)
            {
                try
                {
                    Console.Write(".");
                    client.Poll().ForEach(x => Console.WriteLine(x.Content));
                }
                catch (ApiException ex) when (ex.Code == 100100)
                {
                    Console.Write("*");
                }
            }
        }
    }
}
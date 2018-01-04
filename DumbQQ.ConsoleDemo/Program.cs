using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using DumbQQ.PasswordAuthentication;
using MoreLinq;

namespace DumbQQ.ConsoleDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var client = new DumbQQClient
            {
                Session = DumbQQClient.QrAuthenticate(x =>
                {
                    File.WriteAllBytes(@"temp.png", x);
                    Process.Start(@"temp.png");
                    Console.WriteLine(@"Waiting for manual authentication...");
                })
            };
            Console.WriteLine(@"Logged in!");

            client.Groups.Values.ForEach(x => Console.WriteLine(x.Name));
            client.Friends.Values.ForEach(x => Console.WriteLine(x.Name));
            client.Discussions.Values.ForEach(x => Console.WriteLine(x.Name));
            while (true)
            {
                Console.Write(".");
                client.Poll().ForEach(x => Console.WriteLine(x.Content));
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using DumbQQ.Models.Abstract;
using MoreLinq;
using RestSharp;
using SimpleJson;

namespace DumbQQ.Utils
{
    public static class ExtensionMethods
    {
        private static readonly Regex UrlPattern = new Regex(@"^(?<domain>https?://.*?)/(?<path>.*)$");

        private static (string domain, string path) SeperateDomain(string url)
        {
            var match = UrlPattern.Match(url);
            return (match.Groups[@"domain"].Value, match.Groups[@"path"].Value);
        }

        internal static IRestResponse Get(this RestClient client, (string url, string referer) api,
            params object[] parameters)
        {
            var (domain, path) = SeperateDomain(string.Format(api.url, parameters));
            var request = new RestRequest(path);
            if (api.referer != null) request.AddHeader(@"Referer", api.referer);
            client.BaseUrl = new Uri(domain);
            return client.Get(request);
        }

        internal static IRestResponse<T> Get<T>(this RestClient client, (string url, string referer) api,
            params object[] parameters) where T : new()
        {
            var (domain, path) = SeperateDomain(string.Format(api.url, parameters));
            var request = new RestRequest(path)
                .AddHeader(@"Connection", @"Keep-Alive");
            if (api.referer != null) request.AddHeader(@"Referer", api.referer);
            client.BaseUrl = new Uri(domain);
            return client.Get<T>(request);
        }

        internal static IRestResponse Post(this RestClient client, (string url, string referer) api,
            JsonObject json)
        {
            var (domain, path) = SeperateDomain(api.url);
            var request = new RestRequest(path)
                .AddHeader(@"Referer", api.referer)
                .AddHeader(@"Origin", api.url.Substring(0, api.url.LastIndexOf('/')))
                .AddParameter(@"r", json, ParameterType.GetOrPost);
            client.BaseUrl = new Uri(domain);
            return client.Post(request);
        }

        internal static IRestResponse<T> Post<T>(this RestClient client, (string url, string referer) api,
            JsonObject json) where T : new()
        {
            var (domain, path) = SeperateDomain(api.url);
            var request = new RestRequest(path)
                .AddHeader(@"Referer", api.referer)
                .AddHeader(@"Origin", api.url.Substring(0, api.url.LastIndexOf('/')))
                .AddParameter(@"r", json, ParameterType.GetOrPost);
            client.BaseUrl = new Uri(domain);
            return client.Post<T>(request);
        }

        internal static Dictionary<TKey, TValue> Reassemble<TKey, TValue>(this IEnumerable<TValue> source,
            Func<TValue, TKey> selector, DumbQQClient client, params IEnumerable<TValue>[] parts)
            where TValue : IClientExclusive
        {
            // maps the first parts to a dictionary
            var dictionary = source
                .Select(x => new KeyValuePair<TKey, List<TValue>>(selector(x), new List<TValue> {x}))
                .ToDictionary();

            // get list of properties once
            var properties = typeof(TValue).GetProperties();

            // find all parts of the same object
            foreach (var list in parts)
            {
                foreach (var part in list)
                {
                    var key = selector(part);
                    if (dictionary.ContainsKey(key)) dictionary[key].Add(part);
                    else dictionary[key] = new List<TValue> {part};
                }
            }

            // reassemble
            return dictionary
                .Select(x =>
                {
                    var list = x.Value.ToArray();
                    var obj = list.First();

                    foreach (var property in properties)
                    {
                        property.SetValue(obj,
                            list.Select(y => property.GetValue(y)).SkipUntil(y => y != null)
                                .FirstOrDefault()); // default value will always be accepted, so make sure all properties are nullable and/or the collection with the greatest coverage is passed as `source`.
                    }

                    obj.Client = client;

                    return new KeyValuePair<TKey, TValue>(x.Key, obj);
                })
                .ToDictionary();
        }
    }
}
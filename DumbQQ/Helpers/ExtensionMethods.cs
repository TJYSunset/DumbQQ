using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using DumbQQ.Models.Abstract;
using DumbQQ.Models.Utilities;
using MoreLinq;
using RestSharp;
using SimpleJson;

namespace DumbQQ.Helpers
{
    public static class ExtensionMethods
    {
        private static readonly Regex UrlPattern = new Regex(@"^(?<domain>https?://.*?)/(?<path>.*)$");
        private static readonly string[] ContentTypeInsepctionTargets = {@"json", @"text/plain"};

        internal static (string domain, string path) SeperateDomain(string url)
        {
            var match = UrlPattern.Match(url);
            return (match.Groups[@"domain"].Value, match.Groups[@"path"].Value);
        }

        internal static IRestResponse<T> Inspect<T>(this IRestResponse<T> response) where T : Response
        {
            if (!ContentTypeInsepctionTargets.Any(x => response.ContentType.Contains(x))) return response;
            if (!response.IsSuccessful)
                throw new HttpRequestException(
                    $"HTTP request unsuccessful: status code {response.StatusCode}. See inner exception (if exists) for details.",
                    response.ErrorException);
            if ((response.Data?.Code ?? 0) != 0)
                throw new ApiException($"Request unsuccessful: returned {response.Data?.Code}", response.Data?.Code,
                    response.ErrorException);
            return response;
        }

        internal static IRestResponse<Response> Get(this RestClient client, (string url, string referer) api,
            params object[] parameters)
        {
            return client.Get<Response>(api, parameters);
        }

        internal static IRestResponse<T> Get<T>(this RestClient client, (string url, string referer) api,
            params object[] parameters) where T : Response, new()
        {
            var (domain, path) = SeperateDomain(string.Format(api.url, parameters));
            var request = new RestRequest(path)
                .AddHeader(@"Connection", @"Keep-Alive");
            if (api.referer != null) request.AddHeader(@"Referer", api.referer);
            client.BaseUrl = new Uri(domain);
            return client.Get<T>(request).Inspect();
        }

        internal static IRestResponse<Response> Post(this RestClient client, (string url, string referer) api,
            JsonObject json)
        {
            return client.Post<Response>(api, json);
        }

        internal static IRestResponse<T> Post<T>(this RestClient client, (string url, string referer) api,
            JsonObject json) where T : Response, new()
        {
            var (domain, path) = SeperateDomain(api.url);
            var request = new RestRequest(path)
                .AddHeader(@"Referer", api.referer)
                .AddHeader(@"Origin", api.url.Substring(0, api.url.LastIndexOf('/')))
                .AddParameter(@"r", json, ParameterType.GetOrPost);
            client.BaseUrl = new Uri(domain);
            return client.Post<T>(request).Inspect();
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
            var properties = typeof(TValue).GetProperties()
                .Where(x => !Attribute.IsDefined(x, typeof(LazyPropertyAttribute)));

            // find all parts of the same object
            foreach (var list in parts)
            foreach (var part in list)
            {
                var key = selector(part);
                if (dictionary.ContainsKey(key)) dictionary[key].Add(part);
                else dictionary[key] = new List<TValue> {part};
            }

            // reassemble
            return dictionary
                .Select(x =>
                {
                    var list = x.Value.ToArray();
                    var obj = list.First();

                    foreach (var property in properties)
                        property.SetValue(obj,
                            list.Select(y => property.GetValue(y))
                                .FirstOrDefault(y =>
                                    property.PropertyType.IsValueType
                                        ? y != Activator.CreateInstance(property.PropertyType)
                                        : y != null));

                    obj.Client = client;

                    return new KeyValuePair<TKey, TValue>(x.Key, obj);
                })
                .ToDictionary();
        }
    }
}
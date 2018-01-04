using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DumbQQ.Models.Abstract;
using MoreLinq;
using RestSharp;
using SimpleJson;

namespace DumbQQ.Utils
{
    public static class ExtensionMethods
    {
        internal static RestRequest Get(this (string url, string referer) api, params object[] parameters)
        {
            var request = new RestRequest(new Uri(string.Format(api.url, parameters)));
            if (api.referer != null) request.AddHeader("Referer", api.referer);
            return request;
        }

        internal static RestRequest Post(this (string url, string referer) api, JsonObject json)
        {
            return (RestRequest) new RestRequest(new Uri(api.url))
                .AddHeader("Referer", api.referer)
                .AddHeader("Origin", api.url.Substring(0, api.url.LastIndexOf('/')))
                .AddBody($"r={HttpUtility.UrlEncode(json.ToString())}");
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
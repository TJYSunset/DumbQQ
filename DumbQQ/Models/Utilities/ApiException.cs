using System;
using System.Net.Http;

namespace DumbQQ.Models.Utilities
{
    public class ApiException : HttpRequestException
    {
        public ApiException(string message, int? code, Exception inner = null) : base(message, inner)
        {
            Code = code;
        }

        public int? Code { get; }
    }
}
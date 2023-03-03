using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;

namespace vp.utilities
{
    public class QueryHelper
    {
        public static string GetStringValue(IQueryCollection query, string key) {
            try
            {
                StringValues _query;
                query.TryGetValue(key, out _query);
                return _query[0];
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to parse param {(key ?? "No Key provided")}", e);
            }
            
        }

        public static int GetIntValue(IQueryCollection query, string key) {
            string result = GetStringValue(query, key);
            return int.Parse(result);
        }
    }
}

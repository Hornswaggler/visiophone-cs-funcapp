using System.Collections.Generic;
using vp.models;

namespace vp.DTO
{
    public class SearchQueryResult<T>
    {
        public List<T> data { get; set; }

        public int nextResultIndex { get; set; }
    }
}

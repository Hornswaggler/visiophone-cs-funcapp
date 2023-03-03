using System.Collections.Generic;
using vp.models;

namespace vp.functions.samplepack{
    public class SearchQueryResult<T>
    {
        public List<T> data { get; set; }

        public int nextResultIndex { get; set; }
    }
}

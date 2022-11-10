using System.Collections.Generic;
using vp.models;

namespace vp.DTO
{
    public class SampleQueryResult
    {
        public List<SampleModel> samples { get; set; }

        public int nextResultIndex { get; set; }
    }
}

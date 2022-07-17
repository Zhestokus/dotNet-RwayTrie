using System.Collections.Generic;

namespace dotNet_RwayTrie.DataStructures.RWayTrieDt
{
    public class RWayNodeDt<TVal>
    {
        public byte Key;
        public TVal Val;

        public bool IsLeaf;

        public IDictionary<byte, RWayNodeDt<TVal>> Children;
    }
}

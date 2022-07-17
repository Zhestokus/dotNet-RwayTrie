namespace dotNet_RwayTrie.DataStructures.RWayTrieStd
{
    public class RWayNodeStd<TVal>
    {
        public byte Key;
        public TVal Val;

        public bool IsLeaf;

        public RWayNodeStd<TVal>[] Children;
    }
}

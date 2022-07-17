namespace dotNet_RwayTrie.DataStructures.RedBlackTree
{
    public class RbNode<TKey, TVal>
    {
        public TKey Key;
        public TVal Val;

        public bool IsRed;

        public RbNode<TKey, TVal> Left;
        public RbNode<TKey, TVal> Right;

        public RbNode(TKey key, TVal val)
        {
            Key = key;
            Val = val;
            IsRed = true;
        }

        public RbNode(TKey key, TVal val, bool isRed)
        {
            Key = key;
            Val = val;
            IsRed = isRed;
        }
    }

}
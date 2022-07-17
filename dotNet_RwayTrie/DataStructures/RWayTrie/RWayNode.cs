using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dotNet_RwayTrie.DataStructures.RedBlackTree;

namespace dotNet_RwayTrie.DataStructures.RWayTrie
{

    public class RWayNode<TVal>
    {
        public byte Key;
        public TVal Val;

        public bool IsLeaf;

        public RbTree<byte, RWayNode<TVal>> Children;
    }
}

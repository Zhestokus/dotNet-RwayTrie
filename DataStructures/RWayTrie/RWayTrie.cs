using dotNet_RwayTrie.DataStructures.RedBlackTree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace dotNet_RwayTrie.DataStructures.RWayTrie
{
    public class RWayTrie<TVal> : IEnumerable<KeyValuePair<byte[], TVal>>
    {
        private RWayNode<TVal> _root;
        private Encoding _encoding;

        public RWayTrie()
        {
            _encoding = Encoding.UTF8;
        }
        public RWayTrie(Encoding encoding)
        {
            _encoding = encoding;
        }

        public TVal this[byte[] key]
        {
            get
            {
                var node = FindNode(key);
                if (node == null)
                    throw new Exception();

                return node.Val;
            }
            set
            {
                Insert(key, value, true);
            }
        }
        public TVal this[String key]
        {
            get
            {
                var node = FindNode(key);
                if (node == null)
                    throw new Exception();

                return node.Val;
            }
            set
            {
                Insert(key, value, true);
            }
        }

        public IEnumerable<byte[]> GetKeys()
        {
            foreach (var (key, strKey) in GetKeys(false))
                yield return key;
        }
        public IEnumerable<(byte[], String)> GetKeys(bool stringify)
        {
            var pairs = InOrderEnumerate(stringify);
            foreach (var item in pairs)
                yield return item.Key;
        }

        public IEnumerable<TVal> GetValues()
        {
            var pairs = InOrderEnumerate(false);
            foreach (var item in pairs)
                yield return item.Value;
        }

        public bool Insert(byte[] key, TVal val)
        {
            return Insert(key, val, false);
        }
        public bool Insert(String key, TVal val)
        {
            var bytes = _encoding.GetBytes(key);
            return Insert(bytes, val, false);
        }

        public bool Remove(byte[] key)
        {
            var node = FindNode(key);
            if (node == null)
                return false;

            if (!node.IsLeaf)
                return false;

            node.IsLeaf = false;
            return true;
        }
        public bool Remove(String key)
        {
            var bytes = _encoding.GetBytes(key);
            return Remove(bytes);
        }

        public bool Search(byte[] key, out TVal val)
        {
            val = default(TVal);

            var node = FindNode(key);
            if (node == null)
                return false;

            if (!node.IsLeaf)
                return false;

            val = node.Val;
            return true;
        }
        public bool Search(String key, out TVal val)
        {
            var bytes = _encoding.GetBytes(key);
            return Search(bytes, out val);
        }

        public IEnumerable<KeyValuePair<byte[], TVal>> Search(byte[] prefix)
        {
            var parent = FindNode(prefix);

            var stack = new List<byte>();
            var nodes = InOrderTraversal(0, parent, stack);

            foreach (var (key, node) in nodes)
            {
                if (node != null && node.IsLeaf)
                    yield return new KeyValuePair<byte[], TVal>(key, node.Val);
            }
        }
        public IEnumerable<KeyValuePair<byte[], TVal>> Search(String prefix)
        {
            var bytes = _encoding.GetBytes(prefix);
            return Search(bytes);
        }

        public bool Contains(byte[] key)
        {
            var node = FindNode(key);
            if (node != null && node.IsLeaf)
                return true;

            return false;
        }
        public bool Contains(String key)
        {
            var bytes = _encoding.GetBytes(key);
            return Contains(bytes);
        }

        private bool Insert(byte[] key, TVal val, bool update)
        {
            if (_root == null)
                _root = new RWayNode<TVal> { Children = new RbTree<byte, RWayNode<TVal>>() };

            var node = _root;

            foreach (var b in key)
            {
                if (node.Children == null)
                    node.Children = new RbTree<byte, RWayNode<TVal>>();

                if (!node.Children.Search(b, out var rwNode))
                {
                    rwNode = new RWayNode<TVal>();
                    node.Children.Insert(b, rwNode);
                }

                node = rwNode;
            }

            if (node.IsLeaf && !update)
                return false;

            node.Val = val;
            node.IsLeaf = true;

            return true;
        }
        private bool Insert(String key, TVal val, bool update)
        {
            var bytes = _encoding.GetBytes(key);
            return Insert(bytes, val, update);
        }

        private RWayNode<TVal> FindNode(byte[] key)
        {
            var node = _root;

            foreach (var b in key)
            {
                if (node == null || node.Children == null)
                    return null;

                if (!node.Children.Search(b, out var rwNode))
                    return null;

                node = rwNode;
            }

            return node;
        }
        private RWayNode<TVal> FindNode(String key)
        {
            var bytes = _encoding.GetBytes(key);
            return FindNode(bytes);
        }

        public IEnumerator<KeyValuePair<byte[], TVal>> GetEnumerator()
        {
            var pairs = InOrderEnumerate(false);
            foreach (var item in pairs)
            {
                var (key, strKey) = item.Key;
                yield return new KeyValuePair<byte[], TVal>(key, item.Value);
            }
        }
        public IEnumerator<KeyValuePair<(byte[], String), TVal>> GetEnumerator(bool stringify)
        {
            var pairs = InOrderEnumerate(stringify);
            foreach (var item in pairs)
                yield return item;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<(byte[], String), TVal>> InOrderEnumerate(bool stringify)
        {
            if (_root == null)
                yield break;

            var stack = new List<byte>();
            var nodes = InOrderTraversal(0, _root, stack);

            foreach (var (key, node) in nodes)
            {
                if (node != null && node.IsLeaf)
                {
                    if (stringify)
                    {
                        var strKey = _encoding.GetString(key);
                        yield return new KeyValuePair<(byte[], String), TVal>((key, strKey), node.Val);
                    }
                    else
                    {
                        yield return new KeyValuePair<(byte[], String), TVal>((key, null), node.Val);
                    }
                }

            }
        }

        private IEnumerable<(byte[], RWayNode<TVal>)> PreOrderTraversal(byte key, RWayNode<TVal> parent, List<byte> keyBuffer)
        {
            /*
             *  procedure preOrder(node)
             *      if node = null
             *          return
             *          
             *      visit(node)
             *      
             *      preOrder(node.left)
             *      preOrder(node.right)
             */

            if (parent == null)
                yield break;

            yield return (keyBuffer.ToArray(), parent);

            if (parent.Children != null)
            {
                var children = parent.Children;
                foreach (var pair in children)
                {
                    keyBuffer.Add(pair.Key);

                    var nodes = PreOrderTraversal(pair.Key, pair.Value, keyBuffer);
                    foreach (var node in nodes)
                        yield return node;

                    keyBuffer.RemoveAt(keyBuffer.Count - 1);
                }
            }
        }

        private IEnumerable<(byte[], RWayNode<TVal>)> InOrderTraversal(byte key, RWayNode<TVal> parent, List<byte> keyBuffer)
        {
            /*
             *  procedure inOrder(node)
             *      if node = null
             *          return
             *          
             *      inOrder(node.left)
             *      
             *      visit(node)
             *      
             *      inOrder(node.right)
             */

            if (parent == null)
                yield break;

            if (parent.Children != null)
            {
                foreach (var pair in parent.Children)
                {
                    keyBuffer.Add(pair.Key);

                    var children = InOrderTraversal(pair.Key, pair.Value, keyBuffer);
                    foreach (var child in children)
                        yield return child;

                    yield return (keyBuffer.ToArray(), pair.Value);

                    keyBuffer.RemoveAt(keyBuffer.Count - 1);
                }
            }
        }

        private IEnumerable<(byte[], RWayNode<TVal>)> PostOrderTraversal(byte key, RWayNode<TVal> parent, List<byte> keyBuffer)
        {
            /*
             *  procedure postOrder(node)
             *     if node = null
             *         return
             *         
             *     postOrder(node.left)
             *     postOrder(node.right)
             *     
             *     visit(node)
             */

            if (parent == null)
                yield break;

            keyBuffer.Add(key);

            if (parent.Children != null)
            {
                foreach (var pair in parent.Children)
                {
                    var children = PostOrderTraversal(pair.Key, pair.Value, keyBuffer);
                    foreach (var child in children)
                        yield return child;
                }
            }

            yield return (keyBuffer.ToArray(), parent);

            keyBuffer.RemoveAt(keyBuffer.Count - 1);
        }
    }

}

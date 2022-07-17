using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace dotNet_RwayTrie.DataStructures.RWayTrieStd
{
    public class RWayTrieStd<TVal> : IEnumerable<KeyValuePair<byte[], TVal>>
    {
        private RWayNodeStd<TVal> _root;
        private Encoding _encoding;

        public RWayTrieStd()
        {
            _encoding = Encoding.UTF8;
        }
        public RWayTrieStd(Encoding encoding)
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

        private RWayNodeStd<TVal> FindNode(byte[] key)
        {
            var node = _root;

            foreach (var b in key)
            {
                if (node == null || node.Children == null)
                    return null;

                if (node.Children[b] == null)
                    return null;

                node = node.Children[b];
            }

            return node;
        }
        private RWayNodeStd<TVal> FindNode(String key)
        {
            var bytes = _encoding.GetBytes(key);
            return FindNode(bytes);
        }

        private bool Insert(byte[] key, TVal val, bool update)
        {
            if (_root == null)
                _root = new RWayNodeStd<TVal> { Children = new RWayNodeStd<TVal>[256] };

            var node = _root;

            foreach (var b in key)
            {
                if (node.Children == null)
                    node.Children = new RWayNodeStd<TVal>[256];

                if (node.Children[b] == null)
                    node.Children[b] = new RWayNodeStd<TVal>();

                node = node.Children[b];
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

        private IEnumerable<(byte[], RWayNodeStd<TVal>)> PreOrderTraversal(byte key, RWayNodeStd<TVal> parent, List<byte> keyBuffer)
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
                var len = parent.Children.Length;
                for (byte i = 0; i < len; i++)
                {
                    keyBuffer.Add(i);

                    var nodes = PreOrderTraversal(i, parent.Children[i], keyBuffer);
                    foreach (var node in nodes)
                        yield return node;

                    keyBuffer.RemoveAt(keyBuffer.Count - 1);
                }
            }
        }

        private IEnumerable<(byte[], RWayNodeStd<TVal>)> InOrderTraversal(byte key, RWayNodeStd<TVal> parent, List<byte> keyBuffer)
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
                for (byte i = 0; i < parent.Children.Length; i++)
                {
                    keyBuffer.Add(i);

                    var children = InOrderTraversal(i, parent.Children[i], keyBuffer);
                    foreach (var child in children)
                        yield return child;

                    yield return (keyBuffer.ToArray(), parent.Children[i]);

                    keyBuffer.RemoveAt(keyBuffer.Count - 1);
                }
            }
        }

        private IEnumerable<(byte[], RWayNodeStd<TVal>)> PostOrderTraversal(byte key, RWayNodeStd<TVal> parent, List<byte> keyBuffer)
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
                var len = parent.Children.Length;
                for (byte i = 0; i < len; i++)
                {
                    var children = PostOrderTraversal(i, parent.Children[i], keyBuffer);
                    foreach (var child in children)
                        yield return child;
                }
            }

            yield return (keyBuffer.ToArray(), parent);

            keyBuffer.RemoveAt(keyBuffer.Count - 1);
        }
    }
}

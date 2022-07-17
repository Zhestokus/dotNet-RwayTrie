using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;


namespace dotNet_RwayTrie.DataStructures.RedBlackTree
{


    public class RbTree<TKey, TVal> : IEnumerable<KeyValuePair<TKey, TVal>>
    {
        private IComparer<TKey> _comparer;

        private RbNode<TKey, TVal> _root;

        private int _count;

        public RbTree()
        {
            _comparer = Comparer<TKey>.Default;
        }
        public RbTree(IComparer<TKey> comparer)
        {
            if (comparer == null)
                _comparer = Comparer<TKey>.Default;
            else
                _comparer = comparer;
        }

        public TKey Min
        {
            get
            {
                var node = GetMin();
                if (node != null)
                    return node.Key;

                return default(TKey);
            }
        }

        public TKey Max
        {
            get
            {
                var node = GetMax();
                if (node != null)
                    return node.Key;

                return default(TKey);
            }
        }

        public int Count
        {
            get
            {
                return _count;
            }
        }

        public IComparer<TKey> Comparer
        {
            get
            {
                return _comparer;
            }
        }

        public bool Insert(TKey key, TVal val)
        {
            if (_root == null)
            {   // empty tree
                _root = new RbNode<TKey, TVal>(key, val, false);
                _count = 1;

                return true;
            }

            //
            // Search for a node at bottom to insert the new node. 
            // If we can guanratee the node we found is not a 4-node, it would be easy to do insertion.
            // We split 4-nodes along the search path.
            // 
            var current = _root;
            var parent = (RbNode<TKey, TVal>)null;
            var grandParent = (RbNode<TKey, TVal>)null;
            var greatGrandParent = (RbNode<TKey, TVal>)null;

            int order = 0;
            while (current != null)
            {
                order = _comparer.Compare(key, current.Key);
                if (order == 0)
                {
                    // We could have changed root node to red during the search process.
                    // We need to set it to black before we return.
                    _root.IsRed = false;
                    return false;
                }

                // split a 4-node into two 2-nodes                
                if (Is4Node(current))
                {
                    Split4Node(current);
                    // We could have introduced two consecutive red nodes after split. Fix that by rotation.
                    if (IsRed(parent))
                    {
                        InsertionBalance(current, ref parent, grandParent, greatGrandParent);
                    }
                }

                greatGrandParent = grandParent;
                grandParent = parent;
                parent = current;

                current = (order < 0) ? current.Left : current.Right;
            }

            // ready to insert the new node
            var node = new RbNode<TKey, TVal>(key, val);
            if (order > 0)
                parent.Right = node;
            else
                parent.Left = node;

            // the new node will be red, so we will need to adjust the colors if parent node is also red
            if (parent.IsRed)
                InsertionBalance(node, ref parent, grandParent, greatGrandParent);

            _root.IsRed = false;

            ++_count;
            return true;
        }

        public bool Remove(TKey key)
        {
            if (_root == null)
                return false;

            var current = _root;
            var parent = (RbNode<TKey, TVal>)null;
            var grandParent = (RbNode<TKey, TVal>)null;

            var match = (RbNode<TKey, TVal>)null;
            var parentOfMatch = (RbNode<TKey, TVal>)null;

            var foundMatch = false;
            while (current != null)
            {
                if (Is2Node(current))
                { // fix up 2-Node
                    if (parent == null)
                        current.IsRed = true;
                    else
                    {
                        var sibling = GetSibling(current, parent);
                        if (sibling.IsRed)
                        {
                            // If parent is a 3-node, flip the orientation of the red link. 
                            // We can acheive this by a single rotation        
                            // This case is converted to one of other cased below.
                            if (parent.Right == sibling)
                                RotateLeft(parent);
                            else
                                RotateRight(parent);

                            parent.IsRed = true;
                            sibling.IsRed = false;    // parent's color

                            // sibling becomes child of grandParent or root after rotation. Update link from grandParent or root
                            ReplaceChildOfNodeOrRoot(grandParent, parent, sibling);

                            // sibling will become grandParent of current node 
                            grandParent = sibling;
                            if (parent == match)
                                parentOfMatch = sibling;

                            // update sibling, this is necessary for following processing
                            sibling = (parent.Left == current) ? parent.Right : parent.Left;
                        }

                        if (Is2Node(sibling))
                            Merge2Nodes(parent, current, sibling);
                        else
                        {
                            // current is a 2-node and sibling is either a 3-node or a 4-node.
                            // We can change the color of current to red by some rotation.
                            var rotation = RotationNeeded(parent, current, sibling);
                            var newGrandParent = (RbNode<TKey, TVal>)null;
                            switch (rotation)
                            {
                                case 1: // RbTreeRotation.RightRotation:
                                    sibling.Left.IsRed = false;
                                    newGrandParent = RotateRight(parent);
                                    break;
                                case 2: // RbTreeRotation.LeftRotation:
                                    sibling.Right.IsRed = false;
                                    newGrandParent = RotateLeft(parent);
                                    break;
                                case 3: // RbTreeRotation.RightLeftRotation:
                                    newGrandParent = RotateRightLeft(parent);
                                    break;
                                case 4: // RbTreeRotation.LeftRightRotation:
                                    newGrandParent = RotateLeftRight(parent);
                                    break;
                            }

                            newGrandParent.IsRed = parent.IsRed;

                            parent.IsRed = false;
                            current.IsRed = true;

                            ReplaceChildOfNodeOrRoot(grandParent, parent, newGrandParent);

                            if (parent == match)
                                parentOfMatch = newGrandParent;

                            grandParent = newGrandParent;
                        }
                    }
                }

                var order = foundMatch ? -1 : _comparer.Compare(key, current.Key);
                if (order == 0)
                {
                    // save the matching node
                    foundMatch = true;
                    match = current;
                    parentOfMatch = parent;
                }

                grandParent = parent;
                parent = current;

                if (order < 0)
                    current = current.Left;
                else
                    current = current.Right;       // continue the search in  right sub tree after we find a match
            }

            // move successor to the matching node position and replace links
            if (match != null)
            {
                ReplaceNode(match, parentOfMatch, parent, grandParent);
                --_count;
            }

            if (_root != null)
                _root.IsRed = false;

            return foundMatch;
        }

        public void Clear()
        {
            _root = null;
            _count = 0;

            _root = null;
            _count = 0;
        }

        public bool Contains(TKey key)
        {
            return FindNode(key) != null;
        }

        public bool Search(TKey key, out TVal val)
        {
            val = default(TVal);

            var node = FindNode(key);
            if (node == null)
                return false;

            val = node.Val;
            return true;
        }

        public IEnumerable<KeyValuePair<TKey, TVal>> Search(TKey from, TKey to)
        {
            var nodes = FindRange(from, to);
            foreach (var item in nodes)
                yield return new KeyValuePair<TKey, TVal>(item.Key, item.Val);
        }

        private RbNode<TKey, TVal> FindNode(TKey key)
        {
            return FindNode(_root, key);
        }
        private RbNode<TKey, TVal> FindNode(RbNode<TKey, TVal> node, TKey key)
        {
            var current = node;
            while (current != null)
            {
                int order = _comparer.Compare(key, current.Key);
                if (order == 0)
                    return current;
                else
                    current = (order < 0) ? current.Left : current.Right;
            }

            return null;
        }

        private IEnumerable<RbNode<TKey, TVal>> FindRange(TKey from, TKey to)
        {
            foreach (var item in FindRange(_root, from, to))
                yield return item;
        }
        private IEnumerable<RbNode<TKey, TVal>> FindRange(RbNode<TKey, TVal> node, TKey from, TKey to)
        {
            if (node == null)
                yield break;

            var orderFrom = _comparer.Compare(node.Key, from);
            var orderTo = _comparer.Compare(node.Key, to);

            if (orderFrom > 0)
            {
                var nodes = FindRange(node.Left, from, to);
                foreach (var item in nodes)
                    yield return item;
            }

            if (orderFrom >= 0 && orderTo <= 0)
                yield return node;

            if (orderTo < 0)
            {
                var nodes = FindRange(node.Right, from, to);
                foreach (var item in nodes)
                    yield return item;
            }
        }

        public IEnumerator<KeyValuePair<TKey, TVal>> GetEnumerator()
        {
            var inOrder = InOrderTraversal(_root);
            foreach (var node in inOrder)
                yield return new KeyValuePair<TKey, TVal>(node.Key, node.Val);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private RbNode<TKey, TVal> GetMin()
        {
            var node = _root;

            while (node != null)
            {
                if (node.Left == null)
                    return node;

                node = node.Left;
            }

            return null;
        }

        private RbNode<TKey, TVal> GetMax()
        {
            var node = _root;

            while (node != null)
            {
                if (node.Right == null)
                    return node;

                node = node.Right;
            }

            return null;
        }

        private IEnumerable<RbNode<TKey, TVal>> PreOrderTraversal(RbNode<TKey, TVal> parent)
        {
            var stack = new Stack<RbNode<TKey, TVal>>();
            stack.Push(parent);

            while (stack.Count > 0)
            {
                var node = stack.Pop();
                if (node != null)
                {
                    yield return node;

                    if (node.Left != null)
                        stack.Push(node.Left);

                    if (node.Right != null)
                        stack.Push(node.Right);
                }
            }
        }

        private IEnumerable<RbNode<TKey, TVal>> InOrderTraversal(RbNode<TKey, TVal> parent)
        {
            var stack = new Stack<RbNode<TKey, TVal>>();

            var node = parent;

            while (stack.Count > 0 || node != null)
            {
                if (node != null)
                {
                    stack.Push(node);
                    node = node.Left;
                }
                else
                {
                    node = stack.Pop();
                    yield return node;

                    node = node.Right;
                }
            }
        }

        private IEnumerable<RbNode<TKey, TVal>> PostOrderTraversal(RbNode<TKey, TVal> parent)
        {
            var stack = new Stack<RbNode<TKey, TVal>>();

            var node = parent;
            var last = (RbNode<TKey, TVal>)null;

            while (stack.Count > 0 || node != null)
            {
                if (node != null)
                {
                    stack.Push(node);
                    node = node.Left;
                }
                else
                {
                    var peekNode = stack.Peek();
                    if (peekNode.Right != null && peekNode.Right != last)
                        node = node.Right;
                    else
                    {
                        yield return peekNode;
                        last = stack.Pop();
                    }
                }
            }
        }

        private IEnumerable<RbNode<TKey, TVal>> LevelOrderTraversal(RbNode<TKey, TVal> parent)
        {
            if (parent == null)
                yield break;

            var processQueue = new Queue<RbNode<TKey, TVal>>();
            processQueue.Enqueue(parent);

            RbNode<TKey, TVal> current;

            while (processQueue.Count != 0)
            {
                current = processQueue.Dequeue();
                yield return current;

                if (current.Left != null)
                    processQueue.Enqueue(current.Left);

                if (current.Right != null)
                    processQueue.Enqueue(current.Right);
            }
        }


        private RbNode<TKey, TVal> GetSibling(RbNode<TKey, TVal> node, RbNode<TKey, TVal> parent)
        {
            if (parent.Left == node)
                return parent.Right;

            return parent.Left;
        }

        private bool Is2Node(RbNode<TKey, TVal> node)
        {
            return IsBlack(node) && IsNullOrBlack(node.Left) && IsNullOrBlack(node.Right);
        }

        private bool Is4Node(RbNode<TKey, TVal> node)
        {
            return IsRed(node.Left) && IsRed(node.Right);
        }

        private bool IsBlack(RbNode<TKey, TVal> node)
        {
            return (node != null && !node.IsRed);
        }

        private bool IsNullOrBlack(RbNode<TKey, TVal> node)
        {
            return (node == null || !node.IsRed);
        }

        private bool IsRed(RbNode<TKey, TVal> node)
        {
            return (node != null && node.IsRed);
        }

        private void Merge2Nodes(RbNode<TKey, TVal> parent, RbNode<TKey, TVal> child1, RbNode<TKey, TVal> child2)
        {
            // combing two 2-nodes into a 4-node
            parent.IsRed = false;
            child1.IsRed = true;
            child2.IsRed = true;
        }

        private void ReplaceChildOfNodeOrRoot(RbNode<TKey, TVal> parent, RbNode<TKey, TVal> child, RbNode<TKey, TVal> newChild)
        {
            if (parent != null)
            {
                if (parent.Left == child)
                    parent.Left = newChild;
                else
                    parent.Right = newChild;
            }
            else
                _root = newChild;
        }

        private void ReplaceNode(RbNode<TKey, TVal> match, RbNode<TKey, TVal> parentOfMatch, RbNode<TKey, TVal> succesor, RbNode<TKey, TVal> parentOfSuccesor)
        {
            if (succesor == match)
                succesor = match.Left;
            else
            {
                if (succesor.Right != null)
                    succesor.Right.IsRed = false;

                if (parentOfSuccesor != match)
                {   // detach succesor from its parent and set its right child
                    parentOfSuccesor.Left = succesor.Right;
                    succesor.Right = match.Right;
                }

                succesor.Left = match.Left;
            }

            if (succesor != null)
                succesor.IsRed = match.IsRed;

            ReplaceChildOfNodeOrRoot(parentOfMatch, match, succesor);
        }

        private void InsertionBalance(RbNode<TKey, TVal> current, ref RbNode<TKey, TVal> parent, RbNode<TKey, TVal> grandParent, RbNode<TKey, TVal> greatGrandParent)
        {
            bool parentIsOnRight = (grandParent.Right == parent);
            bool currentIsOnRight = (parent.Right == current);

            var newChildOfGreatGrandParent = (RbNode<TKey, TVal>)null;

            if (parentIsOnRight == currentIsOnRight)
                newChildOfGreatGrandParent = currentIsOnRight ? RotateLeft(grandParent) : RotateRight(grandParent);
            else
            {
                newChildOfGreatGrandParent = currentIsOnRight ? RotateLeftRight(grandParent) : RotateRightLeft(grandParent);
                parent = greatGrandParent;
            }

            grandParent.IsRed = true;
            newChildOfGreatGrandParent.IsRed = false;

            ReplaceChildOfNodeOrRoot(greatGrandParent, grandParent, newChildOfGreatGrandParent);
        }

        private RbNode<TKey, TVal> RotateLeft(RbNode<TKey, TVal> node)
        {
            var x = node.Right;
            node.Right = x.Left;

            x.Left = node;

            return x;
        }

        private RbNode<TKey, TVal> RotateLeftRight(RbNode<TKey, TVal> node)
        {
            var child = node.Left;
            var grandChild = child.Right;

            node.Left = grandChild.Right;
            grandChild.Right = node;

            child.Right = grandChild.Left;
            grandChild.Left = child;

            return grandChild;
        }

        private RbNode<TKey, TVal> RotateRight(RbNode<TKey, TVal> node)
        {
            var x = node.Left;
            node.Left = x.Right;

            x.Right = node;

            return x;
        }

        private RbNode<TKey, TVal> RotateRightLeft(RbNode<TKey, TVal> node)
        {
            var child = node.Right;
            var grandChild = child.Left;

            node.Right = grandChild.Left;
            grandChild.Left = node;

            child.Left = grandChild.Right;
            grandChild.Right = child;

            return grandChild;
        }

        private int RotationNeeded(RbNode<TKey, TVal> parent, RbNode<TKey, TVal> current, RbNode<TKey, TVal> sibling)
        {
            if (IsRed(sibling.Left))
            {
                if (parent.Left == current)
                    return 3; // RbTreeRotation.RightLeftRotation;

                return 2; // RbTreeRotation.RightRotation;
            }
            else
            {
                if (parent.Left == current)
                    return 1; // RbTreeRotation.LeftRotation;

                return 4; // RbTreeRotation.LeftRightRotation;
            }
        }

        private void Split4Node(RbNode<TKey, TVal> node)
        {
            node.IsRed = true;

            node.Left.IsRed = false;

            node.Right.IsRed = false;
        }
    }
}
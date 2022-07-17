# dotNet-RwayTrie

This is optimised version of data structure Rway Trie  

More information about Rway trie  
https://www.ime.usp.br/~yoshi/Sedgewick/Algs4th/Slides/52Tries.pdf  
https://algs4.cs.princeton.edu/52trie/  

Main problem of Rway Trie is memory consumption.  
Rway Trie takes too much memory because standard RWay trie uses array for store data  
and this is why this data structure is too slow.  

There are 3 implementations of RWay Trie in this application

1. RWayTrieStd - standard implementation of RWayTrie (uses arrays)
2. RWayTrieDt - uses Dictionay (HashMap) instead of array
3. RwayTrie - optimized version - uses RedBlackTree instead or array

On my local computer (CPU: Intel Core i7 2600; RAM: 16GB) output of Tests (see tests in Program.cs) is below

<pre><code>
Inserting 30000 Guid Keys

<b>RWayTrie (Optimized) - INSERT:  286 Ms -    136807240 Bytes -   133600,820 Kb -   130,47 Mb
RWayTrie (Optimized) - SEARCH:  125 Ms -        30000 prefix</b>
GC Collect


RWayTrie (Dt)        - INSERT:  481 Ms -    270447072 Bytes -   264108,469 Kb -  257,918 Mb
RWayTrie (Dt)        - SEARCH:  142 Ms -        30000 prefix
GC Collect

RWayTrie (Std)       - INSERT: 1583 Ms -   2214956272 Bytes -  2163043,234 Kb - 2112,347 Mb
RWayTrie (Std)       - SEARCH:  168 Ms -        30000 prefix
GC Collect
</code></pre>

As you can see optimized RwayTrie uses 16 times less memory then standard RWayTrie and ~2 times less memory then RwayTrie with Dictionary instead of array.
Insert in optimized RwayTrie is ~5 times faster then standard RWayTrie and ~2 times faster then RwayTrie with Dictionary

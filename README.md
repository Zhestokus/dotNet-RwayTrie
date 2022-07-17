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

```
Inserting 30000 Guid Keys

RWayTrie (Optimized) - INSERT:  301 Ms -    136829256 Bytes -   133622,320 Kb -  130,491 Mb
RWayTrie (Optimized) - SEARCH:  113 Ms -        30000 prefix
GC Collect

RWayTrie (Dt)        - INSERT:  464 Ms -    270503704 Bytes -   264163,773 Kb -  257,972 Mb
RWayTrie (Dt)        - SEARCH:   48 Ms -        30000 prefix
GC Collect

RWayTrie (Std)       - INSERT: 1419 Ms -   2215319536 Bytes -  2163397,984 Kb - 2112,693 Mb
RWayTrie (Std)       - SEARCH:   14 Ms -        30000 prefix
GC Collect
```

As you can see optimized RwayTrie uses 16 times less memory then standard RWayTrie and ~2 times less memory then RwayTrie with Dictionary instead of array
Search in optimized RwayTrie is slower then others because (as mentioned above) it uses RedBlackTree, search complexity of which is Log2(N) time 
so search complexity in each node is Log2(256) = 8 is maximum time

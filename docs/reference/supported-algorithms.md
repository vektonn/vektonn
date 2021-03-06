---
title: Supported index algorithms
---

### For dense vectors

These algorithms are based on [Faiss library](https://github.com/facebookresearch/faiss/wiki/Faiss-indexes):

* `FaissIndex.L2` — squared Euclidean (L2) distance.
* `FaissIndex.IP` — this is typically used for maximum inner product search. This is not by itself cosine similarity, unless the vectors are normalized.

By default `FaissIndex`-es are constructed in `Flat` mode, i.e. they implement exhaustive (precise) search.

To use Faiss implementation of [HNSW index](https://arxiv.org/abs/1603.09320) provide `Hnsw_M`, `Hnsw_EfConstruction`, and `Hnsw_EfSearch` parameters.


### For sparse vectors

These algorithms are derived from [PySparNN library](https://github.com/facebookresearch/pysparnn):

* `SparnnIndex.Cosine` — Cosine distance (i.e. `1 - cosine_similarity`).
* `SparnnIndex.JaccardBinary` — Jaccard distance for _binary_ vectors (i.e. vectors whose coordinates have the values 0 or 1).

---
title: Supported algorithms
---

### Dense vectors

These algorithms are based on [Faiss library](https://github.com/facebookresearch/faiss/wiki/Faiss-indexes).

| Algorithm                                                        | Type and Params                                                 |
|------------------------------------------------------------------|-----------------------------------------------------------------|
| Exact Search for Inner Product                                   | `FaissIndex.IP`, no params                                      |
| Exact Search for L2                                              | `FaissIndex.L2`, no params                                      |
| HNSW <BR> (Hierarchical Navigable Small World graph exploration) | `FaissIndex.L2`, params: {"M", "efConstruction", "efSearch" }   |

### Sparce vectors

These algorithms are derived from [PySparNN library](https://github.com/facebookresearch/pysparnn).

| Algorithm                                                        | Type and Params                                                 |
|------------------------------------------------------------------|-----------------------------------------------------------------|
| Cosine Distance                                                  | `SparnnIndex.Cosine`, no params                                 |
| Jaccard Distance for Binary Data                                 | `SparnnIndex.JaccardBinary`, no params                          |

Where `distance = 1 - similarity`.

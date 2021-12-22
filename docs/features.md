---
title: Features
---

# Features


## Ease of use

#### Attributes

We store not only embeddings but also their attributes, which are more interesting since they can use real-world entities from your domain. For example, they can identify objects using their real identificators.

#### Dense and sparse vector support

You can work with vectors of any type. For example, to solve word processing problems, you can use bag-of-words and load appropriate sparse vectors into Vektonn.

#### All set and ready to go

We have a handy [client for Python](https://pypi.org/project/vektonn/) (and also [for .NET](https://www.nuget.org/packages/Vektonn.ApiClient/), btw) along with published [Docker images on DockerHub](https://hub.docker.com/u/vektonn) — everything you need for a [quick start](quick-start.md).


## Performance and scalability

#### Low overhead

We have a very thin and efficient management layer atop of actual binary indexes, so the overhead is almost non-existent.
See [the benchmarks](benchmarks.md).

#### Sharding

For horizontal scaling, you can specify the attributes by which the vectors will be distributed into groups (or index shards). When processing a search query, the results from multiple shards will be automatically combined.

#### Data splitting and filtering

You can adjust the indexing scheme for specific filter values for a more efficient search.


## Vectors' lifecycle management

#### Online changes

We support changing indexes as new data arrives (delete, change, or add data to the index), in parallel with search queries.

#### Seamless versioning

You can expand multiple indexes over a single data source (vectors and attributes) and seamlessly transition to new versions of indexes. You can expand different indexes with different parameters of the same data.

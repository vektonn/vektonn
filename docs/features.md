---
title: Features
---

# Features


## Ease of use

#### Storing metadata (attributes)

We store not only embeddings/vectors themselves, but also their metadata (attributes), which allows you to use real-world entities from your domain. 
For example, attributes can be a real-world identifiers (such as name) or really any custom data you want to store along with vectors.

#### Dense and sparse vector support

You can work with vectors of any type — dense or sparse. 
For example, to solve word processing problems, you can use bag-of-words and load appropriate sparse vectors into Vektonn.

#### All set and ready to go

We have SDKs [for Python](https://pypi.org/project/vektonn/) and [for .NET](https://www.nuget.org/packages/Vektonn.ApiClient/) along with published [Docker images on DockerHub](https://hub.docker.com/u/vektonn) — everything you'll need for a [quick start](quick-start.md).


## Performance and scalability

#### Low overhead

We have a very thin and efficient management layer atop of actual binary indices, so the overhead is pretty low.

#### Sharding

For horizontal scaling, you can specify the attributes by which the vectors will be distributed into groups (or index shards). 
When processing a search query, the results from multiple shards will be automatically combined.

#### Data filtering (splitting)

Each shard can be further split into logical parts for an even more efficient search. 
Just specify _split attributes_ in the indexing scheme, and all the queries for that index will filter out unnecessary data before searching. 
For example, you may efficiently search for some goods in a particular store, or for books written in a specific language.


## Data lifecycle management

#### Online changes

We support changing indices as new data arrives (delete, update, or insert data to the index), concurrently with search queries.

#### Seamless versioning

You can deploy multiple indices over a single data source (containing the same vectors and attributes) and seamlessly transition to their new versions. 
Different indices may have different configuration parameters.

---
layout: default
title: Quick start
nav_order: 1
permalink: /
---


# Quick start


## Overview

Vektonn is a high-performance service for finding [k-nearest neighbors (kNN)](https://en.wikipedia.org/wiki/Nearest_neighbor_search#k-nearest_neighbors) in vector space.  
See [Examples](#examples) for some use cases.

**Features:**
- Support for both dense and sparse vectors.
- Precise and approximate kNN (AkNN) algorithms.
- Scalable architecture that easily supports hundreds-of-GB-worth of vector data.

Vektonn is licensed under [Apache license](https://en.wikipedia.org/wiki/Apache_License), so you may freely use it for commercial purposes.


### Components

There are three main parts of Vektonn: an API, an Index, and a Data Source.

- The **API** has methods for search and uploading vector data. It proxies requests to corresponding Indices and Data Sources. 
- A **Data Source** is where all the vectors' data being persistently stored. Currently, a Data Source is implemented as a topic in Kafka. You must declare a Data Source before uploading any data to it.
- An **Index** is an in-memory snapshot of data in Data Source. It updates asynchronously from a corresponding Data Source. Also needs to be declared before use.
  
A data from a single Data Source can be sharded (split) over several Indices to fit in RAM of hosting nodes.

A single Data Source may have several Indices defined on it with different metrics.


### Repositories

- [**Vektonn**](https://github.com/vektonn/vektonn) is a main repository with implementations for the API, Index shards and Data Sources. It also contains sources for [Vektonn's .NET client](https://www.nuget.org/packages/Vektonn.ApiClient/).
- [**Vektonn-index**](https://github.com/vektonn/vektonn-index) is a .NET library for finding nearest neighbors in vector space. It provides an implementation of basic functionality for Indices: vector storage and retrieval.
- [**Vektonn-client-python**](https://github.com/vektonn/vektonn-client-python) is, unsurprisingly, a Python's client for Vektonn.
- [**Vectonn-examples**](https://github.com/vektonn/vektonn-examples) is a repository with examples in Jupyter's notebooks. [Start here](#examples) to see Vektonn in action.  



## Running Vektonn

### Prerequisites

We provide Docker images for Vektonn through [Docker Hub](https://hub.docker.com/u/vektonn).
You'll need [Docker installed](https://docs.docker.com/get-docker/) to run Vektonn locally from these images.

_[Optionally]_ For building the project from source see instructions in individual repositories. You'll need to have [.NET SDK](https://dotnet.microsoft.com/download) (not just runtime) version 5 or greater. 


### Examples <a name="examples"></a>

1. [Hotels](https://github.com/vektonn/vektonn-examples/blob/master/jupyter-notebooks/hotels/hotels.ipynb).
Task: save the user's time while searching for hotels. 
This example uses vectorized user reviews to find hotels.

1. [Price Match Guarantee](https://github.com/vektonn/vektonn-examples/blob/master/jupyter-notebooks/cv/cv.ipynb).
Task: help the seller establish a competitive price for their product on the marketplace.
This example uses data from [Shopee kaggle competiton](https://www.kaggle.com/c/shopee-product-matching/overview/description).

1. [Books](https://github.com/vektonn/vektonn-examples/blob/master/jupyter-notebooks/sparse-vectors/sparse-vectors.ipynb).
Task: find similar books by user reviews. This example uses sparse vectors.

All examples are located in [vektonn-examples](https://github.com/vektonn/vektonn-examples). It's the only repository you need to clone to try Vektonn.






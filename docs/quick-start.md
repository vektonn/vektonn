---
title: Quick Start
---

# Quick Start

## Run Vektonn

In order to run Vektonn locally you will need to install [Docker](https://docs.docker.com/get-docker/).

Run Vektonn configured for QuickStart examples:

```bash
git clone https://github.com/vektonn/vektonn-examples.git /path/to/vektonn-examples

/path/to/vektonn-examples/docker/run-vektonn.sh QuickStart.Index
```

Look at [docker-compose.yaml](https://github.com/vektonn/vektonn-examples/blob/master/docker/docker-compose.yaml) to understand how to setup local Vektonn with single Index shard.
Docker images for Vektonn are published on [Docker Hub](https://hub.docker.com/u/vektonn).


## Use Vektonn API with a Python client

You will need [Python >= 3.7](https://www.python.org/downloads/) to run this sample.

1. Install [Vektonn SDK](https://pypi.org/project/vektonn/) for Python:

```bash
pip install vektonn
```

2. Initialize Vektonn client:

```python
from vektonn import Vektonn

vektonn_client = Vektonn('http://localhost:8081')
```

3. Upload data to Vektonn:

```python
from vektonn.dtos import AttributeDto, AttributeValueDto, InputDataPointDto, VectorDto

vektonn_client.upload(
    data_source_name='QuickStart.Source',
    data_source_version='1.0',
    input_data_points=[
        InputDataPointDto(
            attributes=[
                AttributeDto(key='id', value=AttributeValueDto(int64=1)),
                AttributeDto(key='payload', value=AttributeValueDto(string='sample data point')),
            ],
            vector=VectorDto(is_sparse=False, coordinates=[3.14, 2.71]))
    ])
```

4. Search for `k` nearest data points to the given `query_vector`:

```python
from vektonn.dtos import VectorDto, SearchQueryDto

k = 10
query_vector = VectorDto(is_sparse=False, coordinates=[1.2, 3.4])

search_results = vektonn_client.search(
    index_name='QuickStart.Index',
    index_version='1.0',
    search_query=SearchQueryDto(k=k, query_vectors=[query_vector]))

print(f'For query vector {query_vector.coordinates} {k} nearest data points are:')
for fdp in search_results[0].nearest_data_points:
    attrs = {x.key : x.value for x in fdp.attributes}
    distance, vector, dp_id, payload = fdp.distance, fdp.vector, attrs['id'].int64, attrs['payload'].string
    print(f' - "{payload}" with id = {dp_id}, vector = {vector.coordinates}, distance = {distance}')
```


## What's next

Take a look at [Jupyter notebooks](https://github.com/vektonn/vektonn-examples/tree/master/jupyter-notebooks) with several examples of solving problems similar to real ones using vector spaces and Vektonn:

1. [Hotels](https://github.com/vektonn/vektonn-examples/blob/master/jupyter-notebooks/hotels/hotels.ipynb).
Task: save the user's time while searching for hotels.
This example uses vectorized user reviews to find hotels.

2. [Price Match Guarantee](https://github.com/vektonn/vektonn-examples/blob/master/jupyter-notebooks/cv/cv.ipynb).
Task: help the seller establish a competitive price for their product on the marketplace.

3. [Books](https://github.com/vektonn/vektonn-examples/blob/master/jupyter-notebooks/sparse-vectors/sparse-vectors.ipynb).
Task: find similar books by user reviews. This example demonstrates usage of sparse vectors.

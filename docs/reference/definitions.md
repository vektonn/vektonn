---
Title: Definitions
---


## Data points

**Data point** is a single piece of data to store in Vektonn. It contains both vector (dense or sparse) and its attributes. You can use **attributes** to store relevant data from your domain.

```python
DataPoint := (Vector, Attributes)

Vector       := DenseVector | SparseVector
DenseVector  := (coordinates: list[float])
SparseVector := (coordinates: list[float], coordinateIndices: list[int])

Attribute      := (key: AttributeKey, value: AttributeValue)
Attributes     := dict[AttributeKey, AttributeValue]
AttributeKey   := str
AttributeValue := bool | int64 | float64 | str | UUID | datetime
```


## Data sources

**Data source** is a named (and versioned) persistent storage for data points. It represents a timeline of all data points' updates (uploads and deletions).

```python
DataSourceMeta := (
    id: (name: str, version: str),

    vectorDimension: int,
    vectorsAreSparse: bool,

    permanentAttributes: set[AttributeKey],
    attributeValueTypes: dict[AttributeKey, AttributeValueType] # defines a type of AttributeValue for each AttributeKey
)
```

Defining a data source requires you to set the type of data points that it will store, including their size and permanent attributes.

**Permanent attributes** are attributes that all data points must have. Neither the set of keys nor the values of `permanentAttributes` can change for a given data point over time.

You cannot query a data source directly — you have to create an [index](#indices) for that.


## Indices

**Index** is a snapshot of a single data source that allows to query for data points. Indices are asynchronously updated, making them [eventually consistent](https://en.wikipedia.org/wiki/Eventual_consistency) with their data sources.

```python
IndexAlgorithm := (type: str, params: Optional[dict[str, str]])  # (1)

IndexMeta := (
    id:                (name: str, version: str),

    dataSource:        DataSourceMeta,
    indexAlgorithm:    IndexAlgorithm,

    idAttributes:      set[AttributeKey],
    splitAttributes:   set[AttributeKey],
    shardAttributes:   set[AttributeKey],
    payloadAttributes: set[AttributeKey]
)

Index.permanentAttributes := Index.idAttributes ⋃ Index.splitAttributes ⋃ Index.shardAttributes
Index.permanentAttributes ⊆ DataSourceMeta.permanentAttributes

Index.permanentAttributes ⋂ Index.payloadAttributes ≡ ∅
```

1. See the list of <a href="/vektonn/reference/supported-algorithms">supported algorithms</a>.

<a name="permanent-attributes"></a>
Index's **permanent attributes** are:

- `idAttributes` — a set of keys from your domain that uniquely identifies an object represented by the vector;
- `shardAttributes` — defines a [sharding](#sharding) scheme;
- `splitAttributes` — defines a [splitting](#splitting) scheme.

Note that `idAttributes`, `splitAttributes`, and `shardAttributes` sets can be in arbitrary relationships, but each one must be a subset of a corresponding data source's permanent attributes. They cannot be changed during vector's lifetime.

All **other attributes** are `payloadAttributes` — any metadata to store alongside the vector, may be freely updated.


## Sharding

**Shards** are a physical representation of an index. Each shard has a subset of index's data according to defined sharding scheme. It's possible to have a single shard for an index: that way everything is located on a single machine. Multiple shards, however, are useful to distribute data across several nodes to fit into RAM limitations.

```python
Index := (metadata: IndexMeta, shards: list[IndexShard])
IndexShard := Mapping[IdAttributes, DataPoint]
```


## Splitting

**Splits** are an optional scheme of dividing (splitting) shards data within a single physical node. Splits are useful to speed up search when your data and request are naturally divided inside your domain.

Defining a splitting scheme requires you to provide corresponding attributes on each search.


## Updating data

```python
DataPointUpdate := DataPoint | Tombstone
Tombstone := (permanentAttributes: Attributes)  # uniquely identify a data point to delete
```

Data updates are one of either:

- an **upload** of a new data point;
- a **deletion** of a previously uploaded data point, marked by a `tombstone`.


## Searching

Search for `k` nearest neighbors for each of query vectors:

```python
Search := Callable[[SearchQuery], list[SearchResult]]

SearchQuery    := (k: int, queryVectors: list[Vector], splitFilter: Optional[Attributes])

SearchResult   := (queryVector: Vector, nearestDataPoints: list[FoundDataPoint])
FoundDataPoint := (vector: Vector, attributes: Attributes, distance: float)
```

If `SearchQuery.splitFilter` is present then `SearchQuery.splitFilter.keys ⊆ IndexMeta.splitAttributes`.

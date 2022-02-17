---
Title: Definitions
---


## Data points

**Data point** is a single piece of data to store in Vektonn. It contains both vector (dense or sparse) and its attributes. You can use **attributes** to store relevant data from your domain.

```python
DataPoint := (Vector, Attributes)

Vector       := DenseVector | SparseVector
DenseVector  := (Coordinates: list[float])
SparseVector := (VectorDimension: int, Coordinates: list[float], Indices: list[int])

Attribute      := (Key: AttributeKey, Value: AttributeValue)
Attributes     := dict[AttributeKey, AttributeValue]
AttributeKey   := str
AttributeValue := bool | int | float | str | UUID | datetime
```

Both `int` and `float` are 64-bit.


## Data sources

**Data source** is a named (and versioned) persistent storage for data points. It represents a timeline of all data points' updates (uploads and deletions).

```python
DataSource := (metadata: DataSourceMeta, timeline: list[DataPointUpdate])
DataSourceId := (name: str, version: str)

DataSourceMeta := (
    id: DataSourceId,
    vectorDimension: int,
    vectorsAreSparse: bool,
    permanentAttributes: set[AttributeKey]
)

DataPointUpdate := DataPoint | Tombstone
Tombstone := (idAttributes: Attributes)  # uniquely identify a data point to delete

Index.idAttributes.keys() ⊆ DataSourceMeta.permanentAttributes
```

Defining a data source requires you to set the type of data points that it will store, including their size and permanent attributes.

**Permanent attributes** are attributes that all data points must have. Neither the set of keys nor the values of `permanentAttributes` can change for a given data point over time.

You cannot query a data source directly — you have to create an [index](#indices) for that.


## Indices

**Index** is a snapshot of a single data source that allows to query for data points. Indices are asynchronously updated, making them [eventually consistent](https://en.wikipedia.org/wiki/Eventual_consistency) with their data sources.

```python
Index := (metadata: IndexMeta, shards: list[IndexShard])
IndexShard := Mapping[IdAttributes, DataPoint]

IndexId := (name: str, version: str)

IndexAlgorithm := (type: str, params: Optional[dict[str, str]])  # (1)

IndexMeta := (
    id:                IndexId,
    dataSource:        DataSourceMeta,
    indexAlgorithm:    IndexAlgorithm,
    idAttributes:      set[AttributeKey],
    splitAttributes:   set[AttributeKey],
    shardAttributes:   set[AttributeKey],
    payloadAttributes: set[AttributeKey]
)

Index.permanentAttributes := Index.idAttributes ⋃ Index.splitAttributes ⋃ Index.shardAttributes
Index.permanentAttributes ⋂ Index.payloadAttributes ≡ ∅

Index.permanentAttributes ⊆ DataSourceMeta.permanentAttributes
```

1. See the list of <a href="/vektonn/reference/supported-algorithms">supported algorithms</a>.

<a name="permanent-attributes"></a>
**Permanent** attributes are:

- `IdAttributes` — a set of keys from your domain that uniquely identifies object represented by the vector;
- `ShardAttributes` — defines a [sharding](#sharding) scheme;
- `SplitAttributes` — defines a [splitting](#splitting) scheme.

Index's permanent attributes must be a subset of a corresponding data source's permanent attributes. They cannot be changed during vector's lifetime.

Also note that `IdAttributes`, `SplitAttributes`, and `ShardAttributes` sets can be in arbitrary relationships.

All **other attributes** are `PayloadAttributes` — any metadata to store alongside the vector, may be freely updated.


## Searching

```python
Search := Callable[[SearchQuery], list[SearchResult]]

SearchQuery    := (SplitKey: Optional[Attributes], QueryVectors: list[Vector], int K)

SearchResult   := (QueryVector: Vector, NearestKDataPoints: list[FoundDataPoint])
FoundDataPoint := (Vector: DataPoint, Distance: float)
```

Search query constraints:

- If `SearchQuery.SplitKey` is present then `SearchQuery.SplitKey.Keys ⊆ IndexMeta.SplitAttributes`

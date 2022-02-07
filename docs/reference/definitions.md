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


## Data sources

**Data source** is a named (and versioned) storage for data points. It represents a timeline of all data points' updates (uploads and deletions).

```python
DataSource := (name: str, version: str, timeline: list[DataPointUpdate])

DataPointUpdate := DataPoint | Tombstone
Tombstone := (idAttributes: Attributes)  # uniquely identify a data point to delete
```

You cannot query a data source directly — you have to create an index for that.


## Indices

**Index** is a snapshot of a single data source that allows to query for data points. Indices are asynchronously updated, making them [eventually consistent](https://en.wikipedia.org/wiki/Eventual_consistency) with their data sources.

```python
IndexAlgorithm := str  # (1)

Index := (Metadata: IndexMeta, Shards: list[IndexShard])
IndexShard := Mapping[IdAttributes, DataPoint]

IndexMeta := (
    VectorDimension:  int,
    IndexAlgorithm:   IndexAlgorithm,
    IdAttributes:     set[AttributeKey],
    SplitAttributes:  set[AttributeKey],
    ShardAttributes:  set[AttributeKey],
    PayloadAttribute: set[AttributeKey]
)

PermanentAttributes := IdAttributes ⋃ SplitAttributes ⋃ ShardAttributes
PermanentAttributes ⋂ DataAttributes ≡ ∅
```

1. See list of <a href="/vektonn/reference/supported-algorithms">supported algorithms</a>.


Index constraints:

- `PermanentAttributes ⋂ DataAttributes ≡ ∅`
- Neither the set of keys nor the values of `PermanentAttributes` can change for a given data point over time

Note that `IdAttributes`, `SplitAttributes`, and `ShardAttributes` sets can be in arbitrary relationships.

### Sharding

Selecting a shard for data point:

```python
def get_shard(dp: DataPoint) -> IndexShard:
    shard_key = [attr.value for attr in sorted(dp.attributes) if attr.Key in ShardAttributes]
    return shard_by_partition(shard_key) or shard_by_hash(shard_key)

def shard_by_hash(key) -> IndexShard:
    Index.Shards[abs(hash(key)) % len(Index.Shards)]

def shard_by_partition(key) -> IndexShard:
    next(filter(lambda shard: key in shard, partition_scheme))

# partitioning of a set of all values to arbitrary subsets
partition_scheme := W_1 | W_2 | ... | W_NumberOfPartitions
```


## Searching

```python
Search := Callable[[SearchQuery], list[SearchResult]]

SearchQuery    := (SplitKey: Optional[Attributes], QueryVectors: list[Vector], int K)

SearchResult   := (QueryVector: Vector, NearestKDataPoints: list[FoundDataPoint])
FoundDataPoint := (Vector: DataPoint, Distance: float)
```

Search query constraints:

- If `SearchQuery.SplitKey` is present then `SearchQuery.SplitKey.Keys ⊆ IndexMeta.SplitAttributes`

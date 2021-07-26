---
layout: default
title: How to use
nav_order: 2
---

# How to use SpaceHosting
{: .fs-9 }

---

# Table of contents
* [How to use SpaceHosting](#how) 
  * [Input file format](#format)
    * [Vectors](#vectors)
    * [Metadata](#metadata)
  * [SH_INDEX_ALGORITHM possible values](#ALGORITHM)
* [Examples](#examples)  

---

# How to use <a name="how"></a>
1. Prepare your files:
 * [vectors files](#vectors); 
 * [metadata files](#metadata) (optional). Metadata file is defined in an environment variable SH_VECTORS_METADATA_FILE_NAME in point 2. 
2. Run the SpaceHosting service and define following environment variables:
 * SH_VECTORS_FILE_NAME.
 * SH_VECTORS_FILE_FORMAT.
 * SH_VECTORS_METADATA_FILE_NAME (optional). If the variable is not set, the metadata is not loaded into the index. 
 * SH_INDEX_ALGORITHM – [index type](#ALGORITHM). 
3. SpaceHosting will read input from the specified files and build an index of the specified type. 
4. Submit your search request for kNN/AkNN.
 * You can use batch-mode – find the nearest vectors for several input vectors at once.
5. SpaceHosting will return a response to your request.

## Input file format <a name="format"></a>
### Vectors <a name="vectors"></a>
The file with vectors (SH_VECTORS_FILE_NAME) must contain a list of vectors. Depending on the value of the SH_VECTORS_FILE_FORMAT variable, this list is set differently:

* VectorArrayJson - a list of vectors serialized to JSON. Each vector is an array of numbers of the same length specifying the coordinates of the vector. [File sample](https://github.com/kontur-model-ops/space-hosting/blob/master/.data-samples/vectors.json).

* PandasDataFrameCsv - the list of vectors is specified in CSV format, in the same form as [pandas.DataFrame.to_csv](https://pandas.pydata.org/docs/reference/api/pandas.DataFrame.to_csv.html) serializes it. One vector is one line, coordinates are separated by commas. [File sample](https://github.com/kontur-model-ops/space-hosting/blob/master/.data-samples/vectors-df.csv).

* PandasDataFrameJson - the list of vectors is specified in JSON format, in the same form as pandas.DataFrame.to_json serializes it. [File sample](https://github.com/kontur-model-ops/space-hosting/blob/master/.data-samples/vectors-df.json). 

### Metadata <a name="metadata"></a>

A metadata file is a set of arbitrary key-value pairs. File format is JSON. See an example file [here](https://github.com/kontur-model-ops/space-hosting/blob/master/.data-samples/vectors-metadata.json). The correspondence between vectors and metadata from the input files is built by the index: the first vector from the file with vectors corresponds to the first record in the file with metadata. 

## SH_INDEX_ALGORITHM possible values <a name="ALGORITHM"></a>

* FaissIndex.Flat.L2 – dense vectors, euclidian metric.
* FaissIndex.Flat.IP – dense vectors, inner-product metric.
* SparnnIndex.Cosine – sparse vectors, cosine metric.
* SparnnIndex.JaccardBinary – sparse binary vectors, jaccard metric.

# Examples <a name="examples"></a>
1. [Search for hotels by user request](https://github.com/kontur-model-ops/space-hosting/blob/master/samples/spacehosting_hotels_example.ipynb).  
Task: to save the user's time while searching for hotels. 
This example uses vectorized user reviews to find hotels.

2. [Price Match Guarantee](https://github.com/kontur-model-ops/space-hosting/blob/master/samples/spacehosting_cv_example.ipynb).  
Task: to help the seller establish a competitive price for their product on the marketplace. 
This example uses data from [Shopee kaggle Competiton](https://www.kaggle.com/c/shopee-product-matching/overview/description).

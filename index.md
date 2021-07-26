---
layout: default
title: Home
nav_order: 1
permalink: /
---

# SpaceHosting
{: .fs-9 }

Service for finding k-nearest neighbors.
{: .fs-6 .fw-300 }

[View it on GitHub](https://github.com/kontur-model-ops/space-hosting){: .btn .fs-5 .mb-4 .mb-md-0 }

---

# Get started

SpaceHosting is a service for finding k-nearest neighbors (kNN) using a .NET library [SpaceHosting.Index](https://github.com/kontur-model-ops/space-hosting-index#spacehostingindex). 

Dense and sparse vectors are supported. For dense vectors we use [Faiss](https://github.com/facebookresearch/faiss) native library. For sparse vectors we have ported [PySparNN](https://github.com/facebookresearch/pysparnn) library to C#. 

## Why SpaceHosting? 

SpaceHosting advantages: 
* Supports dense and sparse vectors.
* Uses search algorithms:
  * Faiss.
  * Ported PySparNN.
* Supports Approximate kNN (AkNN).

## SpaceHosting.Index library

SpaceHosting.Index keeps a mapping between vectors and a set of keys. При поиске kNN/AkNN eturns keys instead of vector numbers. A key is a set of parameters that identifies a vector. 

The library also allows to keep any metadata alongside corresponding vectors. kNN/AkNN search results contain metadata as well as vectors. 

[Read more about SpaceHosting.Index](https://github.com/kontur-model-ops/space-hosting-index#spacehostingindex).

## Local installation 
```
git clone https://github.com/kontur-model-ops/space-hosting.git 
cd space-hosting 
./docker-compose-up.sh
```
After that Swagger-specification for SpaceHosting API will be available at <http://localhost:8080>.

## Next Step 

* [See how to use SpaceHosting](/Testdoc/how-to-use).

---

## Support

If you have questions or need help with SpaceHosting service please contact us on Slack channel … .

## License

SpaceHosting is distributed by an [Apache License 2.0](https://github.com/kontur-model-ops/space-hosting/blob/master/LICENSE).

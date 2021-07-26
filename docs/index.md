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

Dense and sparse vectors are supported. [Faiss](https://github.com/facebookresearch/faiss) library is used for dense vectors. [PySparNN](https://github.com/facebookresearch/pysparnn) is being ported to C# and used for sparse vectors. 

## Reasons to use SpaceHosting  

SpaceHosting advantages are: 
* Support dense and sparse vectors.
* Faiss support.
* Ported PySparNN support.
* Approximate kNN (AkNN) support.

## SpaceHosting.Index library

SpaceHosting.Index stores a mapping between vectors and a set of keys. При поиске kNN/AkNN eturns keys instead of vector numbers. A key is a set of parameters that identifies a vector. 

The library also allows you to keep any metadata alongside corresponding vectors. kNN/AkNN search results contain metadata as well as vectors. 

[Read more about SpaceHosting.Index](https://github.com/kontur-model-ops/space-hosting-index#spacehostingindex).

## Local installation 
```
git clone https://github.com/kontur-model-ops/space-hosting.git 
cd space-hosting 
./docker-compose-up.sh
```
After, a Swagger-specification for [SpaceHosting API](https://kontur-model-ops.github.io/space-hosting/swagger/index.html) will be available at <http://localhost:8080>.

## Next Step 

* [See how to use SpaceHosting](/Testdoc/how-to-use).

---

## Support

If you have any questions or need help with SpaceHosting service please contact us on Slack channel … .

## License

SpaceHosting is distributed by an [Apache License 2.0](https://github.com/kontur-model-ops/space-hosting/blob/master/LICENSE).

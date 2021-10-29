</p>
<p align="center">
    <h1 align="center">Vektonn</h1>
    <br>
</p>

## Get started

Vektonn is a service for finding k-nearest neighbors (kNN) using a .NET library [Vektonn.Index](https://github.com/vektonn/vektonn-index). 

Dense and sparse vectors are supported. [Faiss](https://github.com/facebookresearch/faiss) library is used for dense vectors. [PySparNN](https://github.com/facebookresearch/pysparnn) is being ported to C# and used for sparse vectors. 

### Reasons to use Vektonn 

Vektonn advantages are: 
* Support dense and sparse vectors.
* Faiss support.
* PySparNN support.
* Approximate kNN (AkNN) support.

### Vektonn.Index library

Vektonn.Index keeps a mapping between vectors and vector ID’s. kNN/AkNN returns vector ID’s as result.

The library also allows you to keep any metadata alongside corresponding vectors. kNN/AkNN search results contain metadata as well as vectors. 

[Read more about Vektonn.Index](https://github.com/vektonn/vektonn-index).

### Local installation 
```
git clone https://github.com/vektonn/vektonn.git 
cd vektonn 
./docker-compose-up.sh
```
After, a Swagger-specification for [Vektonn API](https://vektonn.github.io/vektonn/swagger/index.html) will be available at <http://localhost:8081>.

### Next Step 

* [See how to use Vektonn](https://vektonn.github.io/vektonn/how-to-use/how-to-use.html).


## Support

If you have any questions or need help with Vektonn please contact us on [Slack channel](http://vektonn.slack.com/).


## License

Vektonn is distributed by an [Apache License 2.0](https://github.com/vektonn/vektonn/blob/master/LICENSE).

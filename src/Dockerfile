# https://benfoster.io/blog/optimising-dotnet-docker-images/

# https://hub.docker.com/_/microsoft-dotnet
ARG DOTNET_REPO=mcr.microsoft.com/dotnet
ARG DOTNET_VERSION_TAG=5.0-buster-slim-amd64
ARG FAISS_VERSION=1.7.2.1

FROM ghcr.io/vektonn/vektonn/faiss-lib:$FAISS_VERSION AS faiss-lib

# build stage
FROM $DOTNET_REPO/sdk:$DOTNET_VERSION_TAG AS build

ARG DOTNET_RID=linux-x64
ARG PUBLISH_TRIMMED=false
ARG SHARED_LIB_API_CONTRACTS=Vektonn.ApiContracts
ARG SHARED_LIB_API_CLIENT=Vektonn.ApiClient
ARG SHARED_LIB_IMPL=Vektonn.SharedImpl
ARG SHARED_LIB_DATA_SOURCE=Vektonn.DataSource
ARG SHARED_LIB_INDEX_SHARD=Vektonn.IndexShard
ARG SHARED_LIB_HOSTING=Vektonn.Hosting
ARG SERVICE_API=Vektonn.ApiService
ARG SERVICE_INDEX_SHARD=Vektonn.IndexShardService

WORKDIR /src

# copy csproj and restore as distinct layer
COPY global.json .
COPY nuget.config .
COPY Directory.Build.props .
COPY $SHARED_LIB_API_CONTRACTS/$SHARED_LIB_API_CONTRACTS.csproj $SHARED_LIB_API_CONTRACTS/
COPY $SHARED_LIB_API_CLIENT/$SHARED_LIB_API_CLIENT.csproj $SHARED_LIB_API_CLIENT/
COPY $SHARED_LIB_IMPL/$SHARED_LIB_IMPL.csproj $SHARED_LIB_IMPL/
COPY $SHARED_LIB_DATA_SOURCE/$SHARED_LIB_DATA_SOURCE.csproj $SHARED_LIB_DATA_SOURCE/
COPY $SHARED_LIB_INDEX_SHARD/$SHARED_LIB_INDEX_SHARD.csproj $SHARED_LIB_INDEX_SHARD/
COPY $SHARED_LIB_HOSTING/$SHARED_LIB_HOSTING.csproj $SHARED_LIB_HOSTING/
COPY $SERVICE_API/$SERVICE_API.csproj $SERVICE_API/
COPY $SERVICE_INDEX_SHARD/$SERVICE_INDEX_SHARD.csproj $SERVICE_INDEX_SHARD/

RUN dotnet restore --runtime $DOTNET_RID $SERVICE_API/$SERVICE_API.csproj
RUN dotnet restore --runtime $DOTNET_RID $SERVICE_INDEX_SHARD/$SERVICE_INDEX_SHARD.csproj

# copy shared sources
COPY $SHARED_LIB_API_CONTRACTS/ $SHARED_LIB_API_CONTRACTS/
COPY $SHARED_LIB_API_CLIENT/ $SHARED_LIB_API_CLIENT/
COPY $SHARED_LIB_IMPL/ $SHARED_LIB_IMPL/
COPY $SHARED_LIB_DATA_SOURCE/ $SHARED_LIB_DATA_SOURCE/
COPY $SHARED_LIB_INDEX_SHARD/ $SHARED_LIB_INDEX_SHARD/
COPY $SHARED_LIB_HOSTING/ $SHARED_LIB_HOSTING/

# publish ApiService
COPY $SERVICE_API/ $SERVICE_API/
RUN dotnet publish \
    --no-restore \
    --configuration Release \
    --self-contained true \
    --runtime $DOTNET_RID \
    -p:PublishTrimmed=$PUBLISH_TRIMMED \
    --output /app/$SERVICE_API \
    $SERVICE_API/$SERVICE_API.csproj

# publish IndexShardService
COPY $SERVICE_INDEX_SHARD/ $SERVICE_INDEX_SHARD/
RUN dotnet publish \
    --no-restore \
    --configuration Release \
    --self-contained true \
    --runtime $DOTNET_RID \
    -p:PublishTrimmed=$PUBLISH_TRIMMED \
    --output /app/$SERVICE_INDEX_SHARD \
    $SERVICE_INDEX_SHARD/$SERVICE_INDEX_SHARD.csproj

# final stage/image for ApiService (target entrypoint with: docker build --target api-service)
FROM $DOTNET_REPO/runtime-deps:$DOTNET_VERSION_TAG AS api-service
EXPOSE 8081
WORKDIR /vektonn
COPY --from=build /app/Vektonn.ApiService ./bin
RUN mkdir ./logs
ENV ASPNETCORE_ENVIRONMENT=Development \
    VEKTONN_HOSTING_ENVIRONMENT=dev \
    VEKTONN_HTTP_PORT=8081 \
    VEKTONN_KAFKA_BOOTSTRAP_SERVERS=localhost \
    VEKTONN_KAFKA_TOPIC_REPLICATION_FACTOR=1 \
    VEKTONN_CONFIG_BASE_DIRECTORY=config
ENTRYPOINT ["./bin/Vektonn.ApiService"]

# final stage/image for IndexShardService (target entrypoint with: docker build --target index-shard-service)
FROM $DOTNET_REPO/runtime-deps:$DOTNET_VERSION_TAG AS index-shard-service
EXPOSE 8082
WORKDIR /vektonn
COPY --from=build /app/Vektonn.IndexShardService ./bin
COPY --from=faiss-lib /lib-faiss-native/ ./lib-faiss-native/
RUN mkdir ./logs
ENV LD_LIBRARY_PATH=/vektonn/lib-faiss-native \
    ASPNETCORE_ENVIRONMENT=Development \
    VEKTONN_HOSTING_ENVIRONMENT=dev \
    VEKTONN_HTTP_PORT=8082 \
    VEKTONN_KAFKA_BOOTSTRAP_SERVERS=localhost \
    VEKTONN_CONFIG_BASE_DIRECTORY=config \
    VEKTONN_INDEX_NAME= \
    VEKTONN_INDEX_VERSION= \
    VEKTONN_INDEX_SHARD_ID=
ENTRYPOINT ["./bin/Vektonn.IndexShardService"]

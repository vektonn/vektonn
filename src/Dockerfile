#######################################################################################################################
# Common ARGs
# https://hub.docker.com/_/microsoft-dotnet
ARG DOTNET_REPO=mcr.microsoft.com/dotnet
ARG DOTNET_VERSION_TAG=6.0-bullseye-slim-amd64
#######################################################################################################################


#######################################################################################################################
# FAISS binaries
ARG FAISS_VERSION=1.7.2.2

FROM ghcr.io/vektonn/vektonn/faiss-lib:$FAISS_VERSION AS faiss-lib
#######################################################################################################################


#######################################################################################################################
# Build stage
FROM $DOTNET_REPO/sdk:$DOTNET_VERSION_TAG AS build

ARG DOTNET_TOOLS_PATH=/dotnet/tools
RUN dotnet tool install dotnet-dump --tool-path $DOTNET_TOOLS_PATH && \
    dotnet tool install dotnet-trace --tool-path $DOTNET_TOOLS_PATH && \
    dotnet tool install dotnet-gcdump --tool-path $DOTNET_TOOLS_PATH && \
    dotnet tool install dotnet-counters --tool-path $DOTNET_TOOLS_PATH && \
    dotnet tool install JetBrains.dotTrace.GlobalTools --tool-path $DOTNET_TOOLS_PATH

ARG DOTNET_RID=linux-x64
ARG SHARED_LIB_API_CONTRACTS=Vektonn.ApiContracts
ARG SHARED_LIB_API_CLIENT=Vektonn.ApiClient
ARG SHARED_LIB_IMPL=Vektonn.SharedImpl
ARG SHARED_LIB_DATA_SOURCE=Vektonn.DataSource
ARG SHARED_LIB_INDEX_SHARD=Vektonn.IndexShard
ARG SHARED_LIB_HOSTING=Vektonn.Hosting
ARG SERVICE_API=Vektonn.ApiService
ARG SERVICE_INDEX_SHARD=Vektonn.IndexShardService

WORKDIR /src

# Copy csproj and restore as distinct layer
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

# Copy shared sources
COPY $SHARED_LIB_API_CONTRACTS/ $SHARED_LIB_API_CONTRACTS/
COPY $SHARED_LIB_API_CLIENT/ $SHARED_LIB_API_CLIENT/
COPY $SHARED_LIB_IMPL/ $SHARED_LIB_IMPL/
COPY $SHARED_LIB_DATA_SOURCE/ $SHARED_LIB_DATA_SOURCE/
COPY $SHARED_LIB_INDEX_SHARD/ $SHARED_LIB_INDEX_SHARD/
COPY $SHARED_LIB_HOSTING/ $SHARED_LIB_HOSTING/

# Publish ApiService
COPY $SERVICE_API/ $SERVICE_API/
RUN dotnet publish \
    --no-restore \
    --configuration Release \
    --self-contained false \
    --runtime $DOTNET_RID \
    --output /app/$SERVICE_API \
    $SERVICE_API/$SERVICE_API.csproj

# Publish IndexShardService
COPY $SERVICE_INDEX_SHARD/ $SERVICE_INDEX_SHARD/
RUN dotnet publish \
    --no-restore \
    --configuration Release \
    --self-contained false \
    --runtime $DOTNET_RID \
    -p:PublishTrimmed=$PUBLISH_TRIMMED \
    --output /app/$SERVICE_INDEX_SHARD \
    $SERVICE_INDEX_SHARD/$SERVICE_INDEX_SHARD.csproj
#######################################################################################################################


#######################################################################################################################
# Final stage/image base
FROM $DOTNET_REPO/aspnet:$DOTNET_VERSION_TAG AS runtime

# Install useful diagnostic tools
ARG DOTNET_TOOLS_PATH=/dotnet/tools
COPY --from=build $DOTNET_TOOLS_PATH $DOTNET_TOOLS_PATH
RUN apt-get update && apt-get install -y procps

RUN mkdir -p /vektonn/logs && \
    mkdir -p /vektonn/dumps

ENV PATH=$PATH:$DOTNET_TOOLS_PATH \
    DOTNET_DbgEnableMiniDump=1 \
    DOTNET_DbgMiniDumpType=2 \
    DOTNET_DbgMiniDumpName=/vektonn/dumps/crash.dmp \
    ASPNETCORE_ENVIRONMENT=Development \
    VEKTONN_KAFKA_BOOTSTRAP_SERVERS= \
    VEKTONN_CONFIG_BASE_DIRECTORY=config

WORKDIR /vektonn
#######################################################################################################################


#######################################################################################################################
# Final stage/image for ApiService (target this entrypoint with: docker build --target api-service)
FROM runtime AS api-service

ARG VEKTONN_API_HTTP_PORT=8081

EXPOSE $VEKTONN_API_HTTP_PORT

COPY --from=build /app/Vektonn.ApiService ./bin

ENV VEKTONN_HTTP_PORT=$VEKTONN_API_HTTP_PORT \
    VEKTONN_KAFKA_TOPIC_REPLICATION_FACTOR=

ENTRYPOINT ["./bin/Vektonn.ApiService"]
#######################################################################################################################


#######################################################################################################################
# Final stage/image for IndexShardService (target this entrypoint with: docker build --target index-shard-service)
FROM runtime AS index-shard-service

ARG VEKTONN_INDEX_SHARD_HTTP_PORT=8082

EXPOSE $VEKTONN_INDEX_SHARD_HTTP_PORT

COPY --from=build /app/Vektonn.IndexShardService ./bin
COPY --from=faiss-lib /lib-faiss-native/ ./lib-faiss-native/

ENV LD_LIBRARY_PATH=/vektonn/lib-faiss-native \
    VEKTONN_HTTP_PORT=$VEKTONN_INDEX_SHARD_HTTP_PORT \
    VEKTONN_INDEX_NAME= \
    VEKTONN_INDEX_VERSION= \
    VEKTONN_INDEX_SHARD_ID=

ENTRYPOINT ["./bin/Vektonn.IndexShardService"]
#######################################################################################################################

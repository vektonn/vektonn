# https://benfoster.io/blog/optimising-dotnet-docker-images/

# https://hub.docker.com/_/microsoft-dotnet
ARG DOTNET_REPO=mcr.microsoft.com/dotnet
ARG DOTNET_VERSION_TAG=5.0-buster-slim-amd64
ARG FAISS_VERSION=1.7.1

FROM ghcr.io/kontur-model-ops/space-hosting/faiss-lib:$FAISS_VERSION AS faiss-lib

# build stage
FROM $DOTNET_REPO/sdk:$DOTNET_VERSION_TAG AS build

ARG DOTNET_RID=linux-x64
ARG PUBLISH_TRIMMED=false
ARG SHARED_LIB=SpaceHosting
ARG PROJECT=SpaceHosting.Service

WORKDIR /src

# copy csproj and restore as distinct layer
COPY global.json .
COPY nuget.config .
COPY Directory.Build.props .
COPY $SHARED_LIB/$SHARED_LIB.csproj $SHARED_LIB/
COPY $PROJECT/$PROJECT.csproj $PROJECT/
RUN dotnet restore --runtime $DOTNET_RID $PROJECT/$PROJECT.csproj

# copy app sources and publish
COPY $SHARED_LIB/ $SHARED_LIB/
COPY $PROJECT/ $PROJECT/
RUN dotnet publish \
    --no-restore \
    --configuration Release \
    --self-contained true \
    --runtime $DOTNET_RID \
    -p:PublishTrimmed=$PUBLISH_TRIMMED \
    --output /app \
    $PROJECT/$PROJECT.csproj


# final stage
FROM $DOTNET_REPO/runtime-deps:$DOTNET_VERSION_TAG
EXPOSE 8080
WORKDIR /app
COPY --from=build /app .
COPY --from=faiss-lib /lib-faiss-native/ /lib-faiss-native/
ENV ASPNETCORE_URLS=http://+:8080 \
    LD_LIBRARY_PATH=/lib-faiss-native
ENTRYPOINT ["./SpaceHosting.Service"]

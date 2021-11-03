#!/bin/bash
set -e
THIS_SCRIPT_DIR=$(cd $(dirname "${BASH_SOURCE[0]}") && pwd)

if [ $# -lt 2 ]; then
    echo "Usage: .build-docker-image.sh docker_target docker_image_name_and_tag";
    exit 1;
fi

DOCKER_TARGET=$1
DOCKER_IMAGE_NAME_AND_TAG=$2

echo "Building $DOCKER_TARGET docker image: $DOCKER_IMAGE_NAME_AND_TAG"

docker image build \
    --pull \
    --target "$DOCKER_TARGET" \
    --tag "$DOCKER_IMAGE_NAME_AND_TAG" \
    --file "$THIS_SCRIPT_DIR/Dockerfile" \
    "$THIS_SCRIPT_DIR"

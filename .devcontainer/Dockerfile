# this image is built and updated weekly
# https://github.com/cse-labs/codespaces-images

FROM ghcr.io/cse-labs/dind:latest

# some images require specific values
ARG USERNAME=vscode

ENV  AUTH_TYPE CLI

RUN  apt-get install -y --no-recommends dotnet-sdk-3.1

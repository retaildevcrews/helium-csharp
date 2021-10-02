#!/bin/bash

echo "post-start start" >> ~/status

# this runs in background each time the container starts

# update the base docker images
docker pull mcr.microsoft.com/dotnet/core/sdk:3.1
docker pull mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine

echo "post-start complete" >> ~/status

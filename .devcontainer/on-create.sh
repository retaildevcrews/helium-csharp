#!/bin/sh

echo "on-create start" >> ~/status

# run dotnet restore
dotnet restore src/helium.sln

echo "on-create complete" >> ~/status

#!/bin/sh

echo "on-create start" >> ~/status

# run dotnet restore
dotnet restore src/helium.sln
dotnet restore src/tests.sln

echo "on-create complete" >> ~/status

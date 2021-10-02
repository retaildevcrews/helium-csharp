#!/bin/sh

echo "on-create start" >> ~/status

# copy vscode files
mkdir -p .vscode
cp .devcontainer/vscode-template/* .vscode

# run dotnet restore
dotnet restore src/helium.sln
dotnet restore src/tests.sln

echo "on-create complete" >> ~/status

#!/bin/sh

# copy vscode files
mkdir -p .vscode && cp docs/vscode-template/* .vscode

# source the bashrc-append from the repo
# you can add project specific settings to .bashrc-append and
# they will be added for every user that clones the repo with Codespaces
# including keys or secrets could be a SECURITY RISK
echo "" >> ~/.bashrc
echo ". ${PWD}/.devcontainer/.bashrc-append" >> ~/.bashrc

DEBIAN_FRONTEND=noninteractive
sudo apt-get update
sudo apt-get install -y --no-install-recommends apt-utils dialog dnsutils
DEBIAN_FRONTEND=dialog

# install WebV global tool
export PATH="$PATH:~/.dotnet/tools"
export DOTNET_ROOT=~/.dotnet
dotnet tool install -g webvalidate --version 1.0.7.3

# set auth type
export AUTH_TYPE=CLI

# run dotnet restore
dotnet restore src/tests.sln

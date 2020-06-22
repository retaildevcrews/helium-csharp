#!/bin/sh

# copy vscode files
mkdir -p .vscode
cp docs/vscode-template/* .vscode

# run dotnet restore
dotnet restore src/tests.sln

# source the bashrc-append from the repo
# you can add project specific settings to .bashrc-append and
# they will be added for every user that clones the repo with Codespaces
# including keys or secrets is a SECURITY RISK
echo "" >> ~/.bashrc
echo ". ${PWD}/.devcontainer/.bashrc-append" >> ~/.bashrc

DEBIAN_FRONTEND=noninteractive
sudo apt-get update
sudo apt-get install -y --no-install-recommends apt-utils dialog
sudo apt-get install -y --no-install-recommends dnsutils httpie
DEBIAN_FRONTEND=dialog

# install WebV global tool
export PATH="$PATH:~/.dotnet/tools"
export DOTNET_ROOT=~/.dotnet
dotnet tool install -g webvalidate

# set auth type
export AUTH_TYPE=CLI

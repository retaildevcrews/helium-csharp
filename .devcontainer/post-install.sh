#!/bin/sh

# run dotnet restore
dotnet restore src/helium.sln

# docker bash-completion
sudo curl -L https://raw.githubusercontent.com/docker/machine/v0.16.0/contrib/completion/bash/docker-machine.bash -o /etc/bash_completion.d/docker-machine

DEBIAN_FRONTEND=noninteractive
# update apt-get
sudo apt-get update
sudo apt-get install -y --no-install-recommends apt-utils dialog

# update / install utils
sudo apt-get install -y --no-install-recommends dnsutils httpie bash-completion curl wget git
DEBIAN_FRONTEND=dialog

# copy vscode files
mkdir -p .vscode && cp docs/vscode-template/* .vscode

# source the bashrc-append from the repo
# you can add project specific settings to .bashrc-append and
# they will be added for every user that clones the repo with Codespaces
# including keys or secrets is a SECURITY RISK
echo "" >> ~/.bashrc
echo ". ${PWD}/.devcontainer/.bashrc-append" >> ~/.bashrc

# install WebV global tool
export PATH="$PATH:~/.dotnet/tools"
export DOTNET_ROOT=~/.dotnet
dotnet tool install -g webvalidate

# set auth type
export AUTH_TYPE=CLI

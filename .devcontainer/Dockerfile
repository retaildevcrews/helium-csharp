FROM mcr.microsoft.com/vscode/devcontainers/dotnetcore:3.1

ARG USERNAME="vscode"

RUN export DEBIAN_FRONTEND=noninteractive && \
    apt-get update && \
    apt-get -y install --no-install-recommends apt-utils dialog && \
    apt-get -y install --no-install-recommends apt-transport-https ca-certificates software-properties-common libssl-dev libffi-dev \
        build-essential gnupg-agent dnsutils httpie bash-completion curl wget git unzip jq lsb-release procps gnupg2

COPY library-scripts /tmp/library-scripts/

RUN  bash /tmp/library-scripts/azcli-debian.sh

RUN  bash /tmp/library-scripts/docker-debian.sh $USERNAME

# RUN  apt-get -y upgrade

ENV  PATH "$PATH:~/.dotnet/tools"
ENV  AUTH_TYPE CLI

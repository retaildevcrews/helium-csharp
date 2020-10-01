# To use the docker cli inside a container, use the bind mount option to add the docker socket to the container.
# Example: "docker run -v '/var/run/docker.sock:/var/run/docker.sock' ..."

# [Choice] .NET Core version: 3.1, 2.1
ARG VARIANT=3.1
FROM mcr.microsoft.com/vscode/devcontainers/dotnetcore:${VARIANT}

ARG USERNAME="vscode"

RUN export DEBIAN_FRONTEND=noninteractive && \
    apt-get update && \
    apt-get -y install --no-install-recommends apt-utils dialog && \
    apt-get -y install --no-install-recommends apt-transport-https ca-certificates software-properties-common libssl-dev libffi-dev \
        build-essential gnupg-agent dnsutils httpie bash-completion curl wget git unzip jq lsb-release procps gnupg2

# [Option] Install Node.js
ARG  INSTALL_NODE="false"
ARG  NODE_VERSION="none"
ENV  NVM_DIR=/usr/local/share/nvm
ENV  NVM_SYMLINK_CURRENT=true \
     PATH=${NVM_DIR}/current/bin:${PATH}
COPY .devcontainer/library-scripts/node-debian.sh /tmp/library-scripts/
RUN  if [ "$INSTALL_NODE" = "true" ]; then bash /tmp/library-scripts/node-debian.sh "${NVM_DIR}" "${NODE_VERSION}" "${USERNAME}"; fi


# [Option] Install Azure CLI
ARG  INSTALL_AZURE_CLI="true"
COPY .devcontainer/library-scripts/azcli-debian.sh /tmp/library-scripts/
RUN  if [ "$INSTALL_AZURE_CLI" = "true" ]; then bash /tmp/library-scripts/azcli-debian.sh; fi

# [Option] Install Docker CLI
ARG  INSTALL_DOCKER="true"
COPY .devcontainer/library-scripts/docker-debian.sh /tmp/library-scripts/
RUN  if [ "${INSTALL_DOCKER}" = "true" ]; then \
        bash /tmp/library-scripts/docker-debian.sh $USERNAME; \
     fi

RUN  apt-get -y upgrade

USER $USERNAME

# [Choice] .NET Core version: 3.1, 2.1
ARG VARIANT=3.1
FROM mcr.microsoft.com/vscode/devcontainers/dotnetcore:${VARIANT}

# [Optional] Uncomment this section to install additional OS packages.
RUN export DEBIAN_FRONTEND=noninteractive && \
    apt-get update && \
    apt-get -y install --no-install-recommends apt-transport-https ca-certificates software-properties-common libssl-dev libffi-dev build-essential gnupg-agent && \
    apt-get -y install --no-install-recommends dnsutils httpie bash-completion curl wget git unzip jq lsb-release

# [Option] Install Node.js
ARG  INSTALL_NODE="false"
ARG  NODE_VERSION="none"
ARG  USERNAME="vscode"
ENV  NVM_DIR=/usr/local/share/nvm
ENV  NVM_SYMLINK_CURRENT=true \
     PATH=${NVM_DIR}/current/bin:${PATH}
COPY library-scripts/node-debian.sh /tmp/library-scripts/
RUN  if [ "$INSTALL_NODE" = "true" ]; then bash /tmp/library-scripts/node-debian.sh "${NVM_DIR}" "${NODE_VERSION}" "${USERNAME}"; fi

# [Option] Install Azure CLI
ARG  INSTALL_AZURE_CLI="true"
COPY library-scripts/azcli-debian.sh /tmp/library-scripts/
RUN  if [ "$INSTALL_AZURE_CLI" = "true" ]; then bash /tmp/library-scripts/azcli-debian.sh; fi

# [Option] Install Docker CLI
ARG  INSTALL_DOCKER="true"
COPY library-scripts/docker-debian.sh /tmp/library-scripts/
RUN  if [ "${INSTALL_DOCKER}" = "true" ]; then \
        bash /tmp/library-scripts/docker-debian.sh "true" "/var/run/docker-host.sock" "/var/run/docker.sock" $USERNAME; \
     else \
        echo '#!/bin/bash\nexec "$@"' > /usr/local/share/docker-init.sh && chmod +x /usr/local/share/docker-init.sh; \
     fi

RUN  groupadd -g 800 docker && \
     usermod -a -G docker $USERNAME && \
     chown $USERNAME /var/run/docker* && \
     apt-get -y upgrade

# [Optional] Uncomment this line to install global node packages.
# RUN su vscode -c "source /usr/local/share/nvm/nvm.sh && npm install -g <your-package-here>" 2>&1

#ENTRYPOINT [ "/usr/local/share/docker-init.sh" ]
#CMD [ "sleep", "infinity" ]

#USER $USERNAME

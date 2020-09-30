# [Choice] .NET Core version: 3.1, 2.1
ARG VARIANT=3.1
FROM mcr.microsoft.com/vscode/devcontainers/dotnetcore:${VARIANT}

# [Optional] Uncomment this section to install additional OS packages.
RUN export DEBIAN_FRONTEND=noninteractive && \
    apt-get -y install --no-install-recommends dnsutils httpie bash-completion curl wget git unzip jq lsb-release && \
    apt-get -y install --no-install-recommends apt-transport-https ca-certificates software-properties-common libssl-dev libffi-dev build-essential gnupg-agent && \
    apt-get -y upgrade

# Install docker
COPY library-scripts/docker-debian.sh /tmp/library-scripts/
RUN bash /tmp/library-scripts/docker-debian.sh
RUN groupadd -g 800 docker && \
    usermod -a -G docker vscode && \
    chown vscode:vscode /var/run/docker* && \
    rm -rf /tmp/library-scripts

# [Optional] Uncomment this line to install global node packages.
# RUN su vscode -c "source /usr/local/share/nvm/nvm.sh && npm install -g <your-package-here>" 2>&1

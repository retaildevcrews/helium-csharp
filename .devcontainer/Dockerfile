FROM mcr.microsoft.com/dotnet/core/sdk:3.1

ENV DEBIAN_FRONTEND=noninteractive

RUN curl https://packages.microsoft.com/config/debian/10/packages-microsoft-prod.deb --output /tmp/packages-microsoft-prod.deb && \
    dpkg -i /tmp/packages-microsoft-prod.deb && \
    #
    apt-get update && \
    apt-get upgrade -y && \
    apt-get dist-upgrade -y && \
    apt-get -y install --no-install-recommends \
    #Install Common Tools
            apt-transport-https \
            apt-utils \
            bash-completion \
            curl \
            dialog \
            dnsutils \
            git \
            httpie \
            lsb-release \
            procps \
            unzip \
            sudo \
            openssh-client \
            less \
            iproute2 \
            gnupg2 \
            jq \
            vim && \
    #
    # Install Azure CLI
    curl -sL https://aka.ms/InstallAzureCLIDeb | bash && \
    #
    # Install Docker CLI
    curl -sL https://download.docker.com/linux/$(lsb_release -is | tr '[:upper:]' '[:lower:]')/gpg | (OUT=$(apt-key add - 2>&1) || echo $OUT) && \
    echo "deb [arch=amd64] https://download.docker.com/linux/$(lsb_release -is | tr '[:upper:]' '[:lower:]') $(lsb_release -cs) stable" | tee /etc/apt/sources.list.d/docker.list && \
    apt-get update && \
    apt-get -y install --no-install-recommends docker-ce-cli && \
    #
    # Install WebValidate
    dotnet tool install -g webvalidate

# This Dockerfile adds a non-root user with sudo access. Use the "remoteUser"
# property in devcontainer.json to use it. On Linux, the container user's GID/UIDs
# will be updated to match your local UID/GID (when using the dockerFile property).
# See https://aka.ms/vscode-remote/containers/non-root-user for details.
ARG USERNAME=helium
ARG USER_UID=1000
ARG USER_GID=$USER_UID

# Create the user
RUN groupadd --gid $USER_GID $USERNAME \
    && useradd -s /bin/bash --uid $USER_UID --gid $USER_GID -m $USERNAME \
    && echo $USERNAME ALL=\(root\) NOPASSWD:ALL > /etc/sudoers.d/$USERNAME\
    && chmod 0440 /etc/sudoers.d/$USERNAME \
    #
    # add docker group
    && groupadd -g 800 docker \
    && usermod -a -G docker $USERNAME

RUN chown -R $USERNAME:$USERNAME /home/$USERNAME

USER helium

# Switch back to dialog for any ad-hoc use of apt-get
ENV DEBIAN_FRONTEND=dialog

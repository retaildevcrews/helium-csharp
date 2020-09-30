FROM mcr.microsoft.com/dotnet/core/sdk:3.1

ENV DEBIAN_FRONTEND=noninteractive

RUN curl https://packages.microsoft.com/config/debian/10/packages-microsoft-prod.deb --output /tmp/packages-microsoft-prod.deb && \
    dpkg -i /tmp/packages-microsoft-prod.deb && \
    #
    apt-get update && \
    apt-get -y install --no-install-recommends apt-utils dialog && \
    #Install Common Tools
    apt-get -y install \
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
            procps \
            apt-transport-https \
            gnupg2 \
            curl \
            lsb-release \
            jq \
            vim && \
    #Install Azure CLI
    curl -sL https://aka.ms/InstallAzureCLIDeb | bash && \
    #
    #Install WebValidate
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

# Switch back to dialog for any ad-hoc use of apt-get
ENV DEBIAN_FRONTEND=dialog


CMD [“echo”,”Image created”] 

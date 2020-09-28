FROM mcr.microsoft.com/dotnet/core/sdk:3.1

ENV DEBIAN_FRONTEND=noninteractive

RUN curl https://packages.microsoft.com/config/debian/10/packages-microsoft-prod.deb --output /tmp/packages-microsoft-prod.deb && \
    dpkg -i /tmp/packages-microsoft-prod.deb && \
    #
    #Install Common Tools
    apt-get update && \
    apt-get -y install --no-install-recommends \
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
            unzip && \
    #Install Azure CLI
    curl -sL https://aka.ms/InstallAzureCLIDeb | bash && \
    #
# Install vim
    #
    #Install WebValidate
    dotnet tool install -g webvalidate
    #Install vim
RUN apt-get update && apt-get install -y vim

CMD [“echo”,”Image created”] 

# Managed Identity and Key Vault with ASP.NET Core

> Build an ASP.NET Core Web API using Managed Identity, Key Vault, and Cosmos DB that is designed to be deployed to Azure App Service or Azure Kubernetes Service (AKS) as a Docker container

![License](https://img.shields.io/badge/license-MIT-green.svg)
![Docker Image Build](https://github.com/retaildevcrews/helium-csharp/workflows/Docker%20Image%20Build/badge.svg)

This is an ASP.NET Core Web API reference application designed to "fork and code" with the following features:

- Securely build, deploy and run an Azure App Service (Web App for Containers) application
- Securely build, deploy and run an Azure Kubernetes Service (AKS) application
- Use Managed Identity to securely access resources
- Securely store secrets in Key Vault
- Securely build and deploy the Docker container from Azure Container Registry (ACR)
- Connect to and query Cosmos DB
- Automatically send telemetry and logs to Azure Monitor

> Instructions for setting up Key Vault, ACR, Azure Monitor and Cosmos DB are in the Helium [readme](https://github.com/retaildevcrews/helium)

## Prerequisites

- Bash shell (tested on Mac, Ubuntu, Windows with WSL2)
  - Will not work with WSL1
  - Docker commands will not work in Cloud Shell
- .NET Core SDK 3.1 ([download](https://dotnet.microsoft.com/download))
- Docker CLI ([download](https://docs.docker.com/install/))
- Azure CLI ([download](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest))
- Visual Studio Code (optional) ([download](https://code.visualstudio.com/download))

## Setup

- Fork this repo and clone to your local machine
- All instructions assume starting from the root of the repo

### Run the application locally

- The application requires Key Vault and Cosmos DB to be setup per the Helium [readme](https://github.com/retaildevcrews/helium)
- You can run the application locally by using Azure CLI cached credentials

```bash

# make sure you are logged into Azure
az account show

# if not log in
az login

# make sure you are in the root of the repo then
cd src/app
dotnet restore

# run in the background
# $He_Name is set to the name of your key vault during setup
# this will use Azure CLI cached credentials

dotnet run -- --keyvault-name $He_Name --auth-type CLI &

# test the application
# the application takes about 15 seconds to start

curl http://localhost:4120/healthz/ietf

# Stop the app
fg

# press ctl-c

```

### Run the application as a local container

> The docker-dev image should not be pushed to a repo

Build a full .NET Core SDK image with Azure CLI installed in the container to support basic development from the container

- Before building the local image, check your uid and gid using the `id` command
  - If neither your uid or gid is 1000, you need to edit `Dockerfile-Dev`
    - change `useradd -u 1000 ...` to use your uid or gid
- This is necessary in order for the local .azure credentials to work within the container
  - Mounting the volume (with the -v option) prevents your Azure credentials from accidentally getting pushed to a repo

- We use a multi-stage docker build as installing the prerequisites and Azure CLI takes a while
  - If you want to build a "permanent cache" of the first stage (so that docker system prune doesn't delete it), you can run this command first
  - `docker build . --target helium-dev-base -t helium-dev-base -f Dockerfile-Dev`

- Customizing your environment with dotFiles
  - If you want to use git from within the container, you should copy your `~/.gitconfig` to `dotFiles` before building the container
  - You can also copy your `~/.bashrc` file to `dotFiles`
    - Ensure you don't accidentally copy any credentials or secrets!
  - `.gitignore` will not commit any files in `dotFiles` that begin with `.`
    - update `.gitignore` for any other files to exclude

- VS Code development within a container
  - Read more about it [here](https://code.visualstudio.com/docs/remote/containers) It's super cool!

```bash

# make sure you are in the root of the repo

# build the dev image
docker build . -t helium-dev -f Dockerfile-Dev

```

#### Run the container

This will launch a bash shell within the container

```bash

docker run -it --rm -p 4120:4120 --name helium-dev \
--env KEYVAULT_NAME=$He_Name \
-v ~/.azure:/home/helium/.azure helium-dev

# make sure AZ CLI works
az account show

# make sure git works
git status

# run the app
cd ~/helium-csharp/src/app

# if you passed your Keyvault name in the env var
dotnet run

# full command
dotnet run -- --auth-type CLI --keyvault-name your-keyvault-name

# exit (the container is automatically removed via the --rm option)
exit

```

## Build the release container using Docker

> For instructions on building the container with ACR, please see the Helium [readme](https://github.com/retaildevcrews/helium)

```bash

# make sure you are in the root of the repo
# build the image
docker build . -t helium-csharp

# run docker tag and docker push to push to your repo

```

## Build the release container using GitHub Actions

- TODO - explain how to update GitHub actions

## Contributing

This project welcomes contributions and suggestions. Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit [Microsoft Contributor License Agreement](https://cla.opensource.microsoft.com).

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

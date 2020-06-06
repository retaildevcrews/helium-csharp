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

- Bash shell (tested on Mac, Ubuntu, Windows with WSL2, CloudSpaces)
  - Will not work with WSL1 or Cloud Shell
- .NET Core SDK 3.1 ([download](https://dotnet.microsoft.com/download))
- Docker CLI ([download](https://docs.docker.com/install/))
- Azure CLI ([download](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest))
- Visual Studio Code (optional) ([download](https://code.visualstudio.com/download))

## Setup

### Cloud Spaces

- The easiest way to experiment with helium is using [CloudSpaces](https://visualstudio.microsoft.com/services/visual-studio-codespaces/)
  - Fork this repo
  - Create a new CloudSpace pointing to the forked repo
  - Use the built-in bash shell

### Local Setup

- Fork this repo
- Clone the forked repo to your local machine
- All instructions assume starting from the root of the repo

## Run helium from bash

- The application requires Key Vault and Cosmos DB to be setup per the Helium setup [instructions](https://github.com/retaildevcrews/helium)
- You can run the application locally by using Azure CLI cached credentials

### Validate az CLI works

```bash

# make sure you are logged into Azure
az account show

# if not, log in
az login

```

### Verify Key Vault Access

```bash

# verify you have access to Key Vault
az keyvault secret show --name CosmosDatabase --vault-name $He_Name

```

### Run the app

> If you are using CodeSpaces, you just need to set the --keyvault-name parameter and press F5

```bash

# make sure you are in the root of the repo
cd src/app
dotnet restore

# run in the background
# $He_Name is set to the name of your key vault during setup
# this will use Azure CLI cached credentials

dotnet run -- --keyvault-name $He_Name --auth-type CLI

# press ctl-c to stop the app

```

### In a separate terminal

```bash

# test the application
curl localhost:4120/version

```

### Deep Testing

We use [Web Validate](https://github.com/retaildevcrews/webvalidate) to run deep verification tests on the Web API

```bash

# install Web Validate as a dotnet global tool
# this is automatically installed in CodeSpaces
dotnet tool install -g webvalidate

# make sure you are in the root of the repository

# run the validation tests
# validation tests are located in the TestFiles directory
webv -s localhost:4120 -f baseline.json

# bad.json tests error conditions that return 4xx codes

# benchmark.json is a 300 request test that covers the entire API

```

## Build the release container using Docker

> For instructions on building the container with ACR, please see the Helium [readme](https://github.com/retaildevcrews/helium)

```bash

# make sure you are in the root of the repo
# build the image
docker build . -t helium-csharp

# run docker tag and docker push to push to your repo

```

## CI-CD

This repo uses [GitHub Actions](/.github/workflows/dockerCI.yml) for Continuous Integration. Detailed setup instructions are here: [ACR](https://github.com/retaildevcrews/helium/blob/master/docs/CI-CD/ACR.md) [DockerHub](https://github.com/retaildevcrews/helium/blob/master/docs/CI-CD/DockerHub.md)

- CI supports pushing to Azure Container Registry or DockerHub
- The action is setup to execute on a PR or commit to ```master```
  - The action does not run on commits to branches other than ```master```
- The action always publishes an image with the ```:beta``` tag
- If you tag the repo with a version i.e. ```v1.0.8``` the action will also
  - Tag the image with ```:1.0.8```
  - Tag the image with ```:stable```
  - Note that the ```v``` is case sensitive (lower case)

CD is supported via webhooks in Azure App Services connected to the ACR or DockerHub repository. See the Helium setup instructions for details.

### Testing

The end-to-end test is executed as part of the CI-CD pipeline and requires the Azure Service Principal be setup per the instructions above and the following `secrets` be set in your GitHub repo:

- Azure Login Information
  - TENANT
  - SERVICE_PRINCIPAL
  - SERVICE_PRINCIPAL_SECRET

### Pushing to Azure Container Registry

In order to push to ACR, you must create a Service Principal that has push permissions to the ACR and set the following `secrets` in your GitHub repo:

- ACR Information
  - ACR_REG
  - ACR_REPO
  - ACR_IMAGE

### Pushing to DockerHub

In order to push to DockerHub, you must set the following `secrets` in your GitHub repo:

- DOCKER_REPO
- DOCKER_USER
- DOCKER_PAT
  - Personal Access Token

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

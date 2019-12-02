# Build a Docker containerized ASP.NET Core application using App Service / AKS, CosmosDB, Managed Identity and Key Vault

![License](https://img.shields.io/badge/license-MIT-green.svg)

This is an ASP.NET Core WebAPI reference application designed to "fork and code" with the following features:

- Securely build, deploy and run an App Service (Web App for Containers) application
- Securely build, deploy and run an Azure Kubernetes Service application
- Use Managed Identity to securely access resources
- Securely store secrets in Key Vault
- Securely build and deploy the Docker container from Container Registry or Azure DevOps
- Connect to and query CosmosDB
- Automatically send telemetry and logs to Azure Monitor
- Instructions for setting up Key Vault, ACR, Azure Monitor and CosmosDB are in the Helium [readme](https://github.com/retaildevcrews/helium)

## Prerequisites

- Docker CLI ([download](https://docs.docker.com/install/))
- .NET Core SDK 2.2 ([download](https://dotnet.microsoft.com/download))
- Azure CLI 2.0.72+ ([download](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest))
- Visual Studio Code (optional) ([download](https://code.visualstudio.com/download))

## Setup

- Fork this repo and clone to your local machine
  - All instructions assume starting from the root of the repo

Build the container using Docker

- The unit tests run as part of the Docker build process. You can also run the unit tests manually.
- For instructions on building the container with ACR, please see the Helium [readme](https://github.com/retaildevcrews/helium)

```bash

# make sure you are in the root of the repo
# build the image
docker build -t helium-csharp -f Dockerfile .

# Tag and push the image to your Docker repo

```

Run the application locally

- The application requires Key Vault and CosmosDB to be setup per the Helium [readme](https://github.com/retaildevcrews/helium)

```bash

# make sure you are in the root of the repo
cd src/app

# run in the background
dotnet run $He_Name &

# test the application
# the application takes about 10 seconds to start
curl http://localhost:4120/healthz

# Stop the app
fg

# press ctl-c

```

Run the application as a local container instead

```bash

# make sure you are in the root of the repo
cd ../..

# build the dev image
# docker-dev builds a full .NET Core SDK image with Azure CLI installed in the container
# you may see red warnings in the build output, they are safe to ignore
# examples: "debconf: ..." or "dpkg-preconfigure: ..."
docker build -t helium-dev -f Dockerfile-Dev .

# run the container
# mount your ~/.azure directory to container root/.azure directory
# you can also run the container and run az login from a bash shell
docker run -d -p 4120:4120 --name helium-dev -v ~/.azure:/root/.azure helium-dev "dotnet" "run" "${He_Name}"

# check the logs
# re-run until the application started message appears
docker logs helium-dev

# curl the health check endpoint
curl http://localhost:4120/healthz


# Stop and remove the container

docker stop helium-dev
docker rm helium-dev

```

Setup CI-CD with Azure DevOps

- The [pipeline file](azure-pipelines.yml) contains the build definition for this app
- You will need to setup a "Container Registry Service Connection" in Azure DevOps before importing the build pipeline
- The pipeline defines "helium" as the name of the service connection
- You can change this to an existing service connection or create a new service connection called helium
- If you use a different name, make sure to update the pipeline

Creating a new Azure DevOps project

- Open Azure DevOps
- Click on New Project
- Enter the project information
- Click on Create

Adding a Service Connection

- Click on the project created above
- Click on Project Settings
- Click on Service connections (under Pipelines heading)
- Click on New service connection
- Select Docker Registry
- Select Azure Container Registry
- Enter helium in the Connection name field
- Select your Azure Subscription
- Select your Container Registry
- Ensure Allow all pipelines to use this connection is checked
- Click OK

Adding a pipeline

- Click on Pipelines
- Click on Create your first Pipeline
- Select the repo that your code was forked to
- Click run

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

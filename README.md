---
page_type: sample
languages:
  - csharp
products:
  - aspnet-core
  - azure-app-service
  - azure-cosmos-db
  - azure-devops
  - azure-container-registry
  - azure-key-vault
  - azure-monitor
roles:
  - developer
  - devops-engineer
  - solution-architect
levels:
  - beginner

description: "A sample ASP.NET Core WebAPI for bootstrapping your next App Service app using Managed Identity and Key Vault"

urlFragment: "/azure-samples/app-service-managed-identity-key-vault-csharp"
---

# Build an ASP.NET Core application using App Service, Managed Identity and Key Vault

![License](https://img.shields.io/badge/license-MIT-green.svg)
![Build Status](https://bartr-ms.visualstudio.com/bluebell/_apis/build/status/bartr.bluebell?branchName=master)

This sample is an ASP.NET Core WebAPI application designed to "fork and code" with the following features:

- Securely build, deploy and run an App Service (Web App for Containers) application
- Use Managed Identity to securely access resources
- Securely store secrets in Key Vault
- Securely build and deploy the Docker container from Container Registry or Azure DevOps
- Connect to and query CosmosDB
- Automatically send telemetry and logs to Azure Monitor

![alt text](./docs/images/architecture.jpg "Architecture Diagram")

## Contents

| File/folder           | Description                             |
| --------------------- | --------------------------------------- |
| `.gitignore`          | Define what to ignore at commit time    |
| `azure-pipelines.yml` | Azure DevOps CI-CD Pipeline             |
| `CHANGELOG.md`        | Repo change log                         |
| `CODE_OF_CONDUCT.md`  | Microsoft Open Source Code of Conduct   |
| `CONTRIBUTING.md`     | Guidelines for contributing to the repo |
| `LICENSE`             | The license for the sample              |
| `README.md`           | This README file                        |
| `SECURITY.md`         | Microsoft Security information          |
| `src`                 | Source code and tests                   |

## Prerequisites

- Azure subscription with permissions to create:
  - Resource Groups, Service Principals, Keyvault, CosmosDB, App Service, Azure Container Registry, Azure Monitor
- Bash shell (tested on Mac, Ubuntu, Windows with WSL2)
  - Will not work in Cloud Shell unless you have a remote dockerd
- Azure CLI 2.0.72+ ([download](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest))
- Docker CLI ([download](https://docs.docker.com/install/))
- .NET Core SDK 2.2 ([download](https://dotnet.microsoft.com/download))
- Visual Studio Code (optional) ([download](https://code.visualstudio.com/download))

## Setup

- Fork this repo and clone to your local machine
  - cd to the base directory of the repo

Login to Azure and select subscription

```bash

az login

# show your Azure accounts
az account list -o table

# select the Azure account
az account set -s {subscription name or Id}

```

Choose a unique DNS name

```bash

# this will be the prefix for all resources
# do not include punctuation - only use a-z and 0-9
# must be at least 5 characters long
# must start with a-z (only lowercase)
export He_Name="youruniquename"

### if true, change He_Name
az cosmosdb check-name-exists -n ${He_Name}

### if nslookup doesn't fail to resolve, change He_Name
nslookup ${He_Name}.azurewebsites.net
nslookup ${He_Name}.vault.azure.net
nslookup ${He_Name}.azurecr.io

```

Create Resource Groups

- When experimenting with this sample, you should create new resource groups to avoid accidentally deleting resources

  - If you use an existing resource group, please make sure to apply resource locks to avoid accidentally deleting resources

- You will create 3 resource groups
  - One for CosmosDB
  - One for ACR
  - One for App Service, Key Vault and Azure Monitor

```bash

# set location
export He_Location=centralus

# resource group names
export He_ACR_RG=${He_Name}-rg-acr
export He_App_RG=${He_Name}-rg-app
export He_Cosmos_RG=${He_Name}-rg-cosmos

# create the resource groups
az group create -n $He_App_RG -l $He_Location
az group create -n $He_ACR_RG -l $He_Location
az group create -n $He_Cosmos_RG -l $He_Location

```

Save your environment variables for ease of reuse and picking up where you left off.

```bash

# run the saveenv.sh script at any time to save He_* variables to ~/${He_Name}.env
# make sure you are in the root of the repo
./saveenv.sh

# at any point if your terminal environment gets cleared, you can source the file
# you only need to remember the name of the env file (or set the $He_Name variable again)
source ~/{yoursameuniquename}.env

```

Create and load sample data into CosmosDB

- This takes several minutes to run
- This sample is designed to use a simple dataset from IMDb of 100 movies and their associated actors and genres
  - See full explanation of data model design decisions [here:](https://github.com/4-co/imdb)

```bash

# set the other Cosmos environment variables
export He_Cosmos_URL=https://${He_Name}.documents.azure.com:443/
export He_Cosmos_DB=imdb
export He_Cosmos_Col=movies

# create the CosmosDB server
az cosmosdb create -g $He_Cosmos_RG -n $He_Name

# create the database
az cosmosdb database create -d $He_Cosmos_DB -g $He_Cosmos_RG -n $He_Name

# create the collection
# 400 is the minimum RUs
# /partitionKey is the partition key
# partition key is the id mod 10
az cosmosdb collection create --throughput 400 --partition-key-path /partitionKey -g $He_Cosmos_RG -n $He_Name -d $He_Cosmos_DB -c $He_Cosmos_Col

# get Cosmos readonly key (used by App Service)
export He_Cosmos_RO_Key=$(az cosmosdb keys list -n $He_Name -g $He_Cosmos_RG --query primaryReadonlyMasterKey -o tsv)

# get readwrite key (used by the imdb import)
export He_Cosmos_RW_Key=$(az cosmosdb keys list -n $He_Name -g $He_Cosmos_RG --query primaryMasterKey -o tsv)

# run the IMDb Import
docker run -it --rm fourco/imdb-import 100Movies $He_Name $He_Cosmos_RW_Key $He_Cosmos_DB $He_Cosmos_Col

# Optional: Run ./saveenv.sh to save latest variables

```

Create Azure Key Vault

- All secrets are stored in Azure Key Vault for security
  - This sample uses Managed Identity to access Key Vault

```bash

## create the Key Vault and add secrets
az keyvault create -g $He_App_RG -n $He_Name

# add CosmosDB keys
az keyvault secret set -o table --vault-name $He_Name --name "CosmosUrl" --value $He_Cosmos_URL
az keyvault secret set -o table --vault-name $He_Name --name "CosmosKey" --value $He_Cosmos_RO_Key
az keyvault secret set -o table --vault-name $He_Name --name "CosmosDatabase" --value $He_Cosmos_DB
az keyvault secret set -o table --vault-name $He_Name --name "CosmosCollection" --value $He_Cosmos_Col

```

(Optional) In order to run the application locally, each developer will need access to the Key Vault. Since you created the Key Vault during setup, you will automatically have permission, so this step is only required for additional developers.

Use the following command to grant permissions to each developer that will need access.

```bash

# get the object id for each developer (optional)
export dev_Object_Id=$(az ad user show --id {developer email address} --query objectId -o tsv)

# grant Key Vault access to each developer (optional)
az keyvault set-policy -n $He_Name --secret-permissions get list --key-permissions get list --object-id $dev_Object_Id

```

Run the unit tests

- The unit tests run as part of the Docker build process. You can also run the unit tests manually.

```bash

cd src/unit-tests

dotnet test

```

Run the application locally

```bash

cd ../app

# run in the background
dotnet run $He_Name &

# test the application
# the application takes about 10 seconds to start
curl http://localhost:4120/healthz

```

(Alternative) Run the application as a local container

```bash

# make sure you are in the src folder
cd ..

# build the dev image
# you may see red warnings in the build output, they are safe to ignore
# examples: "debconf: ..." or "dpkg-preconfigure: ..."
docker build -t helium-dev -f Dockerfile-Dev .

# run the container
# mount your ~/.azure directory to container root/.azure directory
docker run -d -p 4120:4120 --name helium-dev -v ~/.azure:/root/.azure helium-dev "dotnet" "run" "${He_Name}"

# check the logs
# re-run until the application started message appears
docker logs helium-dev

# curl the health check endpoint
curl http://localhost:4120/healthz

```

Run the Integration Test

- Make sure the app is running per previous step

```bash

cd ../integration-test

dotnet run -- -h http://localhost:4120

# run the following to see complete usage options
dotnet run -- --help

cd ..

```

Stop the app

```bash

fg

# press ctl-c

```

(Alternative) Stop and remove the container

```bash

docker stop ${He_Name}

docker rm ${He_Name}

```

Setup Container Registry

- Create the Container Registry with admin access _disabled_

```bash

# create the ACR
az acr create --sku Standard --admin-enabled false -g $He_ACR_RG -n $He_Name

# Login to ACR
az acr login -n $He_Name

# If you get an error that the login server isn't available, it's a DNS issue that will resolve in a minute or two, just retry

# Build the container with az acr build
### Make sure you are in the src directory
az acr build -r $He_Name -t $He_Name.azurecr.io/helium-csharp .

```

Create Azure Monitor

- The Application Insights extension is in preview and needs to be added to the CLI

```bash

# Add App Insights extension
az extension add -n application-insights

# Create App Insights
export He_AppInsights_Key=$(az monitor app-insights component create -g $He_App_RG -l $He_Location -a $He_Name --query instrumentationKey -o tsv)

# add App Insights Key to Key Vault
az keyvault secret set -o table --vault-name $He_Name --name "AppInsightsKey" --value $He_AppInsights_Key

# Optional: Run ./saveenv.sh to save latest variables

```

Create a Service Principal for Container Registry

- App Service will use this Service Principal to access Container Registry

```bash

# create a Service Principal
export He_SP_PWD=$(az ad sp create-for-rbac -n http://${He_Name}-acr-sp --query password -o tsv)
export He_SP_ID=$(az ad sp show --id http://${He_Name}-acr-sp --query appId -o tsv)

# get the Container Registry Id
export He_ACR_Id=$(az acr show -n $He_Name -g $He_ACR_RG --query "id" -o tsv)

# assign acrpull access to Service Principal
az role assignment create --assignee $He_SP_ID --scope $He_ACR_Id --role acrpull

# add credentials to Key Vault
az keyvault secret set -o table --vault-name $He_Name --name "AcrUserId" --value $He_SP_ID
az keyvault secret set -o table --vault-name $He_Name --name "AcrPassword" --value $He_SP_PWD

# Optional: Run ./saveenv.sh to save latest variables

```

Create and configure App Service (Web App for Containers)

- App Service will fail to start until configured properly

```bash

# create App Service plan
az appservice plan create --sku B1 --is-linux -g $He_App_RG -n ${He_Name}-plan

# create Web App for Containers
az webapp create --deployment-container-image-name hello-world -g $He_App_RG -n $He_Name -p ${He_Name}-plan

# assign Managed Identity
export He_MSI_ID=$(az webapp identity assign -g $He_App_RG -n $He_Name --query principalId -o tsv)

# grant Key Vault access to Managed Identity
az keyvault set-policy -n $He_Name --secret-permissions get list --key-permissions get list --object-id $He_MSI_ID

### Configure Web App

# turn on CI
export He_CICD_URL=$(az webapp deployment container config -n $He_Name -g $He_App_RG --enable-cd true --query CI_CD_URL -o tsv)

# add the webhook
az acr webhook create -r $He_Name -n ${He_Name} --actions push --uri $He_CICD_URL --scope helium-csharp:latest

# set the Key Vault name app setting (environment variable)
az webapp config appsettings set --settings KeyVaultName=$He_Name -g $He_App_RG -n $He_Name

# turn on container logging
# this will send stdout and stderr to the logs
az webapp log config --docker-container-logging filesystem -g $He_App_RG -n $He_Name

# get the Service Principal Id and Key from Key Vault
export He_AcrUserId=$(az keyvault secret show --vault-name $He_Name --name "AcrUserId" --query id -o tsv)
export He_AcrPassword=$(az keyvault secret show --vault-name $He_Name --name "AcrPassword" --query id -o tsv)

# Optional: Run ./saveenv.sh to save latest variables

# configure the Web App to use Container Registry
# get Service Principal Id and Key from Key Vault
az webapp config container set -n $He_Name -g $He_App_RG \
-i ${He_Name}.azurecr.io/helium-csharp \
-r https://${He_Name}.azurecr.io \
-u "@Microsoft.KeyVault(SecretUri=${He_AcrUserId})" \
-p "@Microsoft.KeyVault(SecretUri=${He_AcrPassword})"

# restart the Web App
az webapp restart -g $He_App_RG -n $He_Name

# curl the health check endpoint
# this will eventually work, but may take a minute or two
# you may get a 403 error, if so, just run again
curl https://${He_Name}.azurewebsites.net/healthz

```

Run Integration Test

```bash

cd src/integration-test

dotnet run -- -h https://${He_Name}.azurewebsites.net

```

(Alternative) Run the Integration Test as a Docker container

```bash

# build the image
# from the src directory
docker build -t helium-integration -f Dockerfile-Integration-Test .

# run the tests in the container
docker run -it --rm helium-integration -h https://${He_Name}.azurewebsites.net

```

Setup CI-CD with Azure DevOps

- The [pipeline file](azure-pipelines.yml) contains the build definition for this sample
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

## Key concepts

This sample is an ASP.NET Core WebAPI application designed to "fork and code" with the following features:

- Securely build, deploy and run an App Service (Web App for Containers) application
- Use Managed Identity to securely access resources
- Securely store secrets in Key Vault
- Securely build and deploy the Docker container from Container Registry or Azure DevOps
- Connect to and query CosmosDB
- Automatically send telemetry and logs to Azure Monitor

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

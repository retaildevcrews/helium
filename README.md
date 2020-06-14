# Managed Identity and Key Vault with App Services

> Build a Web API reference application using Managed Identity, Key Vault, and Cosmos DB that is designed to be deployed to Azure App Service or Azure Kubernetes Service (AKS)

![License](https://img.shields.io/badge/license-MIT-green.svg)

This is a Web API reference application designed to "fork and code" with the following features:

- Securely build, deploy and run an Azure App Service (Web App for Containers) application
- Securely build, deploy and run an Azure Kubernetes Service (AKS) application
- Use Managed Identity to securely access resources
- Securely store secrets in Key Vault
- Securely build and deploy the Docker container from Azure Container Registry (ACR) or Azure DevOps
- Connect to and query Cosmos DB
- Automatically send telemetry and logs to Azure Monitor

![alt text](./docs/images/architecture.jpg "Architecture Diagram")

## Prerequisites

- Azure subscription with permissions to create:
  - Resource Groups, Service Principals, Key Vault, Cosmos DB, Azure Container Registry, Azure Monitor, App Service or AKS
- Bash shell (tested on Cloudspaces Mac, Ubuntu, Windows with WSL2)
  - Will not work in Cloud Shell or WSL1
- Azure CLI ([download](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest))
- Docker CLI ([download](https://docs.docker.com/install/))
- Visual Studio Code (optional) ([download](https://code.visualstudio.com/download))

## Setup

### Codespaces

```bash

### TODO - include Codespaces instructions

# TODO - change this in each language repo
export He_Repo=helium-csharp

```

### bash

Choose which Helium language implementation you want to use and clone the repo

```bash

# run one of these commands

# dotnet (C#)
git clone https://github.com/retaildevcrews/helium-csharp helium

# Java (Spring-Boot)
git clone https://github.com/retaildevcrews/helium-java helium

# TypeScript (Restify)
git clone https://github.com/retaildevcrews/helium-typescript helium

cd helium

```

Login to Azure and select subscription

```bash

az login

# show your Azure accounts
az account list -o table

# select the Azure account
az account set -s {subscription name or Id}

export He_Sub=$(az account show --subscription {subscription name or Id} --output tsv |awk '{print $3}')

```

Choose a unique DNS name

```bash

### TODO - need to install nslookup in each .devcontainer setup script
# sudo apt-get install ... dnsutils

# this will be the prefix for all resources
# only use a-z and 0-9 - do not include punctuation or uppercase characters
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

- When experimenting with this app, you should create new resource groups to avoid accidentally deleting resources

  - If you use an existing resource group, please make sure to apply resource locks to avoid accidentally deleting resources

- You will create 3 resource groups
  - One for ACR
  - One for App Service or AKS, Key Vault and Azure Monitor
  - One for Cosmos DB (see create and load sample data to Cosmos DB step)

```bash

# set location
export He_Location=centralus

# resource group names
export He_ACR_RG=${He_Name}-rg-acr
export He_App_RG=${He_Name}-rg-app

# create the resource groups
az group create -n $He_App_RG -l $He_Location
az group create -n $He_ACR_RG -l $He_Location

```

Save your environment variables for ease of reuse and picking up where you left off.

```bash

# run the saveenv.sh script at any time to save He_*, Imdb_*, MSI_*, and AKS_* variables to ~/${He_Name}.env
# make sure you are in the root of the repo
./saveenv.sh

# at any point if your terminal environment gets cleared, you can source the file to reload the environment variables
source ~/.helium.env

```

Create and load sample data into Cosmos DB

- This takes several minutes to run
- This reference app is designed to use a simple dataset from IMDb of 1300 movies and their associated actors and genres
- Follow the guidance in the [IMDb Repo](https://github.com/retaildevcrews/imdb) to create a Cosmos DB server (SQL API), a database, and a collection and then load the IMDb data. The repo readme also provides an explanation of the data model design decisions.
- Recommendation is to set $Imdb_Name the same value as $He_Name

Create Azure Key Vault

- All secrets are stored in Azure Key Vault for security
  - This app uses Managed Identity to access Key Vault

```bash

## create the Key Vault and add secrets
az keyvault create -g $He_App_RG -n $He_Name

# Run saveenv.sh to save the Imdb variables
./saveenv.sh -y

```

In order to run the application locally, each developer will need access to the Key Vault. Since you created the Key Vault during setup, you will automatically have permission, so this step is only required for additional developers.

Use the following command to grant permissions to each developer that will need access.

> This step is optional

```bash

# get the object ID by email address for each developer
az keyvault set-policy -n $He_Name --secret-permissions get list --key-permissions get list --object-id \
$(az ad user show --query objectId -o tsv --id {developer email address})

# grant Key Vault access to each developer (optional)
az keyvault set-policy -n $He_Name --secret-permissions get list --key-permissions get list --object-id $dev_Object_Id

```

Create and load sample data into Cosmos DB

- This takes several minutes to run
- This reference app is designed to use a simple dataset from IMDb of 1300 movies and their associated actors and genres
- Follow the guidance in the [IMDb Repo](https://github.com/retaildevcrews/imdb) to create a Cosmos DB server (SQL API), a database, and a collection and then load the IMDb data. The repo readme also provides an explanation of the data model design decisions.
- Recommendation is to set $Imdb_Name the same value as $He_Name

Save the Cosmos DB keys to Key Vault

```bash

# add Cosmos DB config to Key Vault
az keyvault secret set -o table --vault-name $He_Name --name "CosmosUrl" --value https://${Imdb_Name}.documents.azure.com:443/
az keyvault secret set -o table --vault-name $He_Name --name "CosmosKey" --value $(az cosmosdb keys list -n $Imdb_Name -g $Imdb_RG --query primaryReadonlyMasterKey -o tsv)
az keyvault secret set -o table --vault-name $He_Name --name "CosmosRWKey" --value $(az cosmosdb keys list -n $Imdb_Name -g $Imdb_RG --query primaryMasterKey -o tsv)
az keyvault secret set -o table --vault-name $He_Name --name "CosmosDatabase" --value $Imdb_DB
az keyvault secret set -o table --vault-name $He_Name --name "CosmosCollection" --value $Imdb_Col

# retrieve the keys using eval $Imdb_RO_Key
export Imdb_RO_Key='az keyvault secret show -o tsv --query value --vault-name $He_Name --name CosmosKey'
export Imdb_RW_Key='az keyvault secret show -o tsv --query value --vault-name $He_Name --name CosmosRWKey'

# Run saveenv.sh to save the Imdb variables
./saveenv.sh -y

```

Setup Container Registry

- Create the Container Registry with admin access _disabled_

> Currently, App Service cannot access ACR via the Managed Identity, so we have to setup a separate Service Principal and grant access to that SP.

```bash

# create the ACR
az acr create --sku Standard --admin-enabled false -g $He_ACR_RG -n $He_Name

# Login to ACR
az acr login -n $He_Name

# if you get an error that the login server isn't available, it's a DNS issue that will resolve in a minute or two, just retry

# Pull the image from the repo
docker pull retaildevcrew/$He_Repo:stable

# tag the image
docker tag retaildevcrew/$He_Repo:stable $He_Name.azurecr.io/${He_Repo}:latest

# push the image to ACR
docker push $He_Name.azurecr.io/${He_Repo}:latest

```

Create a Service Principal for Container Registry

- App Service will use this Service Principal to access Container Registry

```bash

# create a Service Principal and add password to Key Vault
az keyvault secret set -o table --vault-name $He_Name --name "AcrPassword" --value $(az ad sp create-for-rbac -n http://${He_Name}-acr-sp --query password -o tsv)

# add Service Principal ID to Key Vault
az keyvault secret set -o table --vault-name $He_Name --name "AcrUserId" --value $(az ad sp show --id http://${He_Name}-acr-sp --query appId -o tsv)

# retrive the values using eval $He_SP_PWD
export He_SP_PWD='az keyvault secret show -o tsv --query value --vault-name $He_Name --name AcrPassword'
export He_SP_ID='az keyvault secret show -o tsv --query value --vault-name $He_Name --name AcrUserId'

# get the Container Registry Id
export He_ACR_Id=$(az acr show -n $He_Name -g $He_ACR_RG --query "id" -o tsv)

# assign acrpull access to Service Principal
az role assignment create --assignee $(eval $He_SP_ID) --scope $He_ACR_Id --role acrpull

# save the environment variables
./saveenv.sh -y

```

Create Azure Monitor

- The Application Insights extension is in preview and needs to be added to the CLI

```bash

# Add App Insights extension
az extension add -n application-insights
az feature register --name AIWorkspacePreview --namespace microsoft.insights
az provider register -n microsoft.insights

# Create App Insights
az monitor app-insights component create -g $He_App_RG -l $He_Location -a $He_Name --query instrumentationKey -o table

# add App Insights Key to Key Vault
az keyvault secret set -o tsv --query name --vault-name $He_Name --name "AppInsightsKey" --value $(az monitor app-insights component show -g $He_App_RG -a $He_Name --query instrumentationKey -o tsv)

# save the env variable - use via $(eval $He_AppInsights_Key)
export He_AppInsights_Key='az keyvault secret show -o tsv --query value --vault-name $He_Name --name AppInsightsKey'

# save the environment variables
./saveenv.sh -y

```

Deploy the container to App Service or AKS

- Instructions for [App Service](docs/AppService.md)
- Instructions for [AKS](docs/aks/README.md#L233) (currently requires csharp)

## Dashboard setup

Replace the values in the `Helium_Dashboard.json` file surrounded by `%%` with the proper environment variables
after making sure the proper environment variables are set (He_Sub, He_App_RG, Imdb_RB and Imdb_Name)

```bash

cd $REPO_ROOT/docs/dashboard
sed -i "s/%%SUBSCRIPTION_GUID%%/${He_Sub}/g" Helium_Dashboard.json && \
sed -i "s/%%He_App_RG%%/${He_App_RG}/g" Helium_Dashboard.json && \
sed -i "s/%%Imdb_RG%%/${Imdb_RG}/g" Helium_Dashboard.json && \
sed -i "s/%%Imdb_NAME%%/${Imdb_Name}/g" Helium_Dashboard.json

if [ "$He_Language" == "java" ];
then
  sed -i "s/%%He_Language%%/gelato/g" Helium_Dashboard.json
elif [ "$He_Language" == "csharp" ];
then
  sed -i "s/%%He_Language%%/bluebell/g" Helium_Dashboard.json
elif [ "$He_Language" == "typescript" ];
then
  sed -i "s/%%He_Language%%/sherbert/g" Helium_Dashboard.json
fi

```

Navigate to ([Dashboard](https://portal.azure.com/#dashboard)) within your Azure portal. Click upload and select the `Helium_Dashboard.json` file with your correct subscription GUID, resource group names, and app name.

For more documentation on creating and sharing Dashboards, see ([here](https://docs.microsoft.com/en-us/azure/azure-portal/azure-portal-dashboards)).

## Contributing

This project welcomes contributions and suggestions. Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit [Microsoft Contributor License Agreement](https://cla.opensource.microsoft.com).

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments

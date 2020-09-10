# Demo and Deployment Walk Through

## Background

Project Helium is a reusable Advocated Pattern (AdPat). The focus was originally Azure App Services (Web Apps for Containers). The goal is to have a best practices implementation of a C# (TypeScript/Node & Java Springboot are under development) + Cosmos DB + Key Vault + Azure Monitor application that project teams can use to build applications that support security, HA, DR and Business Continuity.

### Azure Components in Use

- Azure Container Registry
- Azure Kubernetes Service
  - Linkerd ServiceMesh
  - Prometheus
  - Azure Application Gateway Ingress Controller
  - Azure AAD Pod Identity
- Azure Key Vault
- Azure Cosmos DB
- Application Insights

## Demo Install

### Prerequisites

- Azure subscription with permissions to create:
  - Resource Groups, Service Principals, Keyvault, Cosmos DB, AKS, Azure Container Registry, Azure Monitor
- Bash shell (tested on Mac, Ubuntu, Windows with WSL2)
  - Will not work in Cloud Shell or WSL1
- Azure CLI ([download](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest))
- Docker CLI ([download](https://docs.docker.com/install/))
- Visual Studio Code (optional) ([download](https://code.visualstudio.com/download))
- kubectl (install by using `sudo az aks install-cli`)
- Helm v3 ([Install Instructions](https://helm.sh/docs/intro/install/))

### Setup

Fork this repo and clone to your local machine

```bash

cd $HOME

mkdir demo

cd demo

git clone https://github.com/retaildevcrews/helium

```

Change into the base directory of the repo

```bash

cd helium

export REPO_ROOT=$(pwd)

```

#### Login to Azure and select subscription

```bash

az login

# show your Azure accounts
az account list -o table

# select the Azure account
az account set -s {subscription name or Id}

```

This demo will create resource groups, a Cosmos DB instance, Key Vault, Azure Container Registry, and Azure App Service.

#### Choose a unique DNS name

```bash

# this will be the prefix for all resources
# do not include punctuation - only use a-z and 0-9
# must be at least 5 characters long
# must start with a-z (only lowercase)
export He_Name=[your unique name]

### if true, change He_Name
az cosmosdb check-name-exists -n ${He_Name}

### if nslookup doesn't fail to resolve, change He_Name
nslookup ${He_Name}.vault.azure.net
nslookup ${He_Name}.azurecr.io

```

#### Create Resource Groups

> When experimenting with this sample, you should create new resource groups to avoid accidentally deleting resources
>
> If you use an existing resource group, please make sure to apply resource locks to avoid accidentally deleting resources

- You will create 3 resource groups
  - One for ACR
  - One for App Service or AKS, Key Vault and Azure Monitor
  - One for Cosmos DB

```bash

# set location
export He_Location=centralus

# set the subscription
export He_Sub=$(az account show --query id -o tsv)

# resource group names
export Imdb_Name=$He_Name
export He_ACR_RG=${He_Name}-rg-acr
export He_App_RG=${He_Name}-rg-app
export Imdb_RG=${Imdb_Name}-rg-cosmos

# export Cosmos DB env vars
# these will be explained in the Cosmos DB setup step
export Imdb_Location=$He_Location
export Imdb_DB=imdb
export Imdb_Col=movies
export Imdb_RW_Key='az cosmosdb keys list -n $Imdb_Name -g $Imdb_RG --query primaryMasterKey -o tsv'

# create the resource groups
az group create -n $He_App_RG -l $He_Location
az group create -n $He_ACR_RG -l $He_Location
az group create -n $Imdb_RG -l $Imdb_Location

# run the saveenv.sh script at any time to save He_* variables to ~/.helium.env
# make sure you are in the root of the repo
cd $REPO_ROOT
./saveenv.sh

# if your terminal environment gets cleared, you can source the file to reload the environment variables
source ~/.helium.env

```

Create Azure Key Vault

- All secrets are stored in Azure Key Vault for security
  - Helium uses Managed Identity to access Key Vault in production

```bash

## create the Key Vault
az keyvault create -g $He_App_RG -n $He_Name

```

#### Create and load sample data into Cosmos DB

- This takes several minutes to run
- This reference app is designed to use a simple dataset from IMDb of 1300 movies and their associated actors and genres
- Follow the steps in the [IMDb Repo](https://github.com/retaildevcrews/imdb) to create a Cosmos DB server, database, and collection and load the sample IMDb data
  - The repo readme also provides an explanation of the data model design decisions

  > You can safely start with the Create Cosmos DB step
  >
  > The initial steps were completed above

Save the Cosmos DB config to Key Vault

```bash

# add Cosmos DB config to Key Vault
az keyvault secret set -o table --vault-name $He_Name --name "CosmosUrl" --value https://${Imdb_Name}.documents.azure.com:443/
az keyvault secret set -o table --vault-name $He_Name --name "CosmosKey" --value $(az cosmosdb keys list -n $Imdb_Name -g $Imdb_RG --query primaryReadonlyMasterKey -o tsv)
az keyvault secret set -o table --vault-name $He_Name --name "CosmosDatabase" --value $Imdb_DB
az keyvault secret set -o table --vault-name $He_Name --name "CosmosCollection" --value $Imdb_Col

# retrieve the Cosmos DB key using eval $Imdb_RO_Key
export Imdb_RO_Key='az keyvault secret show -o tsv --query value --vault-name $He_Name --name CosmosKey'

# save the Imdb variables
./saveenv.sh -y

```

#### Create Azure Monitor

> The Application Insights extension is in preview and needs to be added to the CLI

```bash

# Add App Insights extension
az extension add -n application-insights
az feature register --name AIWorkspacePreview --namespace microsoft.insights
az provider register -n microsoft.insights

# Create App Insights
az monitor app-insights component create -g $He_App_RG -l $He_Location -a $He_Name -o table

# add App Insights Key to Key Vault
az keyvault secret set -o tsv --query name --vault-name $He_Name --name "AppInsightsKey" --value $(az monitor app-insights component show -g $He_App_RG -a $He_Name --query instrumentationKey -o tsv)

# save the env variable - use eval $He_AppInsights_Key
export He_AppInsights_Key='az keyvault secret show -o tsv --query value --vault-name $He_Name --name AppInsightsKey'

# save the environment variables
./saveenv.sh -y

```

#### Setup Azure Container Registry

- Create the Container Registry with admin access `disabled`

```bash

# create the ACR
az acr create --sku Standard --admin-enabled false -g $He_ACR_RG -n $He_Name

```

Create your AKS Cluster

Set local variables to use in AKS deployment

```bash

export He_AKS_Name="${He_Name}-aks"

```

Determine the latest version of Kubernetes supported by AKS. It is recommended to choose the latest version not in preview for production purposes, otherwise choose the latest in the list.

```bash

az aks get-versions -l $He_Location -o table

export He_K8S_VER=1.16.9

```

```bash

# note: if you see the following failure, navigate to your .azure\ directory
# and delete the file "aksServicePrincipal.json":
#    Waiting for AAD role to propagate[################################    ]  90.0000%Could not create a
#    role assignment for ACR. Are you an Owner on this subscription?

az aks create --name $He_AKS_Name --resource-group $He_App_RG --location $He_Location --enable-cluster-autoscaler --min-count 3 --max-count 6 --node-count 3 --kubernetes-version $He_K8S_VER --attach-acr $He_Name  --no-ssh-key

az aks get-credentials -n $He_AKS_Name -g $He_App_RG

kubectl get nodes

```

Install AAD Pod Identity for the application

Change directories to the `docs/aks` folder and make the `aad-podid.sh` script executable. Running this shell script will deploy AAD Pod Identity to your cluster and assign a Managed Identity.

>NOTE: The second command below has a `.` then a space followed by `./aad-podid.sh ...` this is so the exported variables in the script persist after the script ends in the uder interactive shell

```bash

# add helm repo for aad pod identity
helm repo add aad-pod-identity https://raw.githubusercontent.com/Azure/aad-pod-identity/master/charts

export MI_Name=${He_Name}-mi

cd $REPO_ROOT/docs/aks
sudo chmod +x aad-podid.sh

. ./aad-podid.sh -a ${He_AKS_Name} -r ${He_App_RG} -m ${MI_Name}

cd $REPO_ROOT
./saveenv.sh

```

The last line of the output will explain the proper label annotation needed when deploying the application. This will be needed later during the application install

```bash

echo $MI_Name

```

### Set Keyvault Policy for MI User

```bash

az keyvault set-policy -n ${He_Name} --object-id ${MI_PrincID} --secret-permissions get list --key-permissions get list --certificate-permissions get list

```

## Install Helm 3

Install the latest version of Helm by download the latest [release](https://github.com/helm/helm/releases):

```bash

# mac os
OS=darwin-amd64 && \
REL=v3.2.4 && \ #Should be lastest release from https://github.com/helm/helm/releases
mkdir -p $HOME/.helm/bin && \
curl -sSL "https://get.helm.sh/helm-${REL}-${OS}.tar.gz" | tar xvz && \
chmod +x ${OS}/helm && mv ${OS}/helm $HOME/.helm/bin/helm
rm -R ${OS}

```

or

```bash

# Linux/WSL
OS=linux-amd64 && \
REL=v3.2.4 && \
mkdir -p $HOME/.helm/bin && \
curl -sSL "https://get.helm.sh/helm-${REL}-${OS}.tar.gz" | tar xvz && \
chmod +x ${OS}/helm && mv ${OS}/helm $HOME/.helm/bin/helm
rm -R ${OS}

```

Add the helm binary to your path and set Helm home:

```bash

export PATH=$PATH:$HOME/.helm/bin
export HELM_HOME=$HOME/.helm

```

>NOTE: This will only set the helm command during the existing terminal session. Copy the 2 lines above to your bash or zsh profile so that the helm command can be run any time.

Verify the installation with:

```bash

helm version

```

Add the required helm repositories

```bash

helm repo add stable https://kubernetes-charts.storage.googleapis.com
helm repo add aad-pod-identity https://raw.githubusercontent.com/Azure/aad-pod-identity/master/charts
helm repo add application-gateway-kubernetes-ingress https://appgwingress.blob.core.windows.net/ingress-azure-helm-package/
helm repo update

```

## Install Linkerd Service Mesh into the cluster

Download the Linkerd v2 CLI:

```bash

# macOS and linux/WSL

curl -sL https://run.linkerd.io/install | sh
export PATH=$PATH:$HOME/.linkerd2/bin

```

Install the Linkerd control plane in the linkerd namespace:

```bash

linkerd install | kubectl apply -f -

```

Validate the install with:

```bash

linkerd check

```

## Install the NGNIX ingress controller

Create a namespace for your ingress resources. There is a yaml file located in the clones repository under `$REPO_ROOT/docs/aks/cluster/manifests/ingress-nginx-namespace.yaml`

```bash

kubectl apply -f $REPO_ROOT/docs/aks/cluster/manifests/ingress-nginx-namespace.yaml

```

Use Helm to deploy an NGINX ingress controller

```bash

helm install ingress stable/nginx-ingress \
    --namespace ingress-nginx \
    --set controller.replicaCount=2 \
    --set controller.nodeSelector."beta\.kubernetes\.io/os"=linux \
    --set defaultBackend.nodeSelector."beta\.kubernetes\.io/os"=linux

```

Get the Public IP of your Ingress Controller. This will be used later in deploying the application.

```bash

export INGRESS_PIP=$(kubectl --namespace ingress-nginx get svc -l component=controller -o jsonpath='{.items[0].status.loadBalancer.ingress[0].ip}')

cd $REPO_ROOT
./saveenv.sh

```

## Deploy the needed componenets of helium, key rotator and the testing harness

An helm chart is included for the reference application ([helium](https://github.com/RetailDevCrews/helium-csharp))

Install the Helm Chart located in the cloned directory

```bash

cd $REPO_ROOT/docs/aks/cluster/charts/helium

```

A file called helm-config.yaml with the following contents that needs be to edited to fit the environment being deployed in. The file looks like this

```yaml
# Default values for helium.
# This is a YAML-formatted file.
# Declare variables to be passed into your templates.
labels:
  aadpodidbinding: %%MI_Name%% # Should be value of $MI_Name from the output of aad-podid.sh

image:
  repository: retaildevcrew # The specific repository created for this environment
  name: helium-csharp # The name of the image for the helium-csharp repo

annotations:
  linkerd.io/inject: enabled # Allows for the application to be injected into the Service Mesh

ingress:
  hosts:
    - host: %%INGRESS_PIP%%.nip.io # Replace the IP address with the IP of the nginx external IP (value of $INGRESS_PIP or run kubectl get svc -n ingress-nginx to see the correct IP)
      paths: /

keyVaultName: %%KV_Name%% # Replace with the name of the Key Vault that holds the secrets (value of $He_Name)

```

Replace the values in the file surrounded by `%%` with the proper environment variables

```bash

sed -i "s/%%MI_Name%%/${MI_Name}/g" helm-config.yaml && \
sed -i "s/%%INGRESS_PIP%%/${INGRESS_PIP}/g" helm-config.yaml && \
sed -i "s/%%KV_Name%%/${He_Name}/g" helm-config.yaml

```

This file can now be given to the the helm install as an override to the default values.

```bash

cd $REPO_ROOT/docs/aks/cluster/charts

helm install helium-aks helium -f ./helium/helm-config.yaml

```

optionally if you stored the helium-csharp image in your own Azure Container Registry you can change the registry at the command line as such:

```bash

helm install helium-aks helium --set image.repository=<acr_name>.azurecr.io -f helm-config.yaml

# curl the health check endpoint
curl ${INGRESS_PIP}.nip.io/healthz

```

## Dashboard setup

Replace the values in the `AKS_Dashboard.json` file surrounded by `%%` with the proper environment variables

```bash

cd $REPO_ROOT/docs/aks/demo
sed -i "s/%%SUBSCRIPTION_GUID%%/${He_Sub}/g" AKS_Dashboard.json && \
sed -i "s/%%AKS_RESOURCE_GROUP%%/${He_App_RG}/g" AKS_Dashboard.json && \
sed -i "s/%%COSMOS_RESOURCE_GROUP%%/${Imdb_RG}/g" AKS_Dashboard.json

```

Navigate to ([Dashboard](https://portal.azure.com/#dashboard)) within your Azure portal. Click upload and select the `AKS_Dashboard.json` file with your correct subscription GUID, resource group names, and app name.

For more documentation on creating and sharing Dashboards, see ([here](https://docs.microsoft.com/en-us/azure/azure-portal/azure-portal-dashboards)).

## Optional

A testing application was written to stress test the application and drive the Request Units on the Cosmos DB. You can deploy the application to AKS as a cronjob. The cronjobs can be deployed to your cluster via a helm chart located at `REPO_ROOT/docs/aks/cluster/charts`.

```bash

cd $REPO_ROOT/docs/aks/cluster/charts

helm install helium-smoker smoker --set ingressURL=http://<IP_OF_INGRESS>.nip.io

#Verify the CronJobs are in the cluster
kubectl get cronjobs

```

The cronjobs are set to run for 7.5 minutes every 20 minutes.

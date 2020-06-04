#!/bin/bash
az provider register -n Microsoft.KeyVault
az account set -s Reinsel-sandbox
export He_Sub=$(az account show --subscription Reinsel-sandbox --output tsv |awk '{print $3}')
export He_Name=wsrdash2
az cosmosdb check-name-exists -n ${He_Name}
nslookup ${He_Name}.azurewebsites.net
nslookup ${He_Name}.vault.azure.net
nslookup ${He_Name}.azurecr.ionslookup ${He_Name}.azurewebsites.net
export He_Location=centralus
export He_ACR_RG=${He_Name}-rg-acr
export He_App_RG=${He_Name}-rg-app
az group create -n $He_App_RG -l $He_Location
az group create -n $He_ACR_RG -l $He_Location
export Imdb_Name="imdbcosmoswsrdash2"
az cosmosdb check-name-exists -n ${Imdb_Name}
export Imdb_RG=${Imdb_Name}-cosmos-rg
az group create -n $Imdb_RG -l $Imdb_Location
az cosmosdb create -g $Imdb_RG -n $Imdb_Name
export Imdb_Key=$(az cosmosdb keys list -n $Imdb_Name -g $Imdb_RG --query primaryMasterKey -o tsv)
az cosmosdb sql database create -a $Imdb_Name -n $Imdb_DB -g $Imdb_RG --throughput 400
az cosmosdb sql container create -p /partitionKey -g $Imdb_RG -a $Imdb_Name -d $Imdb_DB -n $Imdb_Col
docker run -it --rm retaildevcrew/imdb-import $Imdb_Name $Imdb_Key $Imdb_DB $Imdb_Col
export Imdb_Cosmos_RO_Key=$(az cosmosdb keys list -n $Imdb_Name -g $Imdb_RG --query primaryMasterKey -o tsv)
az keyvault create -g $He_App_RG -n $He_Name
az keyvault secret set -o table --vault-name $He_Name --name "CosmosUrl" --value https://${Imdb_Name}.documents.azure.com:443/
az keyvault secret set -o table --vault-name $He_Name --name "CosmosKey" --value $Imdb_Cosmos_RO_Key
az keyvault secret set -o table --vault-name $He_Name --name "CosmosDatabase" --value $Imdb_DB
az keyvault secret set -o table --vault-name $He_Name --name "CosmosCollection" --value $Imdb_Col
export dev_Object_Id=$(az ad user show --id wereinse@microsoft.com --query objectId -o tsv)
az keyvault set-policy -n $He_Name --secret-permissions get list --key-permissions get list --object-id $dev_Object_Id
export He_Language=csharp
az acr create --sku Standard --admin-enabled false -g $He_ACR_RG -n $He_Name
az acr login -n $He_Name
az acr build -r $He_Name -t $He_Name.azurecr.io/helium-${He_Language} https://github.com/retaildevcrews/helium-${He_Language}.git
export He_SP_PWD=$(az ad sp create-for-rbac -n http://${He_Name}-acr-sp --query password -o tsv)
export He_SP_ID=$(az ad sp show --id http://${He_Name}-acr-sp --query appId -o tsv)
export He_ACR_Id=$(az acr show -n $He_Name -g $He_ACR_RG --query "id" -o tsv)
az role assignment create --assignee $He_SP_ID --scope $He_ACR_Id --role acrpull
az keyvault secret set -o table --vault-name $He_Name --name "AcrUserId" --value $He_SP_ID

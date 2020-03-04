# Deploy Helium container to Azure App Service

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
az acr webhook create -r $He_Name -n ${He_Name} --actions push --uri $He_CICD_URL --scope helium-${He_Language}:latest

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
-i ${He_Name}.azurecr.io/helium-${He_Language} \
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

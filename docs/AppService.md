# Deploy Helium container to Azure App Service

Create and configure App Service (Web App for Containers)

- App Service will fail to start until configured properly

```bash

# create App Service plan
az appservice plan create --sku B1 --is-linux -g $He_App_RG -n ${He_Name}-plan

### App Service cannot currently use Managed Identity to access ACR
### We pull the Service Principal ID and Key from Key Vault via
### the @Microsoft.KeyVault() format used in -u and -p below

# stop the Web App
az webapp stop -g $He_App_RG -n $He_Name



# create Web App for Containers
az webapp create --deployment-container-image-name hello-world -g $He_App_RG -p ${He_Name}-plan -n $He_Name





### TODO - delete this
### this works consistently once the acrpull role is successfully granted
### it appears that doesn't happen consistently
### I'm testing that now

### You can iterate creating multiple web apps without having to go through everything
### by using these temp steps

export t_name=bartr9d

# create Web App for Containers
az webapp create --deployment-container-image-name hello-world -g $He_App_RG -p ${He_Name}-plan \
-n $t_name

az webapp stop -g $He_App_RG -n $t_name


# configure the Web App to use Container Registry
az webapp config container set -g $He_App_RG \
-n $t_name \
-i ${He_Name}.azurecr.io/${He_Repo} \
-r https://${He_Name}.azurecr.io \
-u "@Microsoft.KeyVault(SecretUri=${He_AcrUserId})" \
-p "@Microsoft.KeyVault(SecretUri=${He_AcrPassword})"


az webapp start -g $He_App_RG -n $t_name

http https://${t_name}.azurewebsites.net/version






# assign Managed Identity
export He_MSI_ID=$(az webapp identity assign -g $He_App_RG -n $He_Name --query principalId -o tsv)

# grant Key Vault access to Managed Identity
az keyvault set-policy -n $He_Name --secret-permissions get list --key-permissions get list --object-id $He_MSI_ID

# turn on CI
export He_CICD_URL=$(az webapp deployment container config -n $He_Name -g $He_App_RG --enable-cd true --query CI_CD_URL -o tsv)

# add the webhook
az acr webhook create -r $He_Name -n ${He_Name} --actions push --uri $He_CICD_URL --scope ${He_Repo}:latest

# set the Key Vault name app setting (environment variable)
az webapp config appsettings set --settings KEYVAULT_NAME=$He_Name -g $He_App_RG -n $He_Name

# turn on container logging
# this will send stdout and stderr to the logs
az webapp log config --docker-container-logging filesystem -g $He_App_RG -n $He_Name

# save environment variables
./saveenv.sh -y

# assign acrpull access to Service Principal
az role assignment create --assignee $(eval $He_SP_ID) --scope $He_ACR_Id --role acrpull

# configure the Web App to use Container Registry
az webapp config container set -n $He_Name -g $He_App_RG \
-i ${He_Name}.azurecr.io/${He_Repo} \
-r https://${He_Name}.azurecr.io \
-u "@Microsoft.KeyVault(SecretUri=${He_AcrUserId})" \
-p "@Microsoft.KeyVault(SecretUri=${He_AcrPassword})"


# start the Web App
az webapp stop -g $He_App_RG -n $He_Name
az webapp start -g $He_App_RG -n $He_Name

# check the version endpoint
# this will eventually work, but may take a minute
# you may get a 403 or timeout error, if so, just run again

sleep 30
http https://${He_Name}.azurewebsites.net/version

```

Run the Validation Test

> For more information on the validation test tool, see [Web Validate](https://github.com/retaildevcrews/webvalidate)

```bash

# run the tests in the container
docker run -it --rm retaildevcrew/webvalidate --server https://${He_Name}.azurewebsites.net --files helium.json

```

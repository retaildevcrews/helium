# Deploy Helium container to Azure App Service

Create and configure App Service (Web App for Containers)

- App Service will fail to start until configured properly

```bash

# create App Service plan
az appservice plan create --sku B1 --is-linux -g $He_App_RG -n ${He_Name}-plan

# create Web App for Containers
az webapp create -g $He_App_RG -n $He_Name -p ${He_Name}-plan \
--deployment-container-image-name hello-world \
--docker-registry-server-user "@Microsoft.KeyVault(SecretUri=$(az keyvault secret show --vault-name $He_Name --name AcrUserId --query id -o tsv))" \
--docker-registry-server-password "@Microsoft.KeyVault(SecretUri=$(az keyvault secret show --vault-name $He_Name --name AcrPassword --query id -o tsv)"

# stop the Web App
az webapp stop -g $He_App_RG -n $He_Name

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

### App Service cannot currently use Managed Identity to access ACR
### We pull the Service Principal ID and Key from Key Vault via
### the @Microsoft.KeyVault() format used in -u and -p below

# configure the Web App to use Container Registry
az webapp config container set -n $He_Name -g $He_App_RG \
-i ${He_Name}.azurecr.io/${He_Repo}:latest \
-r https://${He_Name}.azurecr.io \
-u "@Microsoft.KeyVault(SecretUri=$(az keyvault secret show --vault-name $He_Name --name AcrUserId --query id -o tsv))" \
-p "@Microsoft.KeyVault(SecretUri=$(az keyvault secret show --vault-name $He_Name --name AcrPassword --query id -o tsv))"

# assign acrpull access to Service Principal
az role assignment create --assignee $(eval $He_SP_ID) --scope $He_ACR_Id --role acrpull

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

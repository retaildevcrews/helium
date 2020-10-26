# Deploy Helium to Azure App Service

Create and configure App Service (Web App for Containers)

```bash

# create App Service plan
az appservice plan create --sku S1 --is-linux -g $He_App_RG -n ${He_Name}-plan

# create Web App for Containers
# temporarily use the nginx image
az webapp create --deployment-container-image-name nginx -g $He_App_RG -p ${He_Name}-plan -n $He_Name

# stop the Web App
az webapp stop -g $He_App_RG -n $He_Name

# assign Managed Identity
export He_MI_ID=$(az webapp identity assign -g $He_App_RG -n $He_Name --query principalId -o tsv)

# grant Key Vault access to Managed Identity
az keyvault set-policy -n $He_Name --secret-permissions get list --key-permissions get list --object-id $He_MI_ID

# turn on CI
export He_CICD_URL=$(az webapp deployment container config -n $He_Name -g $He_App_RG --enable-cd true --query CI_CD_URL -o tsv)

# add the webhook
az acr webhook create -r $He_Name -n ${He_Name} --actions push --uri $He_CICD_URL --scope ${He_Repo}:latest

# set the Key Vault config setting
az webapp config appsettings set --settings KEYVAULT_NAME=$He_Name -g $He_App_RG -n $He_Name

# turn on container logging
# this will send stdout and stderr to the logs
az webapp log config --docker-container-logging filesystem -g $He_App_RG -n $He_Name

# save environment variables
./saveenv.sh -y

### App Service cannot currently use Managed Identity to access ACR
### We pull the previously created Service Principal ID and Key from Key Vault via
### the @Microsoft.KeyVault() format used in -u and -p below

# configure the Web App to use Container Registry
az webapp config container set -n $He_Name -g $He_App_RG \
-i ${He_Name}.azurecr.io/${He_Repo} \
-r https://${He_Name}.azurecr.io \
-u "@Microsoft.KeyVault(SecretUri=${He_AcrUserId})" \
-p "@Microsoft.KeyVault(SecretUri=${He_AcrPassword})"

# start the Web App
az webapp start -g $He_App_RG -n $He_Name

# check the version endpoint
# you may get a 403 or timeout error, if so, just retry

http https://${He_Name}.azurewebsites.net/version

```

Run the Validation Test

> For more information on the validation test tool, see [Web Validate](https://github.com/retaildevcrews/webvalidate)

```bash

# run the tests in a container
docker run -it --rm retaildevcrew/webvalidate --server https://${He_Name}.azurewebsites.net --base-url https://raw.githubusercontent.com/retaildevcrews/helium/main/TestFiles/ --files baseline.json

```

## Observability

See [App Observability](AppServiceObservability.md) for details

# Deploy Helium to Azure App Service

Create and configure App Service on Windows using dotnetcore 3.1

```bash

# create App Service plan
az appservice plan create --sku S1 -g $He_App_RG -n ${He_Name}-plan

# create Web App
az webapp create --runtime "dotnetcore|3.1" -g $He_App_RG -p ${He_Name}-plan -n $He_Name

# stop the Web App
az webapp stop -g $He_App_RG -n $He_Name

# assign Managed Identity
export He_MI_ID=$(az webapp identity assign -g $He_App_RG -n $He_Name --query principalId -o tsv)

# grant Key Vault access to Managed Identity
az keyvault set-policy -n $He_Name --secret-permissions get list --key-permissions get list --object-id $He_MI_ID

# set the Key Vault config setting
az webapp config appsettings set --settings KEYVAULT_NAME=$He_Name -g $He_App_RG -n $He_Name

# delete the deployment source if it exists
az webapp deployment source delete -g $He_App_RG -n $He_Name

# Note that you will have to clone the repo to an organization where you have permissions
# to create a webhook or use the --manual-integration flag
# Replace the -u and --branch parameters with your repo values

# set deployment
# this step usually takes 3-5 minutes
az webapp deployment source config -g $He_App_RG -n $He_Name -u https://github.com/retaildevcrews/helium-csharp --branch main --manual-integration

# set the app endpoint
export He_App_Endpoint=https://${He_Name}.azurewebsites.net

# save environment variables
./saveenv.sh -y

# start the Web App
az webapp start -g $He_App_RG -n $He_Name

# check the version endpoint
# you may get a 403 or timeout error, if so, just retry

http ${He_App_Endpoint}/version


```

Run the Validation Test

> For more information on the validation test tool, see [Web Validate](https://github.com/retaildevcrews/webvalidate)

```bash

# run the tests in a container
docker run -it --rm retaildevcrew/webvalidate --server $He_App_Endpoint --base-url https://raw.githubusercontent.com/retaildevcrews/helium/main/TestFiles/ --files baseline.json

```

## Observability

See [App Observability](AppObservability.md) for details

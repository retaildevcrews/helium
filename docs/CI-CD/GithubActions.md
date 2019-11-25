# Setup CI-CD with Github Actions

Setup Github Actions to build and push the helium container to Azure Container Registry (ACR).

## Create Service Principal for ACR Push

First, create a Service Principal and grant it ACR push access using the Azure CLI. 

```bash

# source environment variables from before if needed
# based off of $He_Name value
source ~/{youruniquename}.env

# create a Service Principal
export He_SP_PWD_CICD=$(az ad sp create-for-rbac -n http://${He_Name}-acr-sp-cicd --query password -o tsv)
export He_SP_ID_CICD=$(az ad sp show --id http://${He_Name}-acr-sp-cicd --query appId -o tsv)

# get the Container Registry Id
export He_ACR_Id=$(az acr show -n $He_Name -g $He_ACR_RG --query "id" -o tsv)

# assign acrpush access to Service Principal
az role assignment create --assignee $He_SP_ID_CICD --scope $He_ACR_Id --role acrpush

# show SP values needed to set up Github Action
echo $He_SP_ID_CICD
echo $He_SP_PWD_CICD

```

## Add Service Principal Credentials to Github Secrets

In the browser, navigate to your forked repo page in Github to set up credentials with the newly created service principal.

1. Go to Settings -> Secrets
2. Select "Add a new secret" 
3. Add the secret:
    - Name: REGISTRY_USERNAME
    - Value: {copy output from `echo $He_SP_ID_CICD` command} 
4. Add the secret:
    - Name: REGISTRY_PASSWORD
    - Value: {copy output from `echo $He_SP_PWD_CICD` command}

![alt text](../images/githubactions-cicd.jpg "Add Github Secret")

## Add Github Workflow File (.yml) 

1. Create a new branch in your forked repo off of master. 
2. Copy the [cicdWorkflow.yml](./cicdWorkflow.yml) from this folder to your forked repo in the .github/workflows directory. If the .github folder does not already exist at the root, create one. 
3. Update the yml file in your forked repo with the name of your ACR. The file defaults to "helium", so update with $He_Name from previous readme steps.
    - The workflow is designed to push the same image twice to ACR, one with the latest tag and one tagged with the last commit SHA.  Update the tagging strategy as needed. 
    - It is also designed to be triggered on a push to the master branch, see [documentation](https://help.github.com/en/actions/automating-your-workflow-with-github-actions/workflow-syntax-for-github-actions#onpushpull_requestbranchestags) for more options. 
4. Merge changes into master through a pull request.  

## Verify Successful Workflow

1. After merging changes into master, navigate to the Actions tab in your repo.
2. You should see an instance of the newly added workflow (default name is CI-ACR) running or succesfully completed.

![alt text](../images/githubactions-run.jpg "Successful Workflow Run")
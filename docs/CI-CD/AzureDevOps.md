# Setup CI-CD with Azure DevOps

- The [pipeline file](azure-pipelines.yml) contains the build definition for this sample
- You will need to setup a "Container Registry Service Connection" in Azure DevOps before importing the build pipeline
- The pipeline defines "helium" as the name of the service connection
- You can change this to an existing service connection or create a new service connection called helium
- If you use a different name, make sure to update the pipeline

## Creating a new Azure DevOps project

- Open Azure DevOps
- Click on New Project
- Enter the project information
- Click on Create

## Adding a Service Connection

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

## Adding a pipeline

- Click on Pipelines
- Click on Create your first Pipeline
- Select the repo that your code was forked to
- Click run

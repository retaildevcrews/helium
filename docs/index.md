# Helium Documentation

![License](https://img.shields.io/badge/license-MIT-green.svg)

## Build a Web API reference application using Managed Identity, Key Vault, and Cosmos DB that is designed to be deployed to Azure App Service or Azure Kubernetes Service (AKS)

This is a Web API reference application designed to "fork and code" with the following features:

- Securely build, deploy and run an Azure App Service (Web App for Containers) application
- Securely build, deploy and run an Azure Kubernetes Service (AKS) application
- Use Managed Identity to securely access resources
- Securely store secrets in Key Vault
- Securely build and deploy the Docker container from Azure Container Registry (ACR) or Azure DevOps
- Connect to and query Cosmos DB
- Automatically send telemetry and logs to Azure Monitor

![alt text](images/architecture.jpg "Architecture Diagram")

## Documentation

### [AKS Setup](aks/README.md)

### [Alerts Setup](AlertSetup.md)

### [App Service Setup](AppService.md)

### [Engineering Practices](EngineeringPractices.md)

### [User Stories](UserStories.md)

## Language Specific Documentation

### [dotnet Implementation](DotNet-Core-Developer.md)

### [Java Implementation](Java-Spring-WebFlux-Developer.md)

## CI-CD Documentation

### [ACR Tasks](CI-CD/ACR.md)

### [GitHub Actions](CI-CD/GitHubActions.md)

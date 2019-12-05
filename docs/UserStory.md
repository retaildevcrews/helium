# Helium User Story 

## EPIC
*As a web application developer, I want a reference web application that is secure by default while leveraging engineering best practices so that I can easily fork and code and deploy.*

## USER STORY
*As a web application developer, I want a secure reference application, so that I can securely store, access, and maintain secrets.*


### Acceptance Criteria:
- Securely build, deploy and run an Azure App Service (Web App for Containers) application
- Securely build, deploy and run an Azure Kubernetes Service (AKS) application
- Use Managed Identity to securely access resources
- Securely store, access, and maintain secrets in Key Vault
- Securely build and deploy the Docker container from Azure Container Registry (ACR) or Azure DevOps
- Connect to and query Cosmos DB
- Automatically send telemetry and logs to Azure Monitor
- Multi developer languages (C#, Node.js, and Java Spring Boot)

Also, here are a few points that need to be addressed:
1.	Key Rotation – Research restrictions, existing solutions, etc.
2.	MI support for Web App Containers
3.	Research secure ACR access
4.	Java SDK capability

Key Features:
* Security
    * Managed Identity
    * Pod Identity
    * Key Vault
    * Key Rotation
    * Developer Experience
- Cloud Native Platform (or containerization)
    * ACR
    * App Services
    * AKS
- DevOps
    * IaC
    * CI-CD
    * Smoke Testing
- 	CosmosDB
    * Document Modeling
    * RU optimization
    * Key Rotation
•	Observability
    * Logging
    * Monitoring
    * Alerting
- Developer Experience
    * Secure by Design
    * Fork and Code
    * Local dev or container dev

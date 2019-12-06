# Helium User Story

## User Stories

- As a web application developer, I want a reference web application that is secure by default while leveraging engineering best practices so that I can easily fork and code and deploy.

### Acceptance Criteria

- Provide a step-by-step quick start that delivers a positive developer experience
- Securely build, deploy and run an Azure App Service (Web App for Containers) application
- Securely build, deploy and run an Azure Kubernetes Service (AKS) application
- Use Managed Identity to securely access resources
- Securely store, access, and maintain secrets in Key Vault
- Securely build and deploy the Docker container from Azure Container Registry (ACR), Azure DevOps, GitHub Actions or manually
- Securely connect to and query Cosmos DB
- Automatically send telemetry and logs to Azure Monitor
- Multi developer languages (C#, Node.js, and Java Spring Boot)

Also, here are a few points that need to be addressed:

1. Key Rotation – Research restrictions, existing solutions, etc.
2. Managed Identity support for Web App Containers
3. Managed Identity suport for AKS
4. Research secure ACR access from Azure App Services and AKS
5. Java SDK capability

### Key Features

- Security
  - "Secure by Design"
  - Managed Identity
    - Pod Identity
  - Key Vault
  - Key Rotation
  - Developer Experience

- Cloud Native Platform (containerization)
  - ACR
  - App Services
  - AKS
  - DevOps
  - IaC
  - CI-CD
  - End-to-end Testing

- CosmosDB
  - Document Modeling
  - RU optimization
  - Key Rotation

- Observability
  - Logging
  - Monitoring
  - Alerting

- Developer Experience
  - Secure by Design
  - Fork and Code
  - Local dev or container dev

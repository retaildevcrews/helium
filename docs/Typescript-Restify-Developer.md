# Typescript/Restify Developer Documentation

## Index

1. [Async/Await](#asyncawait)
2. [Restify vs. Express](#restify-vs-express)
3. [Dependency Injection (DI)](#dependency-injection-di)
4. [Managed Identity and Key Vault](#managed-identity-and-key-vault)
    - [Dev Flag](#dev-flag)
5. [Cosmos DB](#cosmos-db)
    - [SQL Parameterization](#sql-parameterization)
    - [Partition Key Function](#partition-key-function)
6. [Healthz Response Cache](#healthz-response-cache)
7. [AKS Pod Identity Support](#aks-pod-identity-support)
8. [Versioning](#versioning)
9. [Middleware](#middleware)
    - [Robots](#robots)
    - [Endpoint Logger](#endpoint-logger)
10. [Logging](#logging)

## Async/Await

Helium uses the typescript async pattern to simplify the code handling Promises. Find a deep dive into the explanation and various patterns available in this [discussion post](https://github.com/orgs/retaildevcrews/teams/helium/discussions/8).

## Restify vs. Express

Restify is written specifically for maintainable and observable REST API's, whereas Express includes a larger set of features for full-fledged web applications. Because Helium does not have the goal of expanding to leverage these features, we decided to use Restify. If there is interest in an Express version, please submit an issue for us to triage in our backlog.

## Dependency Injection (DI)

Helium uses inversify to implement DI. To do this, we create an inversify Container and map, or bind, interfaces with concrete classes. Note that some classes are bound in Singleton Scope, this allows for the same instance of the class to be injected, which we want for our LogService, NodeCache, DataService, and TelemetryService.

[server.ts](https://github.com/retaildevcrews/helium-typescript/blob/master/src/server.ts)

```typescript

    const container: Container = new Container();

    // setup logService (we need it for configuration)
    container.bind<LogService>("LogService").to(BunyanLogService).inSingletonScope();
    const logService = container.get<LogService>("LogService");

    // parse command line arguments to get the Key Vault url and auth type
    const consoleController = new ConsoleController(logService);
    const config = await consoleController.run();
    const healthzCache = new NodeCache();

    // setup ioc container
    container.bind<NodeCache>("NodeCache").toConstantValue(healthzCache);
    container.bind<ConfigValues>("ConfigValues").toConstantValue(config);
    container.bind<interfaces.Controller>(TYPE.Controller).to(ActorController).whenTargetNamed("ActorController");
    container.bind<interfaces.Controller>(TYPE.Controller).to(FeaturedController).whenTargetNamed("FeaturedController");
    container.bind<interfaces.Controller>(TYPE.Controller).to(GenreController).whenTargetNamed("GenreController");
    container.bind<interfaces.Controller>(TYPE.Controller).to(MovieController).whenTargetNamed("MovieController");
    container.bind<interfaces.Controller>(TYPE.Controller).to(HealthzController).whenTargetNamed("HealthzController");
    container.bind<DataService>("DataService").to(CosmosDBService).inSingletonScope();
    container.bind<TelemetryService>("TelemetryService").to(AppInsightsService).inSingletonScope();

```

To enable this behavior, each class must be decorated with the @injectable attribute. See the CosmosDBService example below.

```typescript

@injectable()
export class CosmosDBService implements DataService {

```

To use the injected object, use the get method. Continuing the Cosmos DB example, below shows the container resolving the DataService dependency when attempting to set up the Cosmos DB connection.

[server.ts](https://github.com/retaildevcrews/helium-typescript/blob/master/src/server.ts#L42)

```typescript

    // connect to cosmos db
    let cosmosDbService: DataService;
    try {
        cosmosDbService = container.get<DataService>("DataService");
        await cosmosDbService.connect();
    }

```

## Managed Identity and Key Vault

After creating a Managed Identity for the Helium web app and assigning get and list secret permissions to Key Vault, the following code successfully authenticates using Managed Identity to create the Key Vault Client. Leveraging Managed Identity in this way eliminates the need to store any credential information in app code.  For the local development scenario, we use a different credential specifically for Azure CLI credentials.  This works as long as the developer has access to the Key Vault and is logged in to the Azure CLI with az login. The authentication type can be specified as an environment variable or command line argument, defaulting to MSI.

Currently, we use a different package for MSI credentials and CLI credentials, this will be consolidated with the next release of @azure/identity when they plan to add CLI credentials as an option *without* the built in default to fall back on environment variable credentials.

[KeyVaultService.ts](https://github.com/RetailDevCrews/helium-typescript/blob/master/src/services/KeyVaultService.ts#L36)

```typescript

// use specified authentication type (either MSI or CLI)
const creds: any = this.authType === "MSI" ?
    new azureIdentity.ManagedIdentityCredential() :
    await msRestNodeAuth.AzureCliCredentials.create({ resource: "https://vault.azure.net" });

this.client = new SecretClient(this.url, creds);

// test getSecret to validate successful Key Vault connection
await this.getSecret(cosmosUrl);

```

### Dev Flag

To enforce MSI in production, the --dev flag is required as a command line argument in order to use CLI credentials to authenticate. There is no environment variable option for this arg, as there is with --keyvault-name (KEYVAULT_NAME), for instance. Ideally, conditionaly compilation would have been used as with the C# version. However, typescript does not currently support this feature.

```bash

npm start -- --keyvault-name $He_Name --auth-type CLI --dev

```

## Cosmos DB

### SQL Parameterization

Helium leverages the parameterized query for Cosmos DB queries instead of simply building out the sql query string directly from the provided query parameters from the request.

[CosmosDBService.ts](https://github.com/retaildevcrews/helium-typescript/blob/master/src/services/CosmosDBService.ts#L46)

```typescript

 public async queryActors(queryParams: any): Promise<Actor[]> {
        const SELECT = "select m.id, m.partitionKey, m.actorId, m.type, m.name, m.birthYear, m.deathYear, m.profession, m.textSearch, m.movies from m where m.type = 'Actor' ";
        const ORDER_BY = " order by m.textSearch, m.actorId";
        const parameters = [];

        let sql = SELECT;

        let actorName: string = queryParams.q;

        const { size: pageSize, number: pageNumber } = this.setPagingParameters(queryParams.pageSize, queryParams.pageNumber);

        const offsetLimit = " offset " + (pageNumber * pageSize) + " limit " + pageSize + " ";

        // apply search term if provided in query
        if (actorName) {
            actorName = actorName.trim().toLowerCase().replace("'", "''");

            if (actorName) {
                sql += " and contains(m.textSearch, @actorName)";
                parameters.push({name: "@actorName", value: actorName});
            }
        }

        sql += ORDER_BY + offsetLimit;

        return await this.queryDocuments({ query: sql, parameters: parameters });
    }

```

### Partition Key Function

In order to directly read a document using 1 RU (assuming the document is 1K or less), you need the document's ID and partition key. A good CosmosDB best practice is to compute the partition key from the ID. In our case, we use the integer portion of the Movie or Actor document mod 10. This gives us 10 partitions ("0" - "9") with good distribution. For a deeper discussion on the document modeling decisions, please read this [document](https://github.com/retaildevcrews/imdb).

[queryUtilities.ts](https://github.com/retaildevcrews/helium-typescript/blob/master/src/utilities/queryUtilities.ts)

```typescript

// compute the partition key based on the movieId or actorId
// for this sample, the partition key is mod 10 of the numeric portion of the id
// returns "0" by default
public static getPartitionKey(id: string): string {
    let idInt = 0;

    if ( id.length > 5 && (id.startsWith("tt") || id.startsWith("nm"))) {
        idInt = parseInt(id.substring(2), 10);
        return isNaN(idInt) ? "0" : (idInt % 10).toString();
    }

    return idInt.toString();
}

```

## Healthz Response Cache

Helium's health check generates 5 separate queries and is an expensive request. To help prevent a DoS attack on the /healthz and /healthz/ietf endpoints which run the health check, the response is cached for 60 seconds using node-cache. See [Dependency Injection (DI)](#dependency-injection-di) for the creation of the cache. See the code below for its usage in the /healthz/ietf endpoint.

[HealthzController](https://github.com/retaildevcrews/helium-typescript/blob/master/src/controllers/HealthzController.ts#L54)

```typescript

const cachedValue = this.cache.get("healthz");
if (cachedValue != undefined) {
    healthCheckResult = cachedValue;
} else {
    healthCheckResult = await this.runHealthChecks();
    this.cache.set("healthz", healthCheckResult, 60);
}

```

## AKS Pod Identity Support

AKS Pod Identity is currently in preview and allows applications running within containers on AKS to use Managed Identities and Service Principals to access Azure Services such as Key Vault. Azure App Service also supports Managed Identity for containers.

The first time an AKS pod uses a Managed Identity, it has to start a new proxy. In testing, this usually takes about 30 seconds. If the Managed Identity proxy is not available, Helium will fail to start (by design) and throw an error. While AKS will attempt to automatically restart the pod, it's a known error that we want to manage. A simple retry around the Key Vault client causes the pod start to wait for the Managed Identity proxy to be ready.

Note that once the MI proxy is running, responses are generally under 100ms, so the retry code is not used in that case. The retry code is also not used in the App Service scenario as App Service ensures the proxy is running before starting Helium.

[KeyVaultService.ts](https://github.com/retaildevcrews/helium-typescript/blob/master/src/services/KeyVaultService.ts#L27)

```typescript

// connect to the Key Vault client
// AKS can take longer to spin up pod identity for the first pod, so
//      we retry for up to 90 seconds
public async connect() {
    // retry managed identity for 90 seconds
    const MAX_RETRIES = 90;

    let retries = 0;
    while (retries < MAX_RETRIES){
        try {
            // use specified authentication type (either MSI or CLI)
            const creds: any = this.authType === "MSI" ?
                new azureIdentity.ManagedIdentityCredential() :
                await msRestNodeAuth.AzureCliCredentials.create({ resource: "https://vault.azure.net" });

            this.client = new SecretClient(this.url, creds);

            // test getSecret to validate successful Key Vault connection
            await this.getSecret(cosmosUrl);
            return;
        } catch (e) {
            retries++;
            if (this.authType === "MSI" && retries < MAX_RETRIES) {
                this.logger.info("Key Vault: Retry");
                // wait 1 second and retry (continue while loop)
                await new Promise(resolve => setTimeout(resolve, 1000));
            } else {
                throw new Error("Failed to connect to Key Vault with MSI");
            }
        }
    }
}

```

## Versioning

Helium dynamically builds a version string based on the package version and date time of build. This is output to the console logs at app startup and displayed in the both the /version and healthz/ietf endpoint responses. The addition of date time to the version string in combination with the /version endpoint allows for quick verification that the latest build has been deployed with CI/CD.

[versionUtilities.ts](https://github.com/retaildevcrews/helium-typescript/blob/master/src/utilities/versionUtilities.ts)

```typescript

// build and return the version string based on last build date time
// build time based on dist/server.js file
public static getBuildVersion(): string {

    // get the build time (i.e. 2020-04-02T05:11:04.483Z) and pull out the interesting parts
    const buildTime = fs.statSync("./dist/server.js").mtime.toISOString();
    const [, month, day, hour, minute] = /\d*-(\d*)-(\d*)T(\d*):(\d*):.*Z/.exec(buildTime);

    return `${pkg.version}+${month}${day}.${hour}${minute}`;
}

```

## Middleware

### Robots

There is a robotsText middleware extension method added to Helium to handle a default warmup request of /robots43245.txt (43245 is random) when deploying to Azure App Service. Because Helium does not expect this request as part of its normal app logic, it would cause a 404 error, or in this case a false (expected) error, to appear in Azure Monitor reporting. This extension helps keep reporting clean and only contain true errors warranting investigation.

Code: [robotsText.ts](https://github.com/retaildevcrews/helium-typescript/blob/master/src/middleware/robotsText.ts)

### Endpoint Logger

A custom Request Logger extension is added to handle logging Http request information. As implemented, only 4xx and 5xx responses are logged. This helps make logs easy to search through when debugging errors, rather than having to navigate through several successful requests. In addition to the request logger, helium logs primarily errors to console to keep output clean.

Code: [EndpointLogger.ts](https://github.com/retaildevcrews/helium-typescript/blob/master/src/middleware/EndpointLogger.ts)

## Logging

Helium uses the Bunyan log service. Bunyan supports 6 logging levels, Helium currently implements 4: trace (10), info (30), warn (40), and error (50). The stream logging level definition determines which logs are output to the stream. For instance, if the stream logging level is set to warn, only warn logs and higher (error) will be logged. The default stream logging level is info (30), this can be changed via command line (--log-level argument) or environment variable (LOG_LEVEL).

Code: [BunyanLogService.ts](https://github.com/retaildevcrews/helium-typescript/blob/master/src/services/BunyanLogService.ts)

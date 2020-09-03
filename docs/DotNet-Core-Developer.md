# C# dotnet core Developer Documentation

## Index

1. [Managed Identity and Key Vault](#managed-identity-and-key-vault)
2. [Cosmos DB](#cosmos-db)
    - [Partition Key Function](#partition-key-function)
3. [AKS Pod Identity Support](#aks-pod-identity-support)
4. [Versioning](#versioning)
5. [Dependency Injection (DI)](#dependency-injection-(di))
    - [Key Vault](#key-vault)
    - [Data Access Layer (DAL)](#data-access-layer-(DAL))
    - [Application Insights](#application-insights)
6. [Middleware](#middleware)
7. [Logging](#logging)

## Managed Identity and Key Vault

After creating a Managed Identity for the Helium web app and assigning get and list secret permissions to Key Vault, the following code successfully authenticates using Managed Identity to create the Key Vault Client. Leveraging Managed Identity in this way eliminates the need to store any credential information in app code. For the local development scenario, we use a different credential specifically for Azure CLI credentials.  This works as long as the developer has access to the Key Vault and is logged in to the Azure CLI with `az login`. The authentication type can be specified as an environment variable or command line argument, defaulting to MI.

[Program.cs](https://github.com/RetailDevCrews/helium-csharp/blob/master/src/app/Program.cs#L245)

```c#

// use Managed Identity (MI) for secure access to Key Vault
var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(new AzureServiceTokenProvider().KeyVaultTokenCallback));

// read a key to make sure the connection is valid
await keyVaultClient.GetSecretAsync(kvUrl, Constants.CosmosUrl);

// return the client
return keyVaultClient;

```

## Cosmos DB

### Partition Key Function

In order to directly read a document using 1 RU (assuming the document is 1K or less), you need the document's ID and partition key. A good CosmosDB best practice is to compute the partition key from the ID. In our case, we use the integer portion of the Movie or Actor document mod 10. This gives us 10 partitions ("0" - "9") with good distribution. For a deeper discussion on the document modeling decisions, please read this [document](https://github.com/retaildevcrews/imdb).

While this function calculates the key similarly for both the Actor and Movie IDs, the method is added for both the Actor and Movie model classes. This allows for the method to calculate the partition key differently based on the entity.

[Actor.cs](https://github.com/retaildevcrews/helium-csharp/blob/main/src/app/Model/Actor.cs#L29)
[Movie.cs](https://github.com/retaildevcrews/helium-csharp/blob/main/src/app/Model/Movie.cs#L32)

```c#

public static string ComputePartitionKey(string id)
{
    // validate id
    if (!string.IsNullOrEmpty(id) &&
        id.Length > 5 &&
        (id.StartsWith("tt", StringComparison.OrdinalIgnoreCase)) &&
        int.TryParse(id.Substring(2), out int idInt))
    {
        return (idInt % 10).ToString(CultureInfo.InvariantCulture);
    }

    throw new ArgumentException("Invalid Partition Key");
}

```

The above code shows the `ComputePartitionKey` method for the Movie class which is almost identical to the one in the Actor class.

## AKS Pod Identity Support

AKS Pod Identity is currently in preview and allows applications running within containers on AKS to use Managed Identities and Service Principals to access Azure Services such as Key Vault. Azure App Service also supports Managed Identity for containers.

The first time an AKS pod uses a Managed Identity, it has to start a new proxy. In testing, this usually takes about 30 seconds. If the Managed Identity proxy is not available, Helium will fail to start (by design) and throw an error. While AKS will attempt to automatically restart the pod, it's a known error that we want to manage. A simple retry around the Key Vault client causes the pod start to wait for the Managed Identity proxy to be ready.

Note that once the MI proxy is running, responses are generally under 100ms, so the retry code is not used in that case. The retry code is also not used in the App Service scenario as App Service ensures the proxy is running before starting Helium.

[Program.cs](https://github.com/RetailDevCrews/helium-csharp/blob/master/src/app/Program.cs#L400)

```c#

/// <summary>
/// Get a valid key vault client
/// AKS takes time to spin up the first pod identity, so
///   we retry for up to 90 seconds
/// </summary>
/// <param name="kvUrl">URL of the key vault</param>
/// <param name="authType">MI, CLI or VS</param>
/// <returns></returns>
static async Task<KeyVaultClient> GetKeyVaultClient(string kvUrl, string authType)
{
    // retry Managed Identity for 90 seconds
    //   AKS has to spin up an MI pod which can take a while the first time on the pod
    DateTime timeout = DateTime.Now.AddSeconds(90.0);

    // use MI as default
    string authString;

    switch (authType.ToUpperInvariant())
    {
        case "MI":
            authString = "RunAs=App";
            break;
        case "CLI":
            authString = "RunAs=Developer; DeveloperTool=AzureCli";
            break;
        case "VS":
            authString = "RunAs=Developer; DeveloperTool=VisualStudio";
            break;
        default:
            Console.WriteLine("Invalid Key Vault Authentication Type");
            return null;
    }

    while (true)
    {
        try
        {
            var tokenProvider = new AzureServiceTokenProvider(authString);

            // use Managed Identity (MI) for secure access to Key Vault
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(tokenProvider.KeyVaultTokenCallback));

            // read a key to make sure the connection is valid
            await keyVaultClient.GetSecretAsync(kvUrl, Constants.CosmosUrl).ConfigureAwait(false);

            // return the client
            return keyVaultClient;
        }
        catch (Exception ex)
        {
            if (DateTime.Now <= timeout && authType == "MI")
            {
                // retry MI connections for pod identity
                Console.WriteLine($"KeyVault:Retry");
                await Task.Delay(1000).ConfigureAwait(false);
            }
            else
            {
                // log and fail
                Console.WriteLine($"KeyVault:Exception: {ex.Message}\n{ex}");
                return null;
            }
        }
    }
}

```

## Versioning

Helium builds a version string in version attribute in the assembly version following the pattern: x.y.z+MMdd.hhmm (appending date time of build).

[helium.csproj](https://github.com/retaildevcrews/helium-csharp/blob/master/src/app/helium.csproj)

```c#

<Version>1.0.8+$([System.DateTime]::UtcNow.ToString(`MMdd-HHmm`))</Version>

```

The version is displayed at app startup, in the healthz/ietf endpoint output, and the /version endpoint output. The app version string is retrieved from the assembly in the version middleware.

[version.cs](https://github.com/retaildevcrews/helium-csharp/blob/master/src/app/Middleware/version.cs)

```c#

/// <summary>
/// Get the app version
/// </summary>
public static string Version
{
    get
    {
        if (string.IsNullOrEmpty(version))
        {
            if (Attribute.GetCustomAttribute(Assembly.GetEntryAssembly(), typeof(AssemblyInformationalVersionAttribute)) is AssemblyInformationalVersionAttribute v)
            {
                version = v.InformationalVersion;
            }
        }

        return version;
    }
}

```

## Dependency Injection (DI)

ASP.Net Core uses Dependency Injection (DI) to share objects across different controllers and modules.

### Key Vault

If you need access to Key Vault in your app, you can retrieve the Key Vault Client from ASP.NET's DI rather than have to track credentials and create a new connection.

#### Adding Key Vault via ASP.NET DI

[KeyVaultConnectionExtension.cs](https://github.com/RetailDevCrews/helium-csharp/blob/master/src/app/KeyVault.Extensions/KeyVaultConnectionExtension.cs#L8)

```c#

public static IServiceCollection AddKeyVaultConnection(this IServiceCollection services, KeyVaultClient client, string uri)
{
    // add the KeyVaultConnection as a singleton
    services.AddSingleton<IKeyVaultConnection>(new KeyVaultConnection
    {
        Client = client,
        Uri = uri
    });

    return services;
}

```

[Program.cs](https://github.com/RetailDevCrews/helium-csharp/blob/master/src/app/Program.cs#L474)

```c#

// add the KeyVaultConnection via DI
services.AddKeyVaultConnection(kvClient, new Uri(kvUrl));

```

### Data Access Layer (DAL)

The controllers need access to Helium's implementation of IDal in order to retrieve results from Cosmos DB, so we add the data access layer via DI as a singleton.

#### Adding IDal via ASP.NET DI

[Program.cs](https://github.com/RetailDevCrews/helium-csharp/blob/master/src/app/Program.cs#L474)

```c#

IWebHostBuilder builder = WebHost.CreateDefaultBuilder()
.UseConfiguration(config)
.UseKestrel()
.UseUrls(string.Format($"http://*:{Constants.Port}/"))
.UseStartup<Startup>()
.ConfigureServices(services =>
{
    // add the data access layer via DI
    services.AddDal(config.GetValue<string>(Constants.CosmosUrl),
        config.GetValue<string>(Constants.CosmosKey),
        config.GetValue<string>(Constants.CosmosDatabase),
        config.GetValue<string>(Constants.CosmosCollection));
});

```

#### Retrieve the Data Access Layer from ASP.NET DI

A controller or other module can retrieve the data access layer from ASP.NET DI by calling ```GetService<T>``` or by including IDAL in the controller's constructor.

[ActorsController.cs](https://github.com/RetailDevCrews/helium-csharp/blob/master/src/app/Controllers/ActorsController.cs#L20)
[Program.cs](https://github.com/RetailDevCrews/helium-csharp/blob/master/src/app/Program.cs#L279)

```c#

// retrive in constructor
public ActorsController(ILogger<ActorsController> logger, IDAL dal)

// retrive via code
var dal = host.Services.GetService<IDAL>();

```

### Application Insights

We store the application insights instrumentation key in Key Vault to configure Helium to use Application Insights. When configured, the DI creates a singleton instance of TelemetryClient that can be used to track custom events and metrics.

#### Adding Application Insights from ASP.NET DI

[Startup.cs](https://github.com/RetailDevCrews/helium-csharp/blob/master/src/app/Startup.cs#L48)

```c#

// add App Insights if key set
string appInsightsKey = Configuration.GetValue<string>(Constants.AppInsightsKey);

if (!string.IsNullOrEmpty(appInsightsKey))
{
    services.AddApplicationInsightsTelemetry(appInsightsKey);
}

```

#### Using the TelemetryClient from DI to track custom metric

[Program.cs](https://github.com/RetailDevCrews/helium-csharp/blob/master/src/app/Program.cs#L288)

```c#

// send a NewKeyLoadedMetric to App Insights
if (!string.IsNullOrEmpty(config[Constants.AppInsightsKey]))
{
    var telemetryClient = host.Services.GetService<TelemetryClient>();

    if (telemetryClient != null)
    {
        telemetryClient.TrackMetric(Constants.NewKeyLoadedMetric, 1);
    }
}

```

## Middleware

There is a robotsText middleware extension method added to Helium to handle a default warmup request of /robots43245.txt (43245 is random) when deploying to Azure App Service. Because Helium does not expect this request as part of its normal app logic, it would cause a 404 error, or in this case a false (expected) error, to appear in Azure Monitor reporting. This extension helps keep reporting clean and only contain true errors warranting investigation. Code: [robotsText.cs](https://github.com/RetailDevCrews/helium-csharp/blob/master/src/app/Middleware/robotsText.cs)

## Logging

A custom Request Logger extension is added to handle logging Http request information. This can be configured with LoggerOptions to control which requests to log based on status code.  By default, only 4xx and 5xx responses are logged.  This helps make logs easy to search through when debugging errors, rather than having to navigate through several successful requests.  In addition to the request logger, helium logs to console the console. Log level can be controlled with the --log-level command line parameter. The default is `warn`

Code: [requestLogger.cs](https://github.com/retaildevcrews/helium-csharp/blob/main/src/app/Middleware/RequestLogger/logger.cs)

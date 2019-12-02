# Developer Documentation

## Index:
1. [Managed Identity and Key Vault](#managed-identity-and-key-vault)
2. [Key Rotation](#key-rotation)
3. [Cosmos DB](#cosmos-db)
    - [Reconnect](#reconnect)
    - [Partition Key Function](#partition-key-function)
4. [AKS Pod Identity Support](#aks-pod-identity-support)
5. [Versioning](#versioning)
6. [Dependency Injection (DI)](#dependency-injection-(di))
    - [Key Vault](#key-vault)
    - [Data Access Layer (DAL)](#data-access-layer-(DAL))
    - [Application Insights](#application-insights)
7. [Robots Middleware](#robots-middleware)
8. [Logging](#logging)

## Managed Identity and Key Vault

After creating a Managed Identity for the Helium web app and assigning get and list secret permissions to Key Vault, the following code successfully authenticates using Managed Identity to create the Key Vault Client. Leveraging Managed Identity in this way eliminates the need to store any credential information in app code. This also works in the local development scenario as long as the developer has access to the Key Vault and is logged in to the Azure CLI with az login. 

[Program.cs](./src/app/Program.cs#L240)
```c#

// use Managed Identity (MSI) for secure access to Key Vault
var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(new AzureServiceTokenProvider().KeyVaultTokenCallback));

// read a key to make sure the connection is valid 
await keyVaultClient.GetSecretAsync(kvUrl, Constants.CosmosUrl);

// return the client
return keyVaultClient;

```

## Key Rotation

ASP.NET IConfiguration does not currently track changes to Key Vault secrets. Helium implements a loop in Program.cs that continuously checks Key Vault for changes to the CosmosDB paramaters and calls IDal::Reconnect so that Key Rotation and other scenarios can be supported.

[Program.cs](./src/app/Program.cs#L83)
```c#

// reload the config from Key Vault
config.Reload();

// get the DAL from ASP.NET DI
var dal = _host.Services.GetService<IDAL>();

// this will only reconnect if one or more of the variables changed, so no need to track values here
await dal.Reconnect(config[Constants.CosmosUrl], config[Constants.CosmosKey], config[Constants.CosmosDatabase], config[Constants.CosmosCollection]);

// log Cosmos Key changes to std out and App Insights
if (key != config[Constants.CosmosKey])
{
    key = config[Constants.CosmosKey];
    Console.WriteLine($"Cosmos Key Rotated: {key.Substring(0, 5)} ...");

...

```

## Cosmos DB

### Reconnect

The Reconnect method on IDal allows you to programmatically change your CosmosDB client configuration. If one or more of the connection paramaters changed, Reconnect will attempt to connect to CosmosDB using the new Url, key, database and collection. If it fails, the data access layer will continue using the existing credentials. The force parameter allows you to force the reconnect even if the other parameters don't change.

[dalMain.cs](./src/app/DataAccessLayer/dalMain.cs#L46)
```c#

public async Task Reconnect(string cosmosUrl, string cosmosKey, string cosmosDatabase, string cosmosCollection, bool force = false)
{
    if (force ||
        _cosmosDetails.CosmosCollection != cosmosCollection ||
        _cosmosDetails.CosmosDatabase != cosmosDatabase ||
        _cosmosDetails.CosmosKey != cosmosKey ||
        _cosmosDetails.CosmosUrl != cosmosUrl)
    {
        CosmosDetails d = new CosmosDetails
        {
            CosmosCollection = cosmosCollection,
            CosmosDatabase = cosmosDatabase,
            CosmosKey = cosmosKey,
            CosmosUrl = cosmosUrl
        };

        // open and test a new client / container
        // this will throw an exception if the parameters are not valid
        d.Client = await OpenAndTestCosmosClient(cosmosUrl, cosmosKey, cosmosDatabase, cosmosCollection);
        d.Container = d.Client.GetContainer(cosmosDatabase, cosmosCollection);

        // set the current CosmosDetail
        _cosmosDetails = d;
    }
}

```

Open and test the CosmosDB connection / database / collection. The call to ReadItemAsync retrieves the Action Genre document and verifies that the new parameters can read the collection.

[Program.cs](./src/app/DataAccessLayer/dalMain.cs#L80)
```c#

var c = new CosmosClient(cosmosUrl, cosmosKey, _cosmosDetails.CosmosClientOptions);
var con = c.GetContainer(cosmosDatabase, cosmosCollection);
await con.ReadItemAsync<dynamic>("action", new PartitionKey("0"));

```

### Partition Key Function

In order to directly read a document using 1 RU (assuming the document is 1K or less), you need the document's ID and partition key. A good CosmosDB best practice is to compute the partition key from the ID. In our case, we use the integer portion of the Movie or Actor document mod 10. This gives us 10 partitions ("0" - "9") with good distribution. For a deeper discussion on the document modeling decisions, please read this [document](https://github.com/4-co/imdb)

[Program.cs](./src/app/DataAccessLayer/dalMain.cs#L119)
```c#

public static string GetPartitionKey(string id)
{
    // validate id
    if (id.Length > 5 &&
        (id.StartsWith("tt") || id.StartsWith("nm")) &&
        Int32.TryParse(id.Substring(2), out int idInt))
    {
        return (idInt % 10).ToString();
    }

    throw new ArgumentException("GetPartitionKey");
}

```

## AKS Pod Identity Support

AKS Pod Identity is currently in preview and allows applications running within containers on AKS to use Managed Identities and Service Principals to access Azure Services such as Key Vault. Azure App Service also supports Managed Identity for containers.

The first time an AKS pod uses a Managed Identity, it has to start a new proxy. In testing, this usually takes about 30 seconds. If the Managed Identity proxy is not available, Helium will fail to start (by design) and throw an error. While AKS will attempt to automatically restart the pod, it's a known error that we want to manage. A simple retry around the Key Vault client causes the pod start to wait for the Managed Identity proxy to be ready.

Note that once the MI proxy is running, responses are generally under 100ms, so the retry code is not used in that case. The retry code is also not used in the App Service scenario as App Service ensures the proxy is running before starting Helium.

[Program.cs](./src/app/Program.cs#L228)
```c#

/// <summary>
/// Get a valid key vault client
/// AKS takes time to spin up the first pod identity, so
///   we retry for up to 90 seconds
/// </summary>
/// <param name="kvUrl">URL of the key vault</param>
/// <returns></returns>
static async Task<KeyVaultClient> GetKeyVaultClient(string kvUrl)
{
    // retry Managed Identity for 90 seconds
    //   AKS has to spin up an MI pod which can take a while the first time on the pod
    DateTime timeout = DateTime.Now.AddSeconds(90.0);

    while (true)
    {
        try
        {
            // use Managed Identity (MSI) for secure access to Key Vault
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(new AzureServiceTokenProvider().KeyVaultTokenCallback));

            // read a key to make sure the connection is valid 
            await keyVaultClient.GetSecretAsync(kvUrl, Constants.CosmosUrl);

            // return the client
            return keyVaultClient;
        }
        catch (Exception ex)
        {
            if (DateTime.Now <= timeout)
            {
                // log and retry

                Console.WriteLine($"KeyVault:Retry: {ex.Message}");
                await Task.Delay(1000);
            }
            else
            {
                // log and fail

                Console.WriteLine($"KeyVault:Exception: {ex.Message}\n{ex}");
                Environment.Exit(-1);
            }
        }
    }
}

```

## Versioning

Helium dynamically builds a version string based on the assembly version and date time of build. This is displayed in both the Healthz output as well as the Swagger UI. 

[Version.cs](./src/app/Version.cs#L12)
```c#

string file = System.Reflection.Assembly.GetExecutingAssembly().Location;
DateTime dt = System.IO.File.GetCreationTime(file);

var aVer = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

return string.Format($"{aVer.Major}.{aVer.Minor}.{dt.ToString("MMdd.HHmm")}");

```

## Dependency Injection (DI)

ASP.Net Core uses Dependency Injection (DI) to share objects across different controllers and modules.

### Key Vault 

If you need access to Key Vault in your app, you can retrieve the Key Vault Client from ASP.NET's DI rather than have to track credentials and create a new connection.

#### Adding Key Vault via ASP.NET DI

[KeyVaultConnectionExtension.cs](./src/app/KeyVaultConnectionExtension.cs#L8)
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

#### Retrieving the Key Vault Client from ASP.NET DI

```c#

var keyVaultConn = _host.Services.GetService<IKeyVaultConnection>();

```
### Data Access Layer (DAL)

The controllers need access to Helium's implementation of IDal in order to retrieve results from CosmosDB, so we add the data access layer via DI as a singleton.

#### Adding IDal via ASP.NET DI

[Program.cs](./src/app/Program.cs#L288)
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

A controller or other module can retrieve the data access layer from ASP.NET DI by calling GetService<T> or by including IDAL in the controller's constructor.

[ActorsController.cs](./src/app/Controllers/ActorsController.cs#L20)
[Program.cs](./src/app/Program.cs#L106)
```c#

// retrive in constructor
public ActorsController(ILogger<ActorsController> logger, IDAL dal)

// retrive via code
var dal = _host.Services.GetService<IDAL>();

```

### Application Insights

Optionally store the application insights instrumentation key in Key Vault to configure Helium to use Application Insights. When configured, the DI creates a singleton instance of TelemetryClient that can be used to track custom events and metrics.

#### Adding Application Insights from ASP.NET DI 

[Startup.cs](./src/app/Startup.cs#L48)
```c#

// add App Insights if key set
string appInsightsKey = Configuration.GetValue<string>(Constants.AppInsightsKey);

if (!string.IsNullOrEmpty(appInsightsKey))
{
    services.AddApplicationInsightsTelemetry(appInsightsKey);
}

```

#### Using the TelemetryClient from DI to track custom metric

[Program.cs](./src/app/Program.cs#L120)
```c#

// send a NewKeyLoadedMetric to App Insights
if (!string.IsNullOrEmpty(config[Constants.AppInsightsKey]))
{
    var telemetryClient = _host.Services.GetService<TelemetryClient>();

    if (telemetryClient != null)
    {
        telemetryClient.TrackMetric(Constants.NewKeyLoadedMetric, 1);
    }
}

```

## Robots Middleware

TODO: Is this needed? this is pretty much explained in comments, but may be nice to call out separately.

There is a robotsText middleware extension method added to Helium to handle a default warmup request of /robots43245.txt (43245 is random) when deploying to Azure App Service. Because Helium does not expect this request as part of its normal app logic, it would cause a 404 error, or in this case a false (expected) error, to appear in Azure Monitor reporting. This extension helps keep reporting clean and only contain true errors warranting investigation. Code: [robotsText.cs](./src/app/Middleware/robotsText.cs)

## Logging

A custom Request Logger extension is added to handle logging Http request information. This can be configured with LoggerOptions to control which requests to log based on status code.  By default, only 4xx and 5xx responses are logged.  This helps make logs easy to search through when debugging errors, rather than having to navigate through several successful requests.  In addition to the request logger, helium logs primarily errors to console to keep output clean.   

Code: [requestLogger.cs](./src/app/Middleware/requestLogger.cs)

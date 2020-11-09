using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Microsoft.Rest.Azure.Authentication;
using SimpleStream.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStream.Services
{
  public class AzureStreamService
  {
    #region Fields

    private readonly ConsoleWraper consoleWraper;
    private string assetName;
    private IAzureMediaServicesClient client;
    private ConfigWrapper config;
    private bool hasStreamingPaths;
    private string liveEventName;
    private string liveOutputName;
    private string streamingEndpointName;
    private string streamingLocatorName;
    private Process streamlabsProcess;
    private string uniqueness;
    private string previewEndpoint;

    #endregion Fields

    #region Constructors

    public AzureStreamService(ConsoleWraper consoleWraper)
    {
      this.consoleWraper = consoleWraper;
    }

    #endregion Constructors

    #region Methods

    public async Task Cleanup()
    {
      try
      {
        if (hasStreamingPaths)
        {
          await this.CleanupLiveEventAndOutputAsync(this.client, config.ResourceGroup, config.AccountName, this.liveEventName);

          if (!streamlabsProcess.HasExited)
          {
            // Close process by sending a close message to its main window.
            this.streamlabsProcess.CloseMainWindow();
            // Free resources associated with process.
            this.streamlabsProcess.Close();
          }

          this.consoleWraper.WriteLine("The LiveOutput and LiveEvent are now deleted.  The event is available as an archive and can still be streamed.");
          this.consoleWraper.WriteLine("Press enter to finish cleanup...");
        }
        else
        {
          this.consoleWraper.WriteLine("No Streaming Paths were detected.  Has the Stream been started?");
          this.consoleWraper.WriteLine("Cleaning up and Exiting...");
        }
      }
      catch (ApiErrorException e)
      {
        this.consoleWraper.WriteLine("Hit ApiErrorException");
        this.consoleWraper.WriteLine($"\tCode: {e.Body.Error.Code}");
        this.consoleWraper.WriteLine($"\tCode: {e.Body.Error.Message}");
        this.consoleWraper.WriteLine();
        this.consoleWraper.WriteLine("Exiting, cleanup may be necessary...");
      }
      finally
      {
        await this.CleanupLiveEventAndOutputAsync(this.client, config.ResourceGroup, config.AccountName, this.liveEventName);
        await this.CleanupLocatorandAssetAsync(this.client, config.ResourceGroup, config.AccountName, this.streamingLocatorName, this.assetName);
      }
    }

    public void CleanUpLiveEventAndLocator()
    {
      this.CleanupLiveEventAndOutput(this.client, config.ResourceGroup, config.AccountName, this.liveEventName);
      this.CleanupLocatorandAsset(this.client, config.ResourceGroup, config.AccountName, this.streamingLocatorName, this.assetName);
    }

    public async Task<string> CreateOutputs()
    {
      string playerPath = string.Empty; 

      try
      {
        Asset asset = await this.CreateLiveOutput(this.config, this.client);
        StreamingEndpoint streamingEndpoint = await this.SetupStreamingEndpoint(this.config, this.client, asset);
        ListPathsResponse paths = await this.client.StreamingLocators.ListPathsAsync(this.config.ResourceGroup, this.config.AccountName, this.streamingLocatorName);
        this.GetStreamingPaths(streamingEndpoint, paths, out StringBuilder stringBuilder, out playerPath, out this.hasStreamingPaths);

        if (hasStreamingPaths)
        {
          this.consoleWraper.WriteLine(stringBuilder.ToString());
          this.consoleWraper.WriteLine("Open the following URL to playback the published,recording LiveOutput in the Azure Media Player");
          this.consoleWraper.WriteLine($"\t https://ampdemo.azureedge.net/?url={playerPath}&heuristicprofile=lowlatency");
          this.consoleWraper.WriteLine();

          this.consoleWraper.WriteLine("Continue experimenting with the stream until you are ready to finish.");
          this.consoleWraper.WriteLine("Press enter to stop the LiveOutput...");
        }
        else
        {
          this.consoleWraper.WriteLine("No Streaming Paths were detected.  Has the Stream been started?");
          this.consoleWraper.WriteLine("Press enter to clean up and Exiting...");
        }
      }
      catch (ApiErrorException e)
      {
        this.consoleWraper.WriteLine("Hit ApiErrorException");
        this.consoleWraper.WriteLine($"\tCode: {e.Body.Error.Code}");
        this.consoleWraper.WriteLine($"\tCode: {e.Body.Error.Message}");
        this.consoleWraper.WriteLine();
        this.consoleWraper.WriteLine("Exiting, cleanup may be necessary...");
      }

      return playerPath;
    }

    public async Task<string> Startup()
    {
      try
      {
        this.config = new ConfigWrapper(new ConfigurationBuilder()
       .SetBasePath(Directory.GetCurrentDirectory())
       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
       .AddEnvironmentVariables()
       .Build());

        this.client = await CreateMediaServicesClientAsync(this.config);

        // Creating a unique suffix so that we don't have name collisions if you run the sample
        // multiple times without cleaning up.
        this.uniqueness = Guid.NewGuid().ToString().Substring(0, 13);
        this.liveEventName = "liveevent-" + this.uniqueness;
        this.assetName = "archiveAsset" + this.uniqueness;
        this.liveOutputName = "liveOutput" + this.uniqueness;
        this.streamingLocatorName = "streamingLocator" + this.uniqueness;
        this.streamingEndpointName = "default";

        // Getting the mediaServices account so that we can use the location to create the
        // LiveEvent and StreamingEndpoint
        MediaService mediaService = await this.client.Mediaservices.GetAsync(this.config.ResourceGroup, this.config.AccountName);
        LiveEvent liveEvent = await this.CreateLiveEvent(this.liveEventName, this.client, mediaService, this.config);
        this.streamlabsProcess = this.SetupStream(this.config, liveEvent);
      }
      catch (ApiErrorException e)
      {
        this.consoleWraper.WriteLine("Hit ApiErrorException");
        this.consoleWraper.WriteLine($"\tCode: {e.Body.Error.Code}");
        this.consoleWraper.WriteLine($"\tCode: {e.Body.Error.Message}");
        this.consoleWraper.WriteLine();
        this.consoleWraper.WriteLine("Exiting, cleanup may be necessary...");
      }
      return this.previewEndpoint;
    }

    /// <summary>
    /// Creates the AzureMediaServicesClient object based on the credentials
    /// supplied in local configuration file.
    /// </summary>
    /// <param name="config">The parm is of type ConfigWrapper. This class reads values from local configuration file.</param>
    /// <returns></returns>
    // <CreateMediaServicesClient>
    private static async Task<IAzureMediaServicesClient> CreateMediaServicesClientAsync(ConfigWrapper config)
    {
      ServiceClientCredentials credentials = await GetCredentialsAsync(config);

      return new AzureMediaServicesClient(config.ArmEndpoint, credentials)
      {
        SubscriptionId = config.SubscriptionId,
      };
    }

    private static async Task<ServiceClientCredentials> GetCredentialsAsync(ConfigWrapper config)
    {
      // Use ApplicationTokenProvider.LoginSilentWithCertificateAsync or UserTokenProvider.LoginSilentAsync to get a token using service principal with certificate
      //// ClientAssertionCertificate
      //// ApplicationTokenProvider.LoginSilentWithCertificateAsync

      // Use ApplicationTokenProvider.LoginSilentAsync to get a token using a service principal with symetric key
      ClientCredential clientCredential = new ClientCredential(config.AadClientId, config.AadSecret);
      return await ApplicationTokenProvider.LoginSilentAsync(config.AadTenantId, clientCredential, ActiveDirectoryServiceSettings.Azure);
    }

    private void CleanupLiveEventAndOutput(IAzureMediaServicesClient client, string resourceGroup, string accountName, string liveEventName)
    {
      try
      {
        LiveEvent liveEvent = client.LiveEvents.Get(resourceGroup, accountName, liveEventName);

        if (liveEvent != null)
        {
          if (liveEvent.ResourceState == LiveEventResourceState.Running)
          {
            // If the LiveEvent is running, stop it and have it remove any LiveOutputs
            client.LiveEvents.Stop(resourceGroup, accountName, liveEventName, removeOutputsOnStop: true);
          }

          // Delete the LiveEvent
          client.LiveEvents.Delete(resourceGroup, accountName, liveEventName);
        }
      }
      catch (ApiErrorException e)
      {
        this.consoleWraper.WriteLine("CleanupLiveEventAndOutput -- Hit ApiErrorException");
        this.consoleWraper.WriteLine($"\tCode: {e.Body.Error.Code}");
        this.consoleWraper.WriteLine($"\tCode: {e.Body.Error.Message}");
        this.consoleWraper.WriteLine();
      }
    }

    private void CleanupLocatorandAsset(IAzureMediaServicesClient client, string resourceGroup, string accountName, string streamingLocatorName, string assetName)
    {
      try
      {
        // Delete the Streaming Locator
        client.StreamingLocators.Delete(resourceGroup, accountName, streamingLocatorName);

        // Delete the Archive Asset
        client.Assets.Delete(resourceGroup, accountName, assetName);
      }
      catch (ApiErrorException e)
      {
        this.consoleWraper.WriteLine("CleanupLocatorandAsset -- Hit ApiErrorException");
        this.consoleWraper.WriteLine($"\tCode: {e.Body.Error.Code}");
        this.consoleWraper.WriteLine($"\tCode: {e.Body.Error.Message}");
        this.consoleWraper.WriteLine();
      }
    }

    private async Task CleanupLiveEventAndOutputAsync(IAzureMediaServicesClient client, string resourceGroup, string accountName, string liveEventName)
    {
      try
      {
        LiveEvent liveEvent = await client.LiveEvents.GetAsync(resourceGroup, accountName, liveEventName);

        if (liveEvent != null)
        {
          if (liveEvent.ResourceState == LiveEventResourceState.Running)
          {
            // If the LiveEvent is running, stop it and have it remove any LiveOutputs
            await client.LiveEvents.StopAsync(resourceGroup, accountName, liveEventName, removeOutputsOnStop: true);
          }

          // Delete the LiveEvent
          await client.LiveEvents.DeleteAsync(resourceGroup, accountName, liveEventName);
        }
      }
      catch (ApiErrorException e)
      {
        this.consoleWraper.WriteLine("CleanupLiveEventAndOutputAsync -- Hit ApiErrorException");
        this.consoleWraper.WriteLine($"\tCode: {e.Body.Error.Code}");
        this.consoleWraper.WriteLine($"\tCode: {e.Body.Error.Message}");
        this.consoleWraper.WriteLine();
      }
    }

    private async Task CleanupLocatorandAssetAsync(IAzureMediaServicesClient client, string resourceGroup, string accountName, string streamingLocatorName, string assetName)
    {
      try
      {
        // Delete the Streaming Locator
        await client.StreamingLocators.DeleteAsync(resourceGroup, accountName, streamingLocatorName);

        // Delete the Archive Asset
        await client.Assets.DeleteAsync(resourceGroup, accountName, assetName);
      }
      catch (ApiErrorException e)
      {
        this.consoleWraper.WriteLine("CleanupLocatorandAssetAsync -- Hit ApiErrorException");
        this.consoleWraper.WriteLine($"\tCode: {e.Body.Error.Code}");
        this.consoleWraper.WriteLine($"\tCode: {e.Body.Error.Message}");
        this.consoleWraper.WriteLine();
      }
    }

    private async Task<LiveEvent> CreateLiveEvent(string liveEventName, IAzureMediaServicesClient client, MediaService mediaService, ConfigWrapper config)
    {
      this.consoleWraper.WriteLine($"Creating a live event named {liveEventName}");
      this.consoleWraper.WriteLine();

      // Note: When creating a LiveEvent, you can specify allowed IP addresses in one of the following formats:
      //      IpV4 address with 4 numbers
      //      CIDR address range

      IPRange allAllowIPRange = new IPRange(
          name: "AllowAll",
          address: "0.0.0.0",
          subnetPrefixLength: 0
      );

      // Create the LiveEvent input IP access control.
      LiveEventInputAccessControl liveEventInputAccess = new LiveEventInputAccessControl
      {
        Ip = new IPAccessControl(
                  allow: new IPRange[]
                  {
                                allAllowIPRange
                  }
              )
      };

      // Create the LiveEvent Preview IP access control
      LiveEventPreview liveEventPreview = new LiveEventPreview
      {
        AccessControl = new LiveEventPreviewAccessControl(
              ip: new IPAccessControl(
                  allow: new IPRange[]
                  {
                                allAllowIPRange
                  }
              )
          )
      };

      // To get the same ingest URL for the same LiveEvent name:
      // 1. Set vanityUrl to true so you have ingest like:
      //        rtmps://liveevent-hevc12-eventgridmediaservice-usw22.channel.media.azure.net:2935/live/522f9b27dd2d4b26aeb9ef8ab96c5c77
      // 2. Set accessToken to a desired GUID string (with or without hyphen)

      LiveEvent liveEvent = new LiveEvent(
          location: mediaService.Location,
          description: "Sample LiveEvent for testing",
          //vanityUrl: false,
          encoding: new LiveEventEncoding(
                      // When encodingType is None, the service simply passes through the incoming video and audio layer(s) to the output
                      // When the encodingType is set to Standard or Premium1080p, a live encoder is used to transcode the incoming stream
                      // into multiple bit rates or layers. See https://go.microsoft.com/fwlink/?linkid=2095101 for more information
                      encodingType: LiveEventEncodingType.None,
                      presetName: null
                  ),
          input: new LiveEventInput(LiveEventInputProtocol.RTMP, liveEventInputAccess),
          preview: liveEventPreview,
          streamOptions: new List<StreamOptionsFlag?>()
          {
                        // Set this to Default or Low Latency
                        // When using Low Latency mode, you must configure the Azure Media Player to use the
                        // quick start hueristic profile or you won't notice the change.
                        // In the AMP player client side JS options, set -  heuristicProfile: "Low Latency Heuristic Profile".
                        // To use low latency optimally, you should tune your encoder settings down to 1 second GOP size instead of 2 seconds.
                        StreamOptionsFlag.LowLatency
          }
      );

      this.consoleWraper.WriteLine($"Creating the LiveEvent, be patient this can take time...");

      // When autostart is set to true, the Live Event will be started after creation.
      // That means, the billing starts as soon as the Live Event starts running.
      // You must explicitly call Stop on the Live Event resource to halt further billing.
      // The following operation can sometimes take awhile. Be patient.
      return await client.LiveEvents.CreateAsync(config.ResourceGroup, config.AccountName, liveEventName, liveEvent, autoStart: true);
    }

    private async Task<Asset> CreateLiveOutput(ConfigWrapper config, IAzureMediaServicesClient client)
    {
      // Create an Asset for the LiveOutput to use

      #region CreateAsset

      this.consoleWraper.WriteLine($"Creating an asset named {this.assetName}");
      this.consoleWraper.WriteLine();
      Asset asset = await client.Assets.CreateOrUpdateAsync(config.ResourceGroup, config.AccountName, this.assetName, new Asset());

      #endregion CreateAsset

      // Create the LiveOutput

      #region CreateLiveOutput

      string manifestName = "output";
      this.consoleWraper.WriteLine($"Creating a live output named {this.liveOutputName}");
      this.consoleWraper.WriteLine();

      LiveOutput liveOutput = new LiveOutput(assetName: asset.Name, manifestName: manifestName, archiveWindowLength: TimeSpan.FromMinutes(10));
      liveOutput = await client.LiveOutputs.CreateAsync(config.ResourceGroup, config.AccountName, this.liveEventName, this.liveOutputName, liveOutput);

      #endregion CreateLiveOutput

      return asset;
    }

    private void GetStreamingPaths(StreamingEndpoint streamingEndpoint, ListPathsResponse paths, out StringBuilder stringBuilder, out string playerPath, out bool hasStreamingPaths)
    {
      this.consoleWraper.WriteLine("The urls to stream the output from a client:");
      this.consoleWraper.WriteLine();
      stringBuilder = new StringBuilder();
      playerPath = string.Empty;
      for (int i = 0; i < paths.StreamingPaths.Count; i++)
      {
        UriBuilder uriBuilder = new UriBuilder
        {
          Scheme = "https",
          Host = streamingEndpoint.HostName
        };

        if (paths.StreamingPaths[i].Paths.Count > 0)
        {
          uriBuilder.Path = paths.StreamingPaths[i].Paths[0];
          stringBuilder.AppendLine($"\t{paths.StreamingPaths[i].StreamingProtocol}-{paths.StreamingPaths[i].EncryptionScheme}");
          stringBuilder.AppendLine($"\t\t{uriBuilder.ToString()}");
          stringBuilder.AppendLine();

          if (paths.StreamingPaths[i].StreamingProtocol == StreamingPolicyStreamingProtocol.Dash)
          {
            playerPath = uriBuilder.ToString();
          }
        }
      }

      hasStreamingPaths = stringBuilder.Length > 0;
    }

    private Process SetupStream(ConfigWrapper config, LiveEvent liveEvent)
    {
      // Get the input endpoint to configure the on premise encoder with
      string ingestUrl = liveEvent.Input.Endpoints.First().Url;
      this.consoleWraper.WriteLine($"The ingest url to configure the on premise encoder with is:");
      this.consoleWraper.WriteLine($"\t{ingestUrl}");
      this.consoleWraper.WriteLine();

      this.consoleWraper.WriteLine($"The configuration file is:");
      this.consoleWraper.WriteLine($"\t{config.ConfigFile}");
      string serviceJson = SimpleStreamWpf.Properties.Resources.service.Replace("{server}", ingestUrl);

      if (File.Exists(config.ConfigFile))
      {
        File.Delete(config.ConfigFile);
      }

      using (var file = File.CreateText(config.ConfigFile))
      {
        file.Write(serviceJson);
        file.Flush();
      }

      this.consoleWraper.WriteLine($"The configuration file was patched.");
      this.consoleWraper.WriteLine();

      // Use the previewEndpoint to preview and verify
      // that the input from the encoder is actually being received
      this.previewEndpoint = liveEvent.Preview.Endpoints.First().Url;
      this.consoleWraper.WriteLine($"The preview url is:");
      this.consoleWraper.WriteLine($"\t{previewEndpoint}");
      this.consoleWraper.WriteLine();

      this.consoleWraper.WriteLine($"Open the live preview in your browser and use the Azure Media Player to monitor the preview playback:");
      this.consoleWraper.WriteLine($"\thttps://ampdemo.azureedge.net/?url={previewEndpoint}&heuristicprofile=lowlatency");
      this.consoleWraper.WriteLine();

      this.consoleWraper.WriteLine($"The obs executable is:");
      this.consoleWraper.WriteLine($"\t{config.ObsExecutable}");
      this.consoleWraper.WriteLine($"Starting OBS...");
      Process streamlabsProcess = Process.Start(config.ObsExecutable);
      this.consoleWraper.WriteLine();

      this.consoleWraper.WriteLine("Start the live stream now, sending the input to the ingest url and verify that it is arriving with the preview url.");
      this.consoleWraper.WriteLine("IMPORTANT TIP!: Make ABSOLUTLEY CERTAIN that the video is flowing to the Preview URL before continuing!");
      this.consoleWraper.WriteLine("Press enter to continue...");
      return streamlabsProcess;
    }

    private async Task<StreamingEndpoint> SetupStreamingEndpoint(ConfigWrapper config, IAzureMediaServicesClient client, Asset asset)
    {
      // Create the StreamingLocator

      #region CreateStreamingLocator

      this.consoleWraper.WriteLine($"Creating a streaming locator named {this.streamingLocatorName}");
      this.consoleWraper.WriteLine();

      StreamingLocator locator = new StreamingLocator(assetName: asset.Name, streamingPolicyName: PredefinedStreamingPolicy.ClearStreamingOnly);
      locator = await client.StreamingLocators.CreateAsync(config.ResourceGroup, config.AccountName, this.streamingLocatorName, locator);

      // Get the default Streaming Endpoint on the account
      StreamingEndpoint streamingEndpoint = await client.StreamingEndpoints.GetAsync(config.ResourceGroup, config.AccountName, this.streamingEndpointName);

      // If it's not running, Start it.
      if (streamingEndpoint.ResourceState != StreamingEndpointResourceState.Running)
      {
        this.consoleWraper.WriteLine("Streaming Endpoint was Stopped, restarting now..");
        await client.StreamingEndpoints.StartAsync(config.ResourceGroup, config.AccountName, this.streamingEndpointName);
      }

      #endregion CreateStreamingLocator

      return streamingEndpoint;
    }

    #endregion Methods
  }
}
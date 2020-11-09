using Microsoft.Extensions.Configuration;
using System;

namespace SimpleStream.Configuration
{
  public class ConfigWrapper
  {
    #region Fields

    private readonly IConfiguration _config;

    #endregion Fields

    #region Constructors

    public ConfigWrapper(IConfiguration config)
    {
      this._config = config;
    }

    #endregion Constructors

    #region Properties

    public string AadClientId => this._config["AadClientId"];

    public Uri AadEndpoint => new Uri(this._config["AadEndpoint"]);

    public string AadSecret => this._config["AadSecret"];

    public string AadTenantId => this._config["AadTenantId"];

    public string AccountName => this._config["AccountName"];

    public Uri ArmAadAudience => new Uri(this._config["ArmAadAudience"]);

    public Uri ArmEndpoint => new Uri(this._config["ArmEndpoint"]);

    public string ConfigFile => this._config["ConfigFile"];

    public string Location => this._config["Location"];

    public string ObsExecutable => this._config["ObsExecutable"];

    public string ResourceGroup => this._config["ResourceGroup"];

    public string SubscriptionId => this._config["SubscriptionId"];

    #endregion Properties
  }
}
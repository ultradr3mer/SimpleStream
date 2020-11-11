using Microsoft.Extensions.Configuration;
using System;

namespace OneClickStream.Configuration
{
  public class ConfigWrapper
  {
    #region Fields

    private readonly IConfiguration config;

    #endregion Fields

    #region Constructors

    public ConfigWrapper(IConfiguration config)
    {
      this.config = config;
    }

    #endregion Constructors

    #region Properties

    public string ConfigFile => this.config["ConfigFile"];

    public string ObsExecutable => this.config["ObsExecutable"];

    public string Username => this.config["Username"];

    public string Password => this.config["Password"];

    #endregion Properties
  }
}
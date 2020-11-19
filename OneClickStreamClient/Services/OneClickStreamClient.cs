using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OneClickStreamClient.Services
{
  public class CustomHttpClient : HttpClient
  {
    #region Constructors

    public CustomHttpClient(string username, string password) : base(new HttpClientHandler
    {
      Credentials = new NetworkCredential(username, password)
    })
    {
      byte[] authBytes = Encoding.UTF8.GetBytes(username + ":" + password);
      this.DefaultRequestHeaders.Add("Authorization", "BASIC " + Convert.ToBase64String(authBytes));
    }

    #endregion Constructors
  }
}

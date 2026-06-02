using System.ServiceModel;
using System.ServiceModel.Web;
using CSAnalyzer.Service.Models;

namespace CSAnalyzer.Service
{
    [ServiceContract]
    public interface ICSService
    {
        [OperationContract]
        [WebInvoke(Method = "GET",
                   ResponseFormat = WebMessageFormat.Json,
                   UriTemplate = "profile/{steamId}")]
        PlayerProfile GetProfile(string steamId);
    }
}
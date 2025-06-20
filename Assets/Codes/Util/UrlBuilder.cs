using System.Collections.Specialized;

namespace Codes.Util
{
    public class UrlBuilder
    {
        private readonly string baseUrl;
        NameValueCollection query = System.Web.HttpUtility.ParseQueryString(string.Empty);
        private int port = -1;
        private string endPoint = "";
        public UrlBuilder(string baseUrl)
        {
            this.baseUrl = baseUrl;
        }

        public UrlBuilder AddParam(string paramName, string paramValue)
        {
            query[paramName] = paramValue;
            return this;
        }

        public UrlBuilder SetPort(int port)
        {
            this.port = port;
            return this;
        }

        /// <summary>
        /// set url endpoint
        /// </summary>
        /// <param name="endpoint">follow this Style-> /auth/sign-in</param>
        public UrlBuilder SetEndPoint(string endpoint)
        {
            if (!endpoint.StartsWith("/"))
                endpoint = "/" + endpoint;
            this.endPoint = endpoint;
            return this;
        }
        
        public string Build()
        {
            return baseUrl+((port==-1)?"":":"+port)+endPoint+"?"+query;
        }
    
    
    }
}

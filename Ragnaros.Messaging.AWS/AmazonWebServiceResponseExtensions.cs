using Amazon.Runtime;
using System.Net;

namespace Ragnaros.Messaging.AWS
{
    public static class AmazonWebServiceResponseExtensions
    {
        public static void ThrowIfUnsuccessful(this AmazonWebServiceResponse amazonWebServiceResponse)
        {
            if (amazonWebServiceResponse.HttpStatusCode != HttpStatusCode.OK)
                throw new AmazonServiceException
                    (
                        message: "Failed message from amazon web services",
                        errorType: ErrorType.Unknown,
                        errorCode: null,
                        requestId: amazonWebServiceResponse.ResponseMetadata.RequestId,
                        statusCode: amazonWebServiceResponse.HttpStatusCode
                    );
        }
    }
}

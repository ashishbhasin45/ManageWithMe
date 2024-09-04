using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Runtime.Internal;
using Amazon.S3;
using Amazon.S3.Model;
using System.Net;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace TodoFileHandler
{
    public class Function
    {
        private IDynamoDBContext dynamoDBContext;
        private IAmazonS3 s3Client;
        private static string bucketName = Environment.GetEnvironmentVariable("bucketName");
        private static Dictionary<string, string> headers = new Dictionary<string, string>
        {
            { "Access-Control-Allow-Headers", "Content-Type" },
            { "Access-Control-Allow-Origin",  "*" },
            { "Access-Control-Allow-Methods", "OPTIONS,GET" }
        };

        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public Function(IDynamoDBContext dBContext, IAmazonS3 amazonS3)
        {
            this.dynamoDBContext = dBContext;
            this.s3Client = amazonS3;
        }

        [LambdaFunction]
        public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            IDictionary<string, string> queryParams = request.QueryStringParameters;
            var email = queryParams["email"];
            var token = Convert.ToInt32(queryParams["token"]);
            var todoId = queryParams["todoId"];
            var apiResponse = new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)HttpStatusCode.Unauthorized,
                Headers = headers,
                Body = null,

            };

            if (email != null)
            {
                var user = await this.dynamoDBContext.LoadAsync<User>(email);
                if (user != null && user.Otp == token)
                {

                    var response = await this.getFiles(todoId);
                    apiResponse.StatusCode = (int)HttpStatusCode.OK;
                    apiResponse.Body = JsonSerializer.Serialize(response);
                }
            }

            return apiResponse;
        }
        private async Task<FileResponse> getFiles(string todoId)
        {
            var request = new ListObjectsV2Request()
            {
                BucketName = bucketName,
                Prefix = todoId
            };

            var response = await this.s3Client.ListObjectsV2Async(request);
            FileResponse fileResponse = null;
            if (response.KeyCount > 0)
            {
                var file = response.S3Objects.First();
                var presignRequest = new GetPreSignedUrlRequest()
                {
                    BucketName = bucketName,
                    Key = file.Key,
                    Expires = DateTime.UtcNow.AddSeconds(500)
                };

                //var file = await this.s3Client.GetObjectAsync(bucketName, t.Key);
                fileResponse = new FileResponse
                {
                    Url = s3Client.GetPreSignedURL(presignRequest),
                    FileName = file.Key
                    //FileType = file.Headers.ContentType
                };

            }

            return fileResponse;
        }
    }
}

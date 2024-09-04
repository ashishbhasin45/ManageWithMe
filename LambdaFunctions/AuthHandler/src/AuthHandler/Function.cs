using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Runtime.Internal;
using System.Net;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AuthHandler
{
    public class Function
    {
        private IAmazonDynamoDB _dynamoDB;
        private IDynamoDBContext _dynamoDBContext;
        private static Dictionary<string, string> headers = new Dictionary<string, string>
        {
            { "Access-Control-Allow-Headers", "Content-Type" },
            { "Access-Control-Allow-Origin",  "*" },
            { "Access-Control-Allow-Methods", "OPTIONS,POST" }
        };
        public Function(IDynamoDBContext dBContext)
        {
            _dynamoDBContext = dBContext;
        }

        [LambdaFunction]
        public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest input, ILambdaContext context)
        {
            var requestBody = JsonSerializer.Deserialize<User>(input.Body);
            var validUser = await CheckValidUser(requestBody.Email, requestBody.Otp);
            if (!validUser)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Body = "Wrong OTP",
                    Headers = headers
                };
            }

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Headers = headers
            };
        }

        private async Task<bool> CheckValidUser(string email, int otp)
        {
            try
            {
                var response = await _dynamoDBContext.LoadAsync<User>(email);

                if (response == null || response.Otp != otp)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occueed with ex message: {ex.Message}");
                return false;
            }
        }
    }
}

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Runtime.Internal;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LoginHandler
{
    public class Function
    {
        private IAmazonSimpleNotificationService _snsClient;
        private IDynamoDBContext _dynamoDBContext;
        private static string topicArn = Environment.GetEnvironmentVariable("topicARN");
        private static Dictionary<string, string> headers = new Dictionary<string, string>
        {
            { "Access-Control-Allow-Headers", "Content-Type" },
            { "Access-Control-Allow-Origin",  "*" },
            { "Access-Control-Allow-Methods", "OPTIONS,POST" }
        };
        public Function(IAmazonSimpleNotificationService snsClient, IDynamoDBContext dBContext)
        {
            _dynamoDBContext = dBContext;
            _snsClient = snsClient;
        }

        [LambdaFunction]
        public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest input, ILambdaContext context)
        {
            var otp = GenerateOtp();
            var requestBody = JsonSerializer.Deserialize<User>(input.Body);
            var savedUser = await AddEmailToCollection(requestBody.Email, otp);
            if (!savedUser)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Body = "Something went wrong while saving user",
                    Headers = headers
                };
            }
            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Headers = headers
            };
        }

        private static int GenerateOtp()
        {
            Random random = new Random();
            return random.Next(1000000, 10000000);
        }

        private async Task<bool> AddEmailToCollection(string email, int otp)
        {
            try
            {
                var response = await _dynamoDBContext.LoadAsync<User>(email);

                if (response == null)
                {

                    User user = new User
                    {
                        Email = email,
                    };

                    await _dynamoDBContext.SaveAsync<User>(user);
                    var resp = await subscribeToLoginTopic(email);

                    return resp;
                }

                else
                {
                    response.Otp = otp;
                    await _dynamoDBContext.SaveAsync<User>(response);
                    var resp = await pushMessageToTopic(email, otp);
                    return resp;
                }
            } catch (Exception ex)
            {
                Console.WriteLine($"Exception occueed with ex message: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> subscribeToLoginTopic(string email)
        {
            try
            {
                var filterPolicy = new Dictionary<string, List<string>>()
                {
                    { "email", new List<string> { email } },
                };

                SubscribeRequest request = new SubscribeRequest()
                {
                    TopicArn = topicArn,
                    ReturnSubscriptionArn = true,
                    Protocol = "email",
                    Endpoint = email,
                    Attributes = new Dictionary<string, string> {
                    { "FilterPolicy", JsonSerializer.Serialize(filterPolicy) }
                    }
                };

                var response = await _snsClient.SubscribeAsync(request);
                var subscriptionArn = response.SubscriptionArn;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occueed with ex message: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> pushMessageToTopic(string  email, int otp)
        {
            try
            {
                var messageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {
                        "email", new MessageAttributeValue
                        {
                            DataType = "String",
                            StringValue = email
                        }
                    }
                };
                var snsMessage = new PublishRequest
                {
                    TopicArn = topicArn,
                    Subject = "ManageWithMe Sign-In verification",
                    Message = $"Please use this OTP to sign in into your ManageWithMe account \n {otp}",
                    MessageAttributes = messageAttributes
                };

                var response = await _snsClient.PublishAsync(snsMessage);

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

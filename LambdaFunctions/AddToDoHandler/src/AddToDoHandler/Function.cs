using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SimpleNotificationService;
using System.Net;
using Amazon.Scheduler;
using Amazon.Scheduler.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AddToDoHandler
{
    public class Function
    {
        private static string bucketName = Environment.GetEnvironmentVariable("bucketName");
        private IDynamoDBContext dynamoDBContext;
        private IAmazonS3 s3Client;
        private IAmazonSimpleNotificationService _snsClient;
        private IAmazonScheduler _amazonScheduler;

        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public Function(IDynamoDBContext dBContext, IAmazonS3 amazonS3, IAmazonSimpleNotificationService snsClient, IAmazonScheduler amazonScheduler)
        {
            this.dynamoDBContext = dBContext;
            this.s3Client = amazonS3;
            this._snsClient = snsClient;
            _amazonScheduler = amazonScheduler;
        }

        /// <summary>
        /// A function to add a todo to dynamo db
        /// </summary>
        /// <param name="input">The event for the Lambda function handler to process.</param>
        /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
        /// <returns></returns>
        [LambdaFunction]
        public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(TodoRequest inputBody, ILambdaContext context)
        {
            var todoId = Guid.NewGuid().ToString();

            var todo = new ToDo
            {
                EmailId = inputBody.Email,
                Title = inputBody.Title,
                Notes = inputBody.Notes,
                TodoId = todoId,
                HasFiles = inputBody.File != null,
                DateCreated = DateTime.Now,
                DueOn = inputBody.DueOn,
            };
            var apiResponse = new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = null,

            };

            try
            {
                await this.addTodo(todo);
                await this.addFile(inputBody.File, todoId);
                await ScheduleMessageToEventBridge(todo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured {ex}");
                apiResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
                apiResponse.Body = ex.Message;
            }

            return apiResponse;
        }

        private async Task addTodo(ToDo todo)
        {
            await this.dynamoDBContext.SaveAsync<ToDo>(todo);
        }

        private async Task addFile(ToDoFile fileRequest, string todoId)
        {
            if (fileRequest != null)
            {
                Console.WriteLine("Adding item to bucket");
                using (var inputStream = new MemoryStream(Convert.FromBase64String(fileRequest.FileB64)))
                {
                    await s3Client.PutObjectAsync(new PutObjectRequest
                    {
                        InputStream = inputStream,
                        BucketName = bucketName,
                        Key = todoId + "/" + fileRequest.FileName,
                    });
                }
            }
        }

        private async Task<bool> ScheduleMessageToEventBridge(ToDo todo)
        {
            string topicArn = Environment.GetEnvironmentVariable("topicARN");

            try
            {
                var scheduleTimeWindow = new FlexibleTimeWindow
                {
                    Mode = FlexibleTimeWindowMode.OFF
                };


                var input = new
                {
                    Message = $"You have an upcoming task {todo.Title} in 1 hr, please open ManageWithMe for more details",
                    MessageAttributes = new
                    {
                        email = new
                        {
                            DataType = "String",
                            StringValue = todo.EmailId
                        },
                    },
                    TopicArn = topicArn,
                    Subject = "ManageWithMe Reminder"
                };

                Console.WriteLine($"{{\"Message\":\"You have an upcoming task {todo.Title} in 1 hr, please open ManageWithMe for more details\",\"MessageAttributes\":{{\"email\":{{\"DataType\":\"String\",\"StringValue\":\"{todo.EmailId}\"}}}},\"TopicArn\":\"arn:aws:sns:us-east-1:295826444260:LoginTopic\",\"Subject\":\"ManageWithMe Reminder\"}}");

                var invokeSNSTarget = new Amazon.Scheduler.Model.Target
                {
                    Arn = "arn:aws:scheduler:::aws-sdk:sns:publish",
                    RoleArn = "arn:aws:iam::295826444260:role/LabRole",
                    Input = $"{{\"Message\":\"You have an upcoming task {todo.Title} in 1 hr, please open ManageWithMe for more details\",\"MessageAttributes\":{{\"email\":{{\"DataType\":\"String\",\"StringValue\":\"{todo.EmailId}\"}}}},\"TopicArn\":\"{topicArn}\",\"Subject\":\"ManageWithMe Reminder\"}}"
                };

                var createScheduleRequest = new CreateScheduleRequest
                {
                    Name = todo.TodoId,
                    ScheduleExpression = $"at({todo.DueOn.AddHours(-1).ToString("yyyy-MM-ddTHH:mm:ss")})",
                    State = ScheduleState.ENABLED,
                    Description = "Demo description",
                    FlexibleTimeWindow = scheduleTimeWindow,
                    Target = invokeSNSTarget,
                    ScheduleExpressionTimezone = "America/Halifax",
                    ActionAfterCompletion = ActionAfterCompletion.DELETE
                };

                var createScheduleResponse = await _amazonScheduler.CreateScheduleAsync(createScheduleRequest);
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
    }
}

using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Scheduler;
using Amazon.Scheduler.Model;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace CompleteTodoHandler
{
    public class Function
    {
        private IDynamoDBContext dynamoDBContext;
        private IAmazonScheduler _amazonScheduler;
        private static Dictionary<string, string> headers = new Dictionary<string, string>
        {
            { "Access-Control-Allow-Headers", "Content-Type" },
            { "Access-Control-Allow-Origin",  "*" },
            { "Access-Control-Allow-Methods", "OPTIONS,PATCH" }
        };

        public Function(IDynamoDBContext dBContext, IAmazonScheduler amazonScheduler)
        {
            this.dynamoDBContext = dBContext;
            this._amazonScheduler = amazonScheduler;
        }

        [LambdaFunction]
        public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyResponse request, ILambdaContext context)
        {
            var apiResponse = new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Headers = headers
            };

            try
            {
                var input = JsonSerializer.Deserialize<CompleteTodoRequest>(request.Body);
                if (input.Email != null && input.TodoIds.Count() > 0)
                {
                    var user = await this.dynamoDBContext.LoadAsync<User>(input.Email);
                    if (user == null || user.Otp != input.Token)
                    {
                        apiResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
                    }
                    else
                    {
                        var batchWrite = this.dynamoDBContext.CreateBatchWrite<ToDo>();
                        foreach (var todoId in input.TodoIds)
                        {
                            // Load the current todo
                            var todo = await this.dynamoDBContext.LoadAsync<ToDo>(input.Email, todoId);
                            if (todo != null)
                            {
                                // Update the property
                                todo.IsCompleted = true;
                                // Add the updated todo to the batch
                                batchWrite.AddPutItem(todo);
                            }
                            else
                            {
                                // if todo not present for current email id it means it is malicious todo, remove from list
                                input.TodoIds.Remove(todoId);
                            }

                            // Execute the batch write
                            await batchWrite.ExecuteAsync();
                        }

                        var result = await this.RemoveTodoEvents(input.TodoIds);
                        if (!result)
                        {
                            apiResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
                            apiResponse.Body = "Something went wrong";
                        }
                    }
                }
                else
                {
                    apiResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    apiResponse.Body = "Invalid request";
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                apiResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            return apiResponse;
        }

        private async Task<bool> RemoveTodoEvents(List<string> todoIds)
        {
            try {
                foreach (var todoId in todoIds) {
                    try
                    {
                        var deleteRequest = new DeleteScheduleRequest
                        {
                            Name = todoId
                        };

                        await _amazonScheduler.DeleteScheduleAsync(deleteRequest);
                    }
                    catch(ResourceNotFoundException ex)
                    {
                        Console.WriteLine($"Todo event with id {todoId} not found {ex.Message}");
                        continue;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured while deleting todo {ex.Message}");
                return false;
            }
        }
    }
}

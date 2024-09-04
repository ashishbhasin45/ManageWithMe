using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Runtime.Internal;
using System.Net;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace GetTodoHandler
{
    public class Function
    {
        private IDynamoDBContext dynamoDBContext;
        private static Dictionary<string, string> headers = new Dictionary<string, string>
        {
            { "Access-Control-Allow-Headers", "Content-Type" },
            { "Access-Control-Allow-Origin",  "*" },
            { "Access-Control-Allow-Methods", "OPTIONS,GET" }
        };
        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public Function(IDynamoDBContext dBContext)
        {
            this.dynamoDBContext = dBContext;
        }

        /// <summary>
        /// Function to get the todo for user
        /// </summary>
        /// <param name="request">The event for the Lambda function handler to process.</param>
        /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
        /// <returns></returns>
        [LambdaFunction]
        public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            var apiRespone = new APIGatewayHttpApiV2ProxyResponse
            {
                Headers = headers
            };

            try
            {
                IDictionary<string, string> queryParams = request.QueryStringParameters;
                var email = queryParams["email"];
                var month = Convert.ToInt16(queryParams["month"]);
                var year = Convert.ToInt16(queryParams["year"]);
                var todos = await this.dynamoDBContext.QueryAsync<ToDo>(email).GetRemainingAsync();
                var filteredTodos = todos.Where(t => t.DueOn.Month == month && t.DueOn.Year == year);
                var resp = new TodoResponse
                {
                    Email = email,
                    Events = filteredTodos.GroupBy(t => t.DueOn.Date).Select(x => new Event
                    {
                        Date = x.Key,
                        Todos = x.ToList()
                    }).ToList()
                };


                apiRespone.StatusCode = (int)HttpStatusCode.OK;
                apiRespone.Body = JsonSerializer.Serialize(resp);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                apiRespone.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            return apiRespone;
        }
    }
}

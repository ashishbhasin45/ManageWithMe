using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Amazon.Lambda.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace GetTodoHandler
{
    [LambdaStartup]
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddAWSService<Amazon.S3.IAmazonS3>();

            var dbClient = new AmazonDynamoDBClient();
            services.AddSingleton<IAmazonDynamoDB>(dbClient);
            services.AddSingleton<IDynamoDBContext, DynamoDBContext>();
        }
    }
}

using Amazon.Lambda.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Amazon.S3;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;

namespace TodoFileHandler
{
    [LambdaStartup]
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();
            services.AddSingleton<IDynamoDBContext, DynamoDBContext>();
            services.AddSingleton<IAmazonS3,  AmazonS3Client>();
        }
    }
}

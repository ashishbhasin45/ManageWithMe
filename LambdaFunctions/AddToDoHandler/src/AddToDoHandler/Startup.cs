using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Annotations;
using Amazon.S3;
using Amazon.Scheduler;
using Amazon.SimpleNotificationService;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.Design;

namespace AddToDoHandler
{
    [LambdaStartup]
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var dbClient = new AmazonDynamoDBClient();
            services.AddSingleton<IAmazonDynamoDB>(dbClient);
            services.AddSingleton<IDynamoDBContext, DynamoDBContext>();

            services.AddSingleton<IAmazonS3, AmazonS3Client>();

            services.AddSingleton<IAmazonSimpleNotificationService>(sp =>
            {
                var config = new AmazonSimpleNotificationServiceConfig
                {
                    RegionEndpoint = RegionEndpoint.USEast1
                };
                return new AmazonSimpleNotificationServiceClient(config);
            });

            services.AddSingleton<IAmazonScheduler, AmazonSchedulerClient>();

            //services.AddSingleton<IAmazonEventBridge>(sp =>
            //{
            //    var config = new AmazonEventBridgeConfig { RegionEndpoint = RegionEndpoint.USEast1 };
            //    return new AmazonEventBridgeClient(config);
            //});
        }
    }
}

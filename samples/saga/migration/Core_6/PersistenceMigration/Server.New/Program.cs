//#define POST_MIGRATION

using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Persistence;
using NServiceBus.Persistence.Sql;
using NServiceBus.Routing;
using NServiceBus.Transport;

class Program
{
    static void Main(string[] args)
    {
        Start().GetAwaiter().GetResult();
    }

    static async Task Start()
    {
        Console.Title = "Samples.SagaMigration.Server.New";
        var config = new EndpointConfiguration("Samples.SagaMigration.Server");

#if !POST_MIGRATION
        config.OverrideLocalAddress("Samples.SagaMigration.Server.New");
#endif

        var persistence = config.UsePersistence<SqlPersistence>();
        persistence.ConnectionBuilder(() =>
        {
            var connection = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=nservicebus;Integrated Security=True;");
            return connection;
        });
        persistence.TablePrefix("New");
        config.SendFailedMessagesTo("error");

        var endpoint = await Endpoint.Start(config).ConfigureAwait(false);

        Console.WriteLine("Press <enter> to exit.");
        Console.ReadLine();

        await endpoint.Stop().ConfigureAwait(false);
    }
}

#if POST_MIGRATION
public class DrainTempQueueSatelliteFeature : Feature
{
    public DrainTempQueueSatelliteFeature()
    {
        EnableByDefault();
    }

    protected override void Setup(FeatureConfigurationContext context)
    {
        #region DrainTempQueueSatellite

        var tempQueue = context.Settings.GetTransportAddress(context.Settings.LogicalAddress().CreateQualifiedAddress("New"));
        var mainQueue = context.Settings.LocalAddress();

        context.AddSatelliteReceiver("DrainTempQueueSatellite", tempQueue, new PushRuntimeSettings(maxConcurrency: 1),
            (config, errorContext) => RecoverabilityAction.MoveToError(config.Failed.ErrorQueue),
            (builder, messageContext) =>
            {
                var messageDispatcher = builder.Build<IDispatchMessages>();
                var outgoingHeaders = messageContext.Headers;

                var outgoingMessage = new OutgoingMessage(messageContext.MessageId, outgoingHeaders, messageContext.Body);
                var outgoingOperation = new TransportOperation(outgoingMessage, new UnicastAddressTag(mainQueue));
                
                Console.WriteLine($"Moving message from {tempQueue} to {mainQueue}");

                return messageDispatcher.Dispatch(new TransportOperations(outgoingOperation), messageContext.TransportTransaction, messageContext.Context);
            });
#endregion
    }
}
#endif
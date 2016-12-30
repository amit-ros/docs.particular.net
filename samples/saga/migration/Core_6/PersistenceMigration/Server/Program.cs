using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Persistence;
using NServiceBus.Transport;

class Program
{
    static void Main(string[] args)
    {
        Start().GetAwaiter().GetResult();
    }

    static async Task Start()
    {
        Console.Title = "Samples.SagaMigration.Server";

        var config = new EndpointConfiguration("Samples.SagaMigration.Server");
        config.UsePersistence<NHibernatePersistence>().ConnectionString(@"Data Source=.\SQLEXPRESS;Initial Catalog=nservicebus;Integrated Security=True;");
        config.SendFailedMessagesTo("error");

        var endpoint = await Endpoint.Start(config).ConfigureAwait(false);

        Console.WriteLine("Press <enter> to exit.");
        Console.ReadLine();

        await endpoint.Stop().ConfigureAwait(false);
    }
}
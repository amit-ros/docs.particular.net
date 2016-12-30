using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Persistence;
using NServiceBus.Persistence.Sql;

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
        config.OverrideLocalAddress("Samples.SagaMigration.Server.New");
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

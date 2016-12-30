#region Toggle

//#define MIGRATION

#endregion

using System;
using System.Threading.Tasks;
using NServiceBus;

#region Header

#if NEW
[NServiceBus.Persistence.Sql.SqlSaga(correlationProperty: "SomeId")]
#endif
public class TestSaga : Saga<TestSaga.TestSagaData>,
    IHandleMessages<ReplyFollowUpMessage>,
    IHandleMessages<CorrelatedMessage>,
#if MIGRATION && !NEW
    IHandleMessages<StartingMessage>,
    IAmStartedByMessages<DummyMessage>
#else
    IAmStartedByMessages<StartingMessage>
#endif

#endregion

{
    #region DummyHandler

#if MIGRATION && !NEW
    public Task Handle(DummyMessage message, IMessageHandlerContext context)
    {
        throw new Exception("Dummy");
    }
#endif

    #endregion

    protected override void ConfigureHowToFindSaga(SagaPropertyMapper<TestSagaData> mapper)
    {
#region Mappings

        mapper.ConfigureMapping<StartingMessage>(m => m.SomeId).ToSaga(s => s.SomeId);
        mapper.ConfigureMapping<CorrelatedMessage>(m => m.SomeId).ToSaga(s => s.SomeId);
#if MIGRATION && !NEW
        mapper.ConfigureMapping<DummyMessage>(m => m.SomeId).ToSaga(s => s.SomeId);
#endif

#endregion
    }

#region Handlers
    public Task Handle(StartingMessage message, IMessageHandlerContext context)
    {
        Console.WriteLine($"{Data.SomeId}: Created new saga instance.");
        return Task.CompletedTask;
    }

    public Task Handle(ReplyFollowUpMessage message, IMessageHandlerContext context)
    {
        Console.WriteLine($"{Data.SomeId}: Got a follow-up message. Completing.");
        MarkAsComplete();
        return Task.CompletedTask;
    }

    public Task Handle(CorrelatedMessage message, IMessageHandlerContext context)
    {
        Console.WriteLine($"{Data.SomeId}: Got a correlated message {message.SomeId}. Replying back.");
        return context.Reply(new ReplyMessage
        {
            SomeId = Data.SomeId
        });
    }
#endregion

    public class TestSagaData : ContainSagaData
    {
        public virtual string SomeId { get; set; }
    }
}
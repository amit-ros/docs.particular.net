﻿using System;
using System.Threading.Tasks;
using NServiceBus;

public class MessageHandler : IHandleMessages<MyMessage>
{
    public Task Handle(MyMessage message, IMessageHandlerContext context)
    {
        Console.WriteLine("Hello from Instance 1");
        return Task.FromResult(0);
    }

}
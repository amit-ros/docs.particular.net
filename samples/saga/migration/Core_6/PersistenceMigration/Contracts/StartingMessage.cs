using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus;

public class StartingMessage : IMessage
{
    public string SomeId { get; set; }
}
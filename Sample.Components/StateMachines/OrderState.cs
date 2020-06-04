using System;
using Automatonymous;
using MassTransit.MongoDbIntegration.Saga;
using MongoDB.Bson.Serialization.Attributes;

namespace Sample.Components.StateMachines
{
    public class OrderState :
        SagaStateMachineInstance,
        IVersionedSaga
    {
        [BsonId] public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; } // current state of the saga

        // data to be stored
        public string CustomerNumber { get; set; }
        public DateTime Updated { get; set; }
        public DateTime? SubmitDate { get; set; }
        public int Version { get; set; }
    }
}

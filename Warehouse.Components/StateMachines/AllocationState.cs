using System;
using Automatonymous;
using MassTransit.MongoDbIntegration.Saga;
using MongoDB.Bson.Serialization.Attributes;

namespace Warehouse.Components.StateMachines
{
    public sealed class AllocationState :
        SagaStateMachineInstance,
        IVersionedSaga
    {
        [BsonId] public Guid CorrelationId { get; set; }
        public int Version { get; set; }

        public string CurrentState { get; set; }

        public Guid? HoldDurationToken { get; set; }
    }
}
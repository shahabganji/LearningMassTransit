using System;
using Automatonymous;
using MassTransit;
using MassTransit.MongoDbIntegration.Saga;
using MongoDB.Bson.Serialization.Attributes;
using Warehouse.Contracts.Events;

namespace Warehouse.Components.StateMachines
{
    public sealed class AllocationStateMachine : 
        MassTransitStateMachine<AllocationState>
    {
        public AllocationStateMachine()
        {
            InstanceState(x => x.CurrentState);
            Event(() => AllocationCreated , x=> x.CorrelateById(m => m.Message.AllocationId) );
            Schedule(() => HoldExpiration , state => state.HoldDurationToken , holdDurationCancellationToken =>
            {
                holdDurationCancellationToken.Delay = TimeSpan.FromHours(1);

                // a schedule has an event, Received which is ofType<AllocationHoldDurationExpired>,
                // in it so no need to define it in that state machine again
                // can be configured just like the way for regular events, Event(() => eventName, ....)
                holdDurationCancellationToken.Received = x => x.CorrelateById(m => m.Message.AllocationId);
            });
            
            Initially(
                When(AllocationCreated)
                    // set the schedule, we need to handle its event later on 
                    .Schedule(
                        HoldExpiration,
                        // create a message
                        context => context.Init<AllocationHoldDurationExpiredEvent>(
                            new {context.Data.AllocationId}),
                        
                        sendContext => sendContext.Data.HoldDuration )
                    .TransitionTo(Allocated)
                );
            
            During(Allocated, 
                // handle the event of the HoldExpiration schedule
                When(HoldExpiration.Received)
                    .ThenAsync(
                        context=> Console.Out.WriteLineAsync($"Allocation was released: {context.Data.AllocationId}"))
                    .Finalize());
            
            SetCompletedWhenFinalized();

        }

        // the second generic parameter is the event type for the schedule
        public Schedule<AllocationState, AllocationHoldDurationExpiredEvent> HoldExpiration { get; set; }
        
        public State Allocated { get; set; }
        
        public Event<AllocationCreatedEvent> AllocationCreated { get; set; } 
        
    }


    public sealed class AllocationState :
        SagaStateMachineInstance,
        IVersionedSaga
    {
        [BsonId]
        public Guid CorrelationId { get; set; }
        public int Version { get; set; }
        
        public string CurrentState { get; set; }
        
        public Guid? HoldDurationToken { get; set; }
    }
}

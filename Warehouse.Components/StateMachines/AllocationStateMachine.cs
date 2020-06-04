using System;
using Automatonymous;
using MassTransit;
using Warehouse.Contracts.Events;

namespace Warehouse.Components.StateMachines
{
    public sealed class AllocationStateMachine :
        MassTransitStateMachine<AllocationState>
    {
        public AllocationStateMachine()
        {
            InstanceState(x => x.CurrentState);
            Event(() => AllocationCreated, x => x.CorrelateById(m => m.Message.AllocationId));
            Event(() => ReleaseRequested, x => x.CorrelateById(m => m.Message.AllocationId));
            Schedule(() => HoldExpiration, state => state.HoldDurationToken, holdDurationCancellationToken =>
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
                        delayProvider: sendContext => sendContext.Data.HoldDuration)
                    .TransitionTo(Allocated),
                When(ReleaseRequested)
                    .TransitionTo(Released)
            );

            During(Allocated, // the message is delivered the second time due to failures in broker being down 
                When(AllocationCreated)
                    .Schedule(
                        HoldExpiration,
                        // create a message
                        context => context.Init<AllocationHoldDurationExpiredEvent>(
                            new {context.Data.AllocationId}),
                        delayProvider: sendContext => sendContext.Data.HoldDuration));

            During(Released,
                When(AllocationCreated)
                    .ThenAsync(
                        context => Console.Out.WriteLineAsync(
                            $"Allocation is already released: {context.Data.AllocationId}"))
                    .Finalize()
            );

            During(Allocated,
                // handles the event of the HoldExpiration schedule
                When(HoldExpiration.Received)
                    .ThenAsync(
                        context => Console.Out.WriteLineAsync($"Allocation expired: {context.Data.AllocationId}"))
                    .Finalize(),
                When(ReleaseRequested)
                    .Unschedule(HoldExpiration)
                    .ThenAsync(
                        context => Console.Out.WriteLineAsync(
                            $"Allocation release request, granted: {context.Data.AllocationId}"))
                    .Finalize()
            );

            SetCompletedWhenFinalized();
        }


        // the second generic parameter is the event type for the schedule
        public Schedule<AllocationState, AllocationHoldDurationExpiredEvent> HoldExpiration { get; set; }

        public State Allocated { get; set; }
        public State Released { get; set; }

        public Event<AllocationCreatedEvent> AllocationCreated { get; set; }
        public Event<AllocationReleaseRequestedEvent> ReleaseRequested { get; set; }
    }
}

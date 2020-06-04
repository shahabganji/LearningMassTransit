using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Courier;
using MassTransit.Definition;
using Sample.Components.CourierActivities.Arguments;
using Sample.Components.CourierActivities.Logs;

namespace Sample.Components.CourierActivities
{
    public sealed class PaymentActivity
        : IActivity<PaymentArguments, PaymentLog>
    {
        public async Task<ExecutionResult> Execute(ExecuteContext<PaymentArguments> context)
        {
            string cardNumber = context.Arguments.CardNumber;
            if (string.IsNullOrEmpty(cardNumber))
                throw new ArgumentNullException(nameof(cardNumber));

            // to give the AllocationState saga to have chance to schedule HolDurationExpired event
            // now need to do an Unschedule  when in allocation released state
            await Task.Delay(5000);
            // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            if (cardNumber.StartsWith("5999"))
                throw new InvalidOperationException($"The card number was invalid: {cardNumber}");

            await Task.Delay(200);

            return context.Completed<PaymentLog>(new {AuthorizatioCode = "77777"});
        }

        public async Task<CompensationResult> Compensate(CompensateContext<PaymentLog> context)
        {
            await Task.Delay(100);
            return context.Compensated();
        }
    }


    public sealed class PaymentActivityDefinition :
        ActivityDefinition<PaymentActivity, PaymentArguments, PaymentLog>
    {
        protected override void ConfigureExecuteActivity(IReceiveEndpointConfigurator endpointConfigurator,
            IExecuteActivityConfigurator<PaymentActivity, PaymentArguments> executeActivityConfigurator)
        {
            base.ConfigureExecuteActivity(endpointConfigurator, executeActivityConfigurator);
        }

        protected override void ConfigureCompensateActivity(IReceiveEndpointConfigurator endpointConfigurator,
            ICompensateActivityConfigurator<PaymentActivity, PaymentLog> compensateActivityConfigurator)
        {
            base.ConfigureCompensateActivity(endpointConfigurator, compensateActivityConfigurator);
        }
    }
}

using GreenPipes;
using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.Definition;

namespace Sample.Components.Consumers
{
    public class SubmitOrderConsumerDefinition
        : ConsumerDefinition<SubmitOrderConsumer>
    {
        // public SubmitOrderConsumerDefinition()
        // {
        //     EndpointDefinition = new ConsumerEndpointDefinition<SubmitOrderConsumer>(new EndpointSettings<IEndpointDefinition<SubmitOrderConsumer>>
        //     {
        //         IsTemporary = true, 
        //         Name =$"{KebabCaseEndpointNameFormatter.Instance.Consumer<SubmitOrderConsumer>()}{Guid.NewGuid()}" ,
        //     });
        // }
        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<SubmitOrderConsumer> consumerConfigurator)
        {
            endpointConfigurator.UseInMemoryOutbox();
            endpointConfigurator.UseMessageRetry(r => r.Interval(3, 1000));
        }
    }
}

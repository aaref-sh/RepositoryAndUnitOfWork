using EasyNetQ;

namespace Helper.MessageBroker;


public class MessageBrokerService
{
    public readonly static IBus bus = RabbitHutch.CreateBus(Environment.GetEnvironmentVariable(""));

    public static bool RunningInContainer => (Environment.GetEnvironmentVariable("CUSTOMCONNSTR_RABBITMQ") ?? "").Split(";").Length > 2;

    public static void VoidCall(string functionName, object? parameter)
    {
        bus.SendReceive.Send(functionName, parameter);
    }
}

namespace RunRabbitRun.Net
{
    public interface IQueueNameBuilder
    {
        string Build(string queue);
    }
}
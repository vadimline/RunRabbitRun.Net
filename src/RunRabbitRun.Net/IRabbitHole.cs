namespace RunRabbitRun.Net
{
    public interface IRabbitHole
    {
        void Send(Envelope envelope);
        
        void Send(string exchange, string routingKey, string body);
    }
}
namespace RunRabbitRun.Net.Test
{
    public class QeueuNameBuilder : IQueueNameBuilder
    {
        private string _runtimePart;

        public QeueuNameBuilder(string runtimePart)
        {
            this._runtimePart = runtimePart;
        }

        public string Build(string queue)
        {
            return string.Format(queue, this._runtimePart);
        }
    }
}
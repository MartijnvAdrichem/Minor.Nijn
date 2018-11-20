namespace Minor.Nijn
{
    public interface IEventMessage
    {
        string CorrelationId { get; }
        string EventType { get; }
        string Message { get; }
        string RoutingKey { get; }
        long Timestamp { get; }

        byte[] EncodeMessage();
    }
}

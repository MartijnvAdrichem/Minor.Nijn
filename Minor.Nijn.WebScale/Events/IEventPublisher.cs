namespace Minor.Nijn.WebScale
{
    public interface IEventPublisher
    {
        void Publish(DomainEvent domainEvent);
    }
}

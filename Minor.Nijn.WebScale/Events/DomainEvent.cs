using System;
using Newtonsoft.Json;

namespace Minor.Nijn.WebScale.Events
{
    /// <summary>
    ///     Base class for all domain events.
    /// </summary>
    public abstract class DomainEvent
    {
        /// <summary>
        ///     Creates a domain event by setting the routing key and generating a timestamp.
        /// </summary>
        /// <param name="routingKey">The routing key should be of the format domain.eventname</param>
        public DomainEvent(string routingKey)
        {
            RoutingKey = routingKey;
            Timestamp = DateTime.Now.Ticks;
            ID = Guid.NewGuid();
        }

        /// <summary>
        ///     The Routing Key is used by the underlying protocol to route events to subscribers
        /// </summary>
        [JsonProperty]
        public string RoutingKey { get; private set; }

        /// <summary>
        ///     The Timestamp is set to the creation time of the domain event.
        /// </summary>
        [JsonProperty]
        public long Timestamp { get; private set; }

        /// <summary>
        ///     The ID uniquely identifies the domain event.
        /// </summary>
        [JsonProperty]
        public Guid ID { get; private set; }
    }
}
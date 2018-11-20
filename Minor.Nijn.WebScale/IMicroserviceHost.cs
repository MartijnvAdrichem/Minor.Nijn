using System;

namespace Minor.Nijn.WebScale
{
    public interface IMicroserviceHost : IDisposable
    {
        IServiceProvider Provider { get; }

        object CreateInstanceOfType(Type type);
        void StartListening();
    }
}

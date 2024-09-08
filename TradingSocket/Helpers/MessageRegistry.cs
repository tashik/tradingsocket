using System.Collections.Concurrent;
using TradingSocket.Entities;

namespace TradingSocket.Helpers;

public class MessageRegistry
{
    private readonly ConcurrentDictionary<int, SocketRequest> _registry = new();

    public void RegisterRequest(int messageId, SocketRequest metadata)
    {
        _registry[messageId] = metadata;
    }

    public bool TryGetRequestType(int messageId, out SocketRequest metadata)
    {
        return _registry.TryGetValue(messageId, out metadata);
    }

    public void RemoveRequest(int messageId)
    {
        _registry.TryRemove(messageId, out _);
    }
}
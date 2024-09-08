using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json.Linq;
using Serilog;
using TradingSocket.Entities;
using TradingSocket.Events;
using TradingSocket.Helpers;

namespace TradingSocket;

public abstract class TradingSocketClientAbstract : ITradingSocketClient
{
    private ClientWebSocket _webSocket;
    protected readonly string _clientId;
    protected readonly string _clientSecret;
    private readonly string _wsUrl;
    private string? _accessToken;
    private const int ReconnectDelayMs = 5000;
    private readonly CancellationTokenSource _connectionCancellationTokenSource;
    private CancellationTokenSource _receivingCancellationTokenSource;
    
    private readonly TradingSocketEventDispatcher _eventDispatcher;
    protected readonly MessageIndexer _msgIndexer;

    protected readonly MessageRegistry _messageRegistry;

    public TradingSocketClientAbstract(ConnectionConfig connectionConfig, TradingSocketEventDispatcher eventDispatcher, MessageRegistry messageRegistry, MessageIndexer indexer)
    {
        _eventDispatcher = eventDispatcher;
        _msgIndexer = indexer;
        _messageRegistry = messageRegistry;
        _webSocket = new ClientWebSocket();
        _clientId = connectionConfig.ClientId;
        _clientSecret = connectionConfig.ClientSecret;
        _wsUrl = connectionConfig.WsUrl;
        _connectionCancellationTokenSource = new CancellationTokenSource();
        _receivingCancellationTokenSource = new CancellationTokenSource();
    }

    protected abstract string GetClientName();
    protected abstract bool IsAccessTokenRequired();

    public async Task ConnectAsync(CancellationToken externalToken)
    {
        var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            _connectionCancellationTokenSource.Token,
            externalToken
        );
        var uri = new Uri(_wsUrl);
        var cancellationToken = linkedTokenSource.Token;
        while (!linkedTokenSource.Token.IsCancellationRequested && _webSocket.State != WebSocketState.Open)
        {
            try
            {
                await _webSocket.ConnectAsync(uri, cancellationToken);
                StartReceiving();
                Log.Information("Connected to " + GetClientName() + " WebSocket API.");
                if (IsAccessTokenRequired())
                {
                    await AuthenticateAsync(cancellationToken);
                }
            }
            catch (Exception ex) when (ex is OperationCanceledException or TaskCanceledException)
            {
                Log.Warning("Connection attempt canceled.");
                throw;
            }
            catch (Exception ex)
            {
                if (linkedTokenSource.Token.IsCancellationRequested)
                {
                    Log.Warning("Connection attempt canceled.");
                    throw new OperationCanceledException(cancellationToken);
                }

                Log.Warning($"Connection failed: {ex.Message}. Retrying in {ReconnectDelayMs / 1000} seconds...");
                await Task.Delay(ReconnectDelayMs, cancellationToken);
            }
        }
    }
    public abstract Task SubscribeToPrivateChannelAsync(string channel, CancellationToken cancellationToken);


    public abstract Task SubscribeToTickerAsync(string instrumentName, CancellationToken cancellationToken);
    
    public async Task CloseAsync(CancellationToken cancellationToken)
    {
        StopReceiving();
        if (_webSocket.State is WebSocketState.Open or WebSocketState.CloseReceived)
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
        }
        Dispose();
    }
    
    
    protected async Task SendAsync(string message, CancellationToken cancellationToken)
    {
        try
        {
            var messageBuffer = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(messageBuffer);
            await _webSocket.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken);
        }
        catch (Exception ex) when (ex is OperationCanceledException || ex is TaskCanceledException)
        {
            Log.Warning("Send attempt canceled.");
            throw;
        }
        catch (Exception ex)
        {
            Log.Error($"Send failed: {ex.Message}");
            // Handle send failure (e.g., reconnect)
        }
    }

    private async Task ReceiveAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[1024 * 4];
        while (_webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Log.Information($"Received: {message}");
                    
                    await ProcessResponseAsync(message);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    Log.Warning("WebSocket connection closed. Attempting to reconnect...");
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancellationToken);
                    await ReconnectAsync(cancellationToken);
                }
            }
            catch (Exception ex) when (ex is OperationCanceledException || ex is TaskCanceledException)
            {
                Log.Warning("Receive operation canceled.");
                throw;
            }
            catch (Exception ex)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Log.Warning("Receive operation canceled.");
                    throw new OperationCanceledException(cancellationToken);
                }

                Log.Error($"Receive failed: {ex.Message}. Attempting to reconnect...");
                await ReconnectAsync(cancellationToken);
            }
        }
    }

    protected abstract Task ProcessResponseAsync(string message);
    
    protected async Task OnNewMessage(JToken? data, string args, int id)
    {
        _messageRegistry.TryGetRequestType(id, out var requestType);
        var jReq = new SocketResponse()
        {
           DataObject = data,
           Args = args,
           RequestType = requestType
        };
       
        await _eventDispatcher.DispatchAsync(new MessageArrivedEvent(jReq));
    }

    private void ResetWebsocket()
    {
        _webSocket.Dispose();
        _webSocket = new ClientWebSocket();
    }

    private async Task ReconnectAsync(CancellationToken cancellationToken)
    {
        ResetWebsocket();
        await EstablishConnection(cancellationToken);
    }

    private async Task EstablishConnection(CancellationToken cancellationToken)
    {
        await ConnectAsync(cancellationToken);
        await AuthenticateAsync(cancellationToken);
        // Re-subscribe to channels as needed
    }

    protected abstract Task AuthenticateAsync(CancellationToken cancellationToken);

    private void StartReceiving()
    {
        _receivingCancellationTokenSource = new CancellationTokenSource();
        _ = Task.Run(() => ReceiveAsync(_receivingCancellationTokenSource.Token));
    }

    private void StopReceiving()
    {
        _receivingCancellationTokenSource.Cancel();
    }

    private void Dispose()
    {
        _connectionCancellationTokenSource?.Cancel();
        ResetWebsocket();
        Log.Information("Socket client stopped");
    }
    
}

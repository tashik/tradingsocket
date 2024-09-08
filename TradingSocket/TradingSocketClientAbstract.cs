using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json.Linq;
using Serilog;
using TradingSocket.Deribit.EventHandlers;
using TradingSocket.Entities;
using TradingSocket.Helpers;

namespace TradingSocket;

public abstract class TradingSocketClientAbstract : ITradingSocketClient
{
    private ClientWebSocket _webSocket;
    protected readonly string ClientId;
    protected readonly string ClientSecret;
    private readonly string _wsUrl;
    protected string? AccessToken;
    private const int ReconnectDelayMs = 5000;
    private readonly CancellationTokenSource _connectionCancellationTokenSource;
    private CancellationTokenSource _receivingCancellationTokenSource;
    
    protected readonly TradingSocketEventDispatcher EventDispatcher;
    protected readonly MessageIndexer MsgIndexer;

    protected readonly MessageRegistry MsgRegistry;

    public TradingSocketClientAbstract(ConnectionConfig connectionConfig, TradingSocketEventDispatcher eventDispatcher, MessageRegistry msgRegistry, MessageIndexer indexer)
    {
        EventDispatcher = eventDispatcher;
        MsgIndexer = indexer;
        MsgRegistry = msgRegistry;
        _webSocket = new ClientWebSocket();
        ClientId = connectionConfig.ClientId;
        ClientSecret = connectionConfig.ClientSecret;
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
                Log.Information("Connected to {ClientName} WebSocket API.", GetClientName());
                if (IsAccessTokenRequired())
                {
                    await AuthenticateAsync(cancellationToken);
                }
            }
            catch (Exception ex) when (ex is OperationCanceledException or TaskCanceledException)
            {
                Log.Warning("{ClientName}: Connection attempt canceled.", GetClientName());
                throw;
            }
            catch (Exception ex)
            {
                if (linkedTokenSource.Token.IsCancellationRequested)
                {
                    Log.Warning("{ClientName}: Connection attempt canceled.", GetClientName());
                    throw new OperationCanceledException(cancellationToken);
                }

                Log.Warning("{ClientName}: Connection failed: {Message}. Retrying in {DelayMs} seconds...", GetClientName(), ex.Message, ReconnectDelayMs / 1000);
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
            Log.Warning("{ClientName}: Send attempt canceled.", GetClientName());
            throw;
        }
        catch (Exception ex)
        {
            Log.Error("{ClientName}: Send failed: {Message}", GetClientName(), ex.Message);
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
                    Log.Information("{ClientName}: Received: {Message}", GetClientName(), message);
                    
                    await ProcessResponseAsync(message);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    Log.Warning("WebSocket {ClientName} connection closed. Attempting to reconnect...", GetClientName());
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancellationToken);
                    await ReconnectAsync(cancellationToken);
                }
            }
            catch (Exception ex) when (ex is OperationCanceledException || ex is TaskCanceledException)
            {
                Log.Warning("{ClientName}: Receive operation canceled.", GetClientName());
                throw;
            }
            catch (Exception ex)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Log.Warning("{ClientName}: Receive operation canceled.", GetClientName());
                    throw new OperationCanceledException(cancellationToken);
                }

                Log.Error("{ClientName} Receive failed: {Message}. Attempting to reconnect...", GetClientName(), ex.Message);
                await ReconnectAsync(cancellationToken);
            }
        }
    }

    protected abstract Task ProcessResponseAsync(string message);

    protected abstract Task OnNewMessage(JToken? data, string args, int id);

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
        Log.Information("Socket {ClientName} client stopped", GetClientName());
    }
    
}

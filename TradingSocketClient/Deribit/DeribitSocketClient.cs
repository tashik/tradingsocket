using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json.Linq;
using TradingSocketClient.Deribit.Entities;
using TradingSocketClient.Deribit.Factories;

namespace TradingSocketClient.Deribit;

public class DeribitSocketClient : AbstractTradingSocket
{
    private ClientWebSocket _webSocket;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private string _accessToken = "";
    private const int ReconnectDelayMs = 5000;
    private CancellationTokenSource _connectionCancellationTokenSource;
    private CancellationTokenSource _receivingCancellationTokenSource;

    public DeribitSocketClient(string clientId, string clientSecret)
    {
        _webSocket = new ClientWebSocket();
        _clientId = clientId;
        _clientSecret = clientSecret;
        _connectionCancellationTokenSource = new CancellationTokenSource();
        _receivingCancellationTokenSource = new CancellationTokenSource();
    }

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        var uri = new Uri("wss://www.deribit.com/ws/api/v2");
        while (_webSocket.State != WebSocketState.Open)
        {
            try
            {
                await _webSocket.ConnectAsync(uri, cancellationToken);
                Console.WriteLine("Connected to Deribit WebSocket API.");
            }
            catch (Exception ex) when (ex is OperationCanceledException || ex is TaskCanceledException)
            {
                Console.WriteLine("Connection attempt canceled.");
                throw;
            }
            catch (Exception ex)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine("Connection attempt canceled.");
                    throw new OperationCanceledException(cancellationToken);
                }

                Console.WriteLine($"Connection failed: {ex.Message}. Retrying in {ReconnectDelayMs / 1000} seconds...");
                await Task.Delay(ReconnectDelayMs, cancellationToken);
            }
        }
    }

    public async Task SendAsync(string message, CancellationToken cancellationToken)
    {
        try
        {
            var messageBuffer = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(messageBuffer);
            await _webSocket.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken);
        }
        catch (Exception ex) when (ex is OperationCanceledException || ex is TaskCanceledException)
        {
            Console.WriteLine("Send attempt canceled.");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Send failed: {ex.Message}");
            // Handle send failure (e.g., reconnect)
        }
    }

    public async Task ReceiveAsync(CancellationToken cancellationToken)
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
                    Console.WriteLine($"Received: {message}");

                    // Use the factory to create the appropriate subscription update object
                    var subscriptionUpdate = SubscriptionUpdateFactory.CreateSubscriptionUpdate(message);
                    var data = subscriptionUpdate?.Data;
                    if (data is AccessLogData accessLogData)
                    {
                        Console.WriteLine($"Timestamp: {accessLogData.Timestamp}");
                        Console.WriteLine($"Log: {accessLogData.Log}");
                        Console.WriteLine($"IP: {accessLogData.Ip}");
                        Console.WriteLine($"ID: {accessLogData.Id}");
                        Console.WriteLine($"Country: {accessLogData.Country}");
                        Console.WriteLine($"City: {accessLogData.City}");
                    }
                    // Handle other data types similarly
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine("WebSocket connection closed. Attempting to reconnect...");
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancellationToken);
                    await ReconnectAsync(cancellationToken);
                }
            }
            catch (Exception ex) when (ex is OperationCanceledException || ex is TaskCanceledException)
            {
                Console.WriteLine("Receive operation canceled.");
                throw;
            }
            catch (Exception ex)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine("Receive operation canceled.");
                    throw new OperationCanceledException(cancellationToken);
                }

                Console.WriteLine($"Receive failed: {ex.Message}. Attempting to reconnect...");
                await ReconnectAsync(cancellationToken);
            }
        }
    }

    private async Task ReconnectAsync(CancellationToken cancellationToken)
    {
        _webSocket.Dispose();
        _webSocket = new ClientWebSocket();
        await ConnectAsync(cancellationToken);
        await AuthenticateAsync(cancellationToken);
        // Re-subscribe to channels as needed
    }

    public async Task AuthenticateAsync(CancellationToken cancellationToken)
    {
        var authRequest = new
        {
            jsonrpc = "2.0",
            method = "public/auth",
            @params = new
            {
                grant_type = "client_credentials",
                client_id = _clientId,
                client_secret = _clientSecret
            },
            id = 1
        };

        var message = Newtonsoft.Json.JsonConvert.SerializeObject(authRequest);
        await SendAsync(message, cancellationToken);

        var buffer = new byte[1024 * 4];
        var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
        var response = Encoding.UTF8.GetString(buffer, 0, result.Count);

        var jsonResponse = JObject.Parse(response);
        if (jsonResponse["result"] != null)
        {
            _accessToken = jsonResponse["result"]["access_token"].ToString();
            Console.WriteLine("Authentication successful. Access Token: " + _accessToken);
        }
        else
        {
            throw new Exception("Authentication failed.");
        }
    }

    public async Task SubscribeToPrivateChannelAsync(string channel, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_accessToken))
        {
            throw new InvalidOperationException("Client is not authenticated.");
        }

        var subscribeMessage = new
        {
            jsonrpc = "2.0",
            method = "private/subscribe",
            @params = new
            {
                channels = new[] { channel }
            },
            id = 2
        };

        var message = Newtonsoft.Json.JsonConvert.SerializeObject(subscribeMessage);
        await SendAsync(message, cancellationToken);
    }

    public async Task SubscribeToTickerAsync(string instrumentName, CancellationToken cancellationToken)
    {
        var subscribeMessage = new
        {
            jsonrpc = "2.0",
            method = "public/subscribe",
            @params = new
            {
                channels = new[] { $"ticker.{instrumentName}.raw" }
            },
            id = 3
        };

        var message = Newtonsoft.Json.JsonConvert.SerializeObject(subscribeMessage);
        await SendAsync(message, cancellationToken);
    }

    public void StartReceiving()
    {
        _receivingCancellationTokenSource = new CancellationTokenSource();
        Task.Run(() => ReceiveAsync(_receivingCancellationTokenSource.Token));
    }

    public void StopReceiving()
    {
        _receivingCancellationTokenSource.Cancel();
    }

    public async Task CloseAsync(CancellationToken cancellationToken)
    {
        if (_webSocket.State == WebSocketState.Open || _webSocket.State == WebSocketState.CloseReceived)
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
        }
    }

    public void Dispose()
    {
        _connectionCancellationTokenSource?.Cancel();
        _receivingCancellationTokenSource?.Cancel();
        _webSocket?.Dispose();
    }
    
}
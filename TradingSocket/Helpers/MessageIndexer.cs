namespace TradingSocket.Helpers;

public class MessageIndexer
{
    private const int MaxId = 400;
    private const int OffsetMilliseconds = 3570000; // offset от балды
    private int _lastTimeIndex = 0;
    private readonly object _locker = new();

    public int GetIndex(int traderId = 0)
    {
        int currentTimeIndex = (int)(DateTime.Now.TimeOfDay.TotalMilliseconds / 10.0) - OffsetMilliseconds;
        
        int newLastTimeIndex;
        lock (_locker)
        {
            newLastTimeIndex = (currentTimeIndex <= _lastTimeIndex) ? _lastTimeIndex + 1 : currentTimeIndex;
            _lastTimeIndex = newLastTimeIndex;
        }

        return MaxId * newLastTimeIndex + traderId;
    }

    public int GetNumberFromMsgId(int msgId)
    {
        return msgId % MaxId;
    }
}
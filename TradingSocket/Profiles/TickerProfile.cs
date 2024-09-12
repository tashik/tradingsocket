using AutoMapper;
using TradingSocket.Deribit.Objects;
using TradingSocket.Okx.Objects;
using TradingSocketEvents.Domain;

namespace TradingSocket.Profiles;

public class TickerProfile : Profile
{
    public TickerProfile()
    {
        // OKX to Ticker mapping
        CreateMap<OkxInstrument, Ticker>()
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.InstId))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => MapInstrumentType(src.InstType)))
            .ForMember(dest => dest.OptionType, opt => opt.MapFrom(src => MapOptionType(src.OptionType)))
            .ForMember(dest => dest.ContractType, opt => opt.MapFrom(src => MapContractType(src.ContractType)))
            .ForMember(dest => dest.ExpDate, opt => opt.MapFrom(src => MapTimestamp(src.ExpDate)));
        
        // OKX to Ticker mapping
        CreateMap<DeribitInstrument, Ticker>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => MapInstrumentType(src.InstType)))
            .ForMember(dest => dest.OptionType, opt => opt.MapFrom(src => MapOptionType(src.OptionType)))
            .ForMember(dest => dest.ContractType, opt => opt.MapFrom(src => MapContractType(src.ContractType)))
            .ForMember(dest => dest.ExpDate, opt => opt.MapFrom(src => MapTimestampFromLong(src.ExpirationTimestamp)));
    }

    private static InstrumentType MapInstrumentType(string instrumentType)
    {
        if (Enum.TryParse(instrumentType, true, out InstrumentType result))
        {
            return result;
        }

        throw new ArgumentException($"Invalid instrument type: {instrumentType}");
    }
    
    private static OptionType? MapOptionType(string optionType)
    {
        return optionType switch
        {
            "C" => OptionType.Call,
            "call" => OptionType.Call,
            "P" => OptionType.Put,
            "put" => OptionType.Put,
            _ => null
        };
    }
    
    private static ContractType MapContractType(string contractType)
    {
        return contractType switch
        {
            "linear" => ContractType.Linear,
            "inverse" => ContractType.Inverse,
            "reversed" => ContractType.Inverse,
            _ => ContractType.Linear
        };
    }

    private static DateTime? MapTimestamp(string unixTimestampString)
    {
        if (long.TryParse(unixTimestampString, out long unixTimestampSeconds))
        {
            return MapTimestampFromLong(unixTimestampSeconds);
        }

        return null;
    }
    
    private static DateTime? MapTimestampFromLong(long unixTimestampSeconds)
    {
        if (unixTimestampSeconds == long.MaxValue || unixTimestampSeconds == long.MinValue)
        {
            return null;
        }
        DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(unixTimestampSeconds).UtcDateTime;
        return dateTime;
    }
}
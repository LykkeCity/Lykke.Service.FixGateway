namespace Lykke.Service.FixGateway.Services.DTO.MatchingEngine
{
    public enum MessageStatus
    {
        Ok = 0,
        LowBalance = 401,
        AlreadyProcessed = 402,
        UnknownAsset = 410,
        NoLiquidity = 411,
        NotEnoughFunds = 412,
        //FREE 413
        ReservedVolumeHigherThanBalance = 414,
        LimitOrderNotFound = 415,
        BalanceLowerThanReserved = 416,
        LeadToNegativeSpread = 417,
        TooSmallVolume = 418,
        InvalidFee = 419,
        Duplicate = 430,
        Runtime = 500
    }
}

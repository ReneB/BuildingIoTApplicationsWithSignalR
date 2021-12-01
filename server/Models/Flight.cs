namespace Server.Models {
    public record Flight(
        int Year,
        int Month,
        int DayOfMonth,
        string? Origin,
        string? OriginCommonName,
        int DepGate,
        int CRSDepTime,
        string? Dest,
        string? DestCommonName,
        int ArrGate,
        int CRSArrTime,
        string UniqueCarrier,
        int FlightNum
    );
}
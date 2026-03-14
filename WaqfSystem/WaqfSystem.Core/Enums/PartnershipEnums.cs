namespace WaqfSystem.Core.Enums
{
    public enum PartnershipType
    {
        RevenuePercent = 0,
        FloorOwnership = 1,
        UnitOwnership = 2,
        UsufructRight = 3,
        LandPercent = 4,
        TimedPartnership = 5,
        HarvestShare = 6
    }

    public enum PartnerType
    {
        Individual = 0,
        Company = 1,
        Heirs = 2,
        Government = 3,
        Foundation = 4,
        Other = 5
    }

    public enum RevenueDistribMethod
    {
        Monthly = 0,
        Quarterly = 1,
        Annual = 2,
        PerCollection = 3
    }

    public enum TransferStatus
    {
        Pending = 0,
        Transferred = 1,
        Cancelled = 2
    }

    public enum ContactType
    {
        SMS = 0,
        WhatsApp = 1,
        Email = 2,
        Phone = 3,
        Meeting = 4,
        Letter = 5,
        PDF = 6
    }

    public enum ContactDirection
    {
        Outgoing = 0,
        Incoming = 1
    }

    public enum PartnershipNotificationTrigger
    {
        ExpiryWarning90 = 0,
        ExpiryWarning30 = 1,
        Expired = 2,
        DistributionCreated = 3,
        TransferCompleted = 4,
        StatementReady = 5
    }
}

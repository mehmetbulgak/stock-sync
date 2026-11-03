namespace StockSync.Domain.Products;

public enum StockMovementType
{
    InitialStock,
    Adjustment,
    Reservation,
    ReservationReleased,
    Shipment,
}

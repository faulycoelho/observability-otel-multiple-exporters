namespace Observability.IoC.Shared
{
    public record BookingRequestInternalDto(int userid, decimal value, int bookingId, string traceId);

}

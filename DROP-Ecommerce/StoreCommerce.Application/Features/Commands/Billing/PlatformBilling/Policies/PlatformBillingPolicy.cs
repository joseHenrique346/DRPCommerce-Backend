namespace StoreCommerce.Application.Billing;

public static class PlatformBillingPolicy
{
    public const int GraceDays = 7;

    public static DateTime PeriodEnd(DateTime debitDate, DateTime? anchor)
    {
        if (!anchor.HasValue)
            return debitDate.AddMonths(1);

        var months = (debitDate.Year - anchor.Value.Year) * 12 + debitDate.Month - anchor.Value.Month;
        var end = anchor.Value.AddMonths(months + 1);

        if (months < 0 || end <= debitDate || end > debitDate.AddDays(35))
            throw new ArgumentException("A data de débito não corresponde ao ciclo mensal.");

        return end;
    }

    public static decimal UpgradeAmount(decimal oldMonthly, decimal newMonthly, DateTime start, DateTime end, DateTime now)
    {
        if (newMonthly <= oldMonthly || now < start || now >= end)
            throw new ArgumentException("O upgrade exige período pago vigente e valor mensal superior.");

        return Math.Max(0.01m, decimal.Round((newMonthly - oldMonthly) * (end.Ticks - now.Ticks) /
            (end.Ticks - start.Ticks), 2, MidpointRounding.AwayFromZero));
    }

    public static string Status(DateTime? paidUntil, bool closed, bool review, DateTime now)
    {
        if (closed)
            return "canceled";

        if (!paidUntil.HasValue)
            return review ? "review_required" : "awaiting_payment";

        if (now >= paidUntil.Value.AddDays(GraceDays))
            return "expired";

        if (now >= paidUntil.Value)
            return "past_due";

        return review ? "review_required" : "active";
    }
}

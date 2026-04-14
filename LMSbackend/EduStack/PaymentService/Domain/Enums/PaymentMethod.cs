// PaymentMethod Enum — Supported payment methods for course purchases.
// • Stored as STRING in DB (HasConversion<string>()) for readability.
// • Parsed from a string in InitiatePaymentRequest using Enum.TryParse (case-insensitive).
// • Methods reflect common Indian payment ecosystem (UPI, NetBanking, Wallet).

namespace PaymentService.Domain.Enums;

public enum PaymentMethod
{
    CreditCard,   // international & domestic credit cards
    DebitCard,    // bank debit cards
    UPI,          // Unified Payments Interface (default — most popular in India)
    NetBanking,   // direct bank transfer
    Wallet        // digital wallets (Paytm, PhonePe, etc.)
}
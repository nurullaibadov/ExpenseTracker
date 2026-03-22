namespace ExpenseTracker.Domain.Enums;

public enum UserRole
{
    User = 0,
    Admin = 1
}

public enum ExpenseType
{
    Expense = 0,
    Income = 1
}

public enum PaymentMethod
{
    Cash = 0,
    CreditCard = 1,
    DebitCard = 2,
    BankTransfer = 3,
    Other = 4
}

public enum RecurringInterval
{
    Daily = 0,
    Weekly = 1,
    Monthly = 2,
    Yearly = 3
}

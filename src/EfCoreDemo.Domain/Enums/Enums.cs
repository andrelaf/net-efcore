namespace EfCoreDemo.Domain.Enums;

public enum OrderStatus
{
    Pending = 0,
    Paid = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4
}

/// <summary>Papel de um autor em um livro (payload de relacionamento N:N).</summary>
public enum AuthorRole
{
    Primary = 0,
    CoAuthor = 1,
    Editor = 2,
    Translator = 3
}

public enum AuditAction
{
    Insert = 0,
    Update = 1,
    Delete = 2
}

public enum EbookFormat
{
    Pdf = 0,
    Epub = 1,
    Mobi = 2
}

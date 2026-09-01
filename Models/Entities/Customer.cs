using PosApi.Models.Enums;

namespace PosApi.Models.Entities;

public class Customer
{
    public string CustomerCode { get; set; } = null!;
    public string CustomerName { get; set; } = null!;
    public string? Mobile { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public CustomerType CustomerType { get; set; }
    public int LoyaltyPoints { get; set; }
    public decimal CreditLimit { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }

    public CreditCustomer? CreditCustomer { get; set; }
    public ICollection<ChequeRegister> ChequeRegisters { get; set; } = new List<ChequeRegister>();
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}

public class CreditCustomer
{
    public int CreditId { get; set; }
    public string CustomerCode { get; set; } = null!;
    public decimal CreditLimit { get; set; }
    public decimal ReceiptTotal { get; set; }
    public decimal ReturnTotal { get; set; }
    public decimal PaidCredit { get; set; }
    public decimal Outstanding { get; set; }
    public bool IsActivate { get; set; }

    public Customer? Customer { get; set; }
}

public class ChequeRegister
{
    public int Id { get; set; }
    public string ChequeNo { get; set; } = null!;
    public string CustomerCode { get; set; } = null!;
    public string? ReceiptNo { get; set; }
    public DateTime ChequeDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public ChequeStatus Status { get; set; }

    public Customer? Customer { get; set; }
}

namespace MVC_Project.Jobs.Models
{
    public class InvoicesDTO
    {
        public int totalInvoice { get; set; }
        public int totalInvoiceReceived { get; set; }
        public int totalInvoiceIssued { get; set; }
        public int extraBills { get; set; }
    }
}
using OCPPaymentSystemAPI.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace OCPPaymentSystemAPI.Documents
{
    public class MemoPdfGenerator
    {
        public byte[] Generate(MemoPdfModel model)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.MarginLeft(45);
                    page.MarginRight(45);
                    page.MarginTop(30);
                    page.MarginBottom(35);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10));

                    page.Header().Column(header =>
                    {
                        header.Item().AlignRight().Text("MINAMAS").Bold().FontSize(18);
                        header.Item().PaddingTop(10).AlignCenter().Text("M E M O R A N D U M").Bold().FontSize(15);
                    });

                    page.Content().Column(col =>
                    {
                        col.Spacing(4);
                        col.Item().PaddingTop(20);

                        AddHeaderRow(col, "Kepada", "Head Treasury");
                        AddHeaderRow(col, "Dari", "RCEO " + model.Region);
                        AddHeaderRow(col, "O/Ref", model.MemoNo);
                        AddHeaderRow(col, "Tanggal", model.MemoDate == DateTime.MinValue ? "" : model.MemoDate.ToString("d MMMM yyyy"));
                        AddHeaderRow(col, "Perihal", "Tagihan " + model.Perihal);

                        col.Item().PaddingTop(15).Column(line =>
                        {
                            line.Item().BorderBottom(1);
                            line.Item().PaddingTop(3).BorderBottom(1);
                        });

                        col.Item().PaddingTop(18).Text(text =>
                        {
                            text.Span("Terlampir kami sampaikan Invoice tagihan ");
                            text.Span(string.IsNullOrWhiteSpace(model.Perihal) ? "-" : model.Perihal);
                            text.Span(" dari ");
                            text.Span(string.IsNullOrWhiteSpace(model.SupplierName) ? "-" : model.SupplierName).Bold();
                            text.Span(", dengan uraian sebagai berikut :");
                        });

                        col.Item().PaddingTop(15).Row(row =>
                        {
                            row.ConstantItem(25).Text("•").Bold();
                            row.RelativeItem().Text(text =>
                            {
                                text.Span("No. Inv. ");
                                text.Span(string.IsNullOrWhiteSpace(model.InvoiceNo) ? "-" : model.InvoiceNo);
                                text.Span(" = ");
                                text.Span("Rp. " + model.Amount.ToString("N0") + ",-").Bold();
                            });
                        });

                        col.Item().PaddingTop(22).Text(text =>
                        {
                            text.Span("Tagihan tersebut dibebankan kepada PT. ");
                            text.Span(string.IsNullOrWhiteSpace(model.CompanyName) ? "-" : model.CompanyName).Bold();
                            text.Span(". Mohon bantuan Bapak untuk memproses pembayaran tersebut dan ditransfer ke rekening :");
                        });

                        col.Item().PaddingTop(18).AlignCenter().Column(bank =>
                        {
                            bank.Item().Text(string.IsNullOrWhiteSpace(model.AccountName) ? model.SupplierName : model.AccountName).Bold();
                            bank.Item().Text(string.IsNullOrWhiteSpace(model.BankName) ? "-" : model.BankName).Bold();
                            bank.Item().Text("A/C No : " + (string.IsNullOrWhiteSpace(model.AccountNo) ? "-" : model.AccountNo)).Bold();
                            bank.Item().PaddingTop(3).Text("Jatuh tempo : Segera - Urgent").Bold().Underline();
                        });

                        col.Item().PaddingTop(22).Text("Demikian kami sampaikan, atas bantuan dan kerjasamanya, kami ucapkan terima kasih.");

                        if (model.Approvers != null && model.Approvers.Count > 0)
                        {
                            var approvers = model.Approvers.Take(5).ToList();

                            if (approvers.Count > 0)
                            {
                                col.Item().PaddingTop(8).Row(row =>
                                {
                                    for (int i = 0; i < 4; i++)
                                    {
                                        if (i < approvers.Count)
                                        {
                                            var approver = approvers[i];
                                            row.RelativeItem().Element(container => ComposeApproverCard(container, approver, i));
                                        }
                                        else
                                        {
                                            row.RelativeItem();
                                        }
                                    }
                                });
                            }

                            if (approvers.Count > 4)
                            {
                                col.Item().PaddingTop(5).Row(row =>
                                {
                                    row.RelativeItem();
                                    row.RelativeItem().Element(container => ComposeApproverCard(container, approvers[4], -99));
                                    row.RelativeItem();
                                });
                            }
                        }
                    });

                    page.Footer().AlignCenter().Text("Generated by OCP Payment System");
                });
            }).GeneratePdf();
        }

        private static void ComposeApproverCard(IContainer container, MemoApprover approver, int i = 0)
        {
            container.Border(1).Padding(5).Column(column =>
            {
                if (i == 0)
                {
                    column.Item().AlignCenter().Text("Requested by").FontSize(9);
                }
                else
                {
                    column.Item().AlignCenter().Text("Approved by").FontSize(9);
                }

                column.Item().PaddingTop(10).AlignCenter().Text(string.IsNullOrWhiteSpace(approver.Name) ? "-" : approver.Name).FontSize(9).Bold();

                column.Item().PaddingTop(7).AlignCenter().Text(
                    approver.Date.HasValue ? approver.Date.Value.ToString("d/M/yyyy h:mm:ss tt") : "-"
                ).FontSize(8);

                column.Item().PaddingTop(5).AlignCenter().Text(
                    string.IsNullOrWhiteSpace(approver.Designation) ? "-" : approver.Designation
                ).FontSize(9).Bold();
            });
        }

        private static void AddHeaderRow(ColumnDescriptor col, string title, string value)
        {
            col.Item().Row(row =>
            {
                row.ConstantItem(65).Text(title).Bold();
                row.ConstantItem(10).Text(":");
                row.RelativeItem().Text(value ?? "");
            });
        }
    }
}
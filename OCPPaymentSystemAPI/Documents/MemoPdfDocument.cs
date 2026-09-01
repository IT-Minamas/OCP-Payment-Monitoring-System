using OCPPaymentSystemAPI.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace OCPPaymentSystemAPI.Documents
{
    public class MemoPdfDocument : IDocument
    {
        private readonly MemoPdfModel _model;

        public MemoPdfDocument(MemoPdfModel model)
        {
            _model = model;
        }

        public DocumentMetadata GetMetadata()
        {
            return DocumentMetadata.Default;
        }

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);

                // Standard memo mempunyai margin yang cukup lebar
                page.MarginLeft(40);
                page.MarginRight(40);
                page.MarginTop(30);
                page.MarginBottom(35);

                page.DefaultTextStyle(x =>
                    x.FontFamily("Arial")
                     .FontSize(10));

                page.Content()
                    .Column(column =>
                    {
                        // =====================================================
                        // HEADER
                        // =====================================================

                        column.Item()
                            .Height(75)
                            .Row(row =>
                            {
                                // Kiri kosong
                                row.RelativeItem();

                                // Logo / Company Name
                                // Hardcoded sementara karena logo image belum
                                // kita mapping ke file/path tertentu.
                                row.ConstantItem(150)
                                    .AlignRight()
                                    .AlignMiddle()
                                    .Text("Minamas")
                                    .FontFamily("Arial")
                                    .FontSize(22)
                                    .Bold();
                            });


                        // =====================================================
                        // TITLE
                        // =====================================================

                        column.Item()
                            .AlignCenter()
                            .Text("M E M O R A N D U M")
                            .FontFamily("Arial")
                            .FontSize(14)
                            .Bold();

                        column.Item()
                            .Height(18);


                        // =====================================================
                        // MEMO HEADER INFORMATION
                        // =====================================================

                        column.Item()
                            .Row(row =>
                            {
                                row.ConstantItem(70)
                                    .Text("Kepada")
                                    .Bold();

                                row.ConstantItem(10)
                                    .Text(":");

                                row.RelativeItem()
                                    .Text("Head Treasury");
                            });

                        column.Item()
                            .Row(row =>
                            {
                                row.ConstantItem(70)
                                    .Text("Dari")
                                    .Bold();

                                row.ConstantItem(10)
                                    .Text(":");

                                row.RelativeItem()
                                    .Text("GUTHRIE INTERNATIONAL PRATAMA INDONESIA");
                            });

                        column.Item()
                            .Row(row =>
                            {
                                row.ConstantItem(70)
                                    .Text("O/Ref")
                                    .Bold();

                                row.ConstantItem(10)
                                    .Text(":");

                                row.RelativeItem()
                                    .Text(_model.MemoNo);
                            });

                        column.Item()
                            .Row(row =>
                            {
                                row.ConstantItem(70)
                                    .Text("Tanggal")
                                    .Bold();

                                row.ConstantItem(10)
                                    .Text(":");

                                row.RelativeItem()
                                    .Text(
                                        _model.MemoDate == default
                                            ? ""
                                            : _model.MemoDate.ToString("d MMMM yyyy")
                                    );
                            });

                        column.Item()
                            .Row(row =>
                            {
                                row.ConstantItem(70)
                                    .Text("Perihal")
                                    .Bold();

                                row.ConstantItem(10)
                                    .Text(":");

                                row.RelativeItem()
                                    .Text(
                                        $"Tagihan Pembelian TBS {_model.SupplierName} ke {_model.CompanyName}"
                                    )
                                    .Bold();
                            });


                        // =====================================================
                        // SEPARATOR
                        // =====================================================

                        column.Item()
                            .PaddingTop(18)
                            .PaddingBottom(18)
                            .Column(separator =>
                            {
                                separator.Item()
                                    .BorderBottom(1);

                                separator.Item()
                                    .PaddingTop(3)
                                    .BorderBottom(1);
                            });


                        // =====================================================
                        // BODY
                        // =====================================================

                        column.Item()
                            .Text(text =>
                            {
                                text.Span("Terlampir kami sampaikan Invoice tagihan Pembelian TBS dari ");

                                text.Span(_model.SupplierName)
                                    .Bold();

                                text.Span(
                                    ", dengan uraian sebagai berikut :"
                                );
                            });

                        column.Item()
                            .PaddingTop(15)
                            .Row(row =>
                            {
                                row.ConstantItem(25)
                                    .Text("•")
                                    .Bold();

                                row.RelativeItem()
                                    .Text(text =>
                                    {
                                        text.Span("No. Inv. ");

                                        text.Span(
                                            string.IsNullOrWhiteSpace(_model.InvoiceNo)
                                                ? "-"
                                                : _model.InvoiceNo
                                        )
                                        .Bold();

                                        text.Span(" = ");

                                        text.Span(
                                            $"Rp. {_model.Amount:N0},-"
                                        )
                                        .Bold();
                                    });
                            });


                        // =====================================================
                        // PAYMENT INFORMATION
                        // =====================================================

                        column.Item()
                            .PaddingTop(25)
                            .Text(text =>
                            {
                                text.Span("Tagihan tersebut dibebankan kepada ");

                                text.Span(
                                    _model.CompanyName.ToUpper()
                                )
                                .Bold();

                                text.Span(
                                    ". Mohon bantuan Bapak untuk memproses pembayaran tersebut dan ditransfer ke rekening :"
                                );
                            });


                        // =====================================================
                        // BANK INFORMATION
                        // =====================================================

                        column.Item()
                            .PaddingTop(18)
                            .AlignCenter()
                            .Column(bank =>
                            {
                                bank.Item()
                                    .Text(
                                        string.IsNullOrWhiteSpace(_model.AccountName)
                                            ? _model.SupplierName
                                            : _model.AccountName
                                    )
                                    .Bold();

                                bank.Item()
                                    .Text(
                                        string.IsNullOrWhiteSpace(_model.BankName)
                                            ? "Bank Rakyat Indonesia – Lubuklinggau"
                                            : _model.BankName
                                    )
                                    .Bold();

                                bank.Item()
                                    .Text(
                                        $"A/C No : {(
                                            string.IsNullOrWhiteSpace(_model.AccountNo)
                                                ? "0129-01-002720-30-1"
                                                : _model.AccountNo
                                        )}"
                                    )
                                    .Bold();

                                bank.Item()
                                    .Text("Jatuh tempo : Segera - Urgent")
                                    .Bold()
                                    .Underline();
                            });


                        // =====================================================
                        // CLOSING
                        // =====================================================

                        column.Item()
                            .PaddingTop(25)
                            .Text(
                                "Demikian kami sampaikan, atas bantuan dan kerjasamanya, " +
                                "kami ucapkan terima kasih."
                            );


                        // =====================================================
                        // SIGNATURE AREA
                        // =====================================================

                        column.Item()
                            .PaddingTop(55)
                            .Row(row =>
                            {
                                // -------------------------------------------------
                                // CREATED BY
                                // -------------------------------------------------

                                row.RelativeItem()
                                    .Column(left =>
                                    {
                                        left.Item()
                                            .Text("Dibuat,");

                                        left.Item()
                                            .Height(55);

                                        left.Item()
                                            .Text(
                                                string.IsNullOrWhiteSpace(_model.CreatedBy)
                                                    ? "Chandra Ebtario"
                                                    : _model.CreatedBy
                                            )
                                            .Bold()
                                            .Underline();

                                        left.Item()
                                            .Text("Trade - GIPI");
                                    });


                                // -------------------------------------------------
                                // APPROVED BY
                                // -------------------------------------------------

                                row.RelativeItem()
                                    .Column(right =>
                                    {
                                        right.Item()
                                            .AlignCenter()
                                            .Text("Approved")
                                            .Bold();

                                        right.Item()
                                            .Height(55);

                                        right.Item()
                                            .AlignCenter()
                                            .Text(
                                                string.IsNullOrWhiteSpace(_model.ApprovedBy)
                                                    ? "Zulkifli Nasution"
                                                    : _model.ApprovedBy
                                            )
                                            .Bold()
                                            .Underline();

                                        right.Item()
                                            .AlignCenter()
                                            .Text("Acting CFO Minamas");
                                    });
                            });


                        // =====================================================
                        // SECOND APPROVAL / HARD-CODED SIGNATURE
                        // =====================================================

                        column.Item()
                            .PaddingTop(5)
                            .Row(row =>
                            {
                                row.RelativeItem()
                                    .Column(left =>
                                    {
                                        left.Item()
                                            .Text("Disetujui,");

                                        left.Item()
                                            .Height(50);

                                        left.Item()
                                            .Text("Zulkifli Nasution")
                                            .Bold()
                                            .Underline();

                                        left.Item()
                                            .Text("Trade - GIPI");
                                    });

                                row.RelativeItem();
                            });
                    });
            });
        }
    }
}
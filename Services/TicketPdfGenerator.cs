using AquaPass.ModelsDto;
using AquaPass.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QRCoder;

namespace AquaPass.Services
{
    public class TicketPdfGenerator : ITicketPdfGenerator
    {
        public byte[] GenerateOrderTicketsPdf(Order order)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(25);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontColor(Colors.Grey.Darken3));

                    // Шапка документа
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("AquaPass").FontSize(24).ExtraBold().FontColor(Colors.Blue.Medium);
                            col.Item().Text("Електронний квиток відвідувача").FontSize(12).FontColor(Colors.Grey.Medium);
                        });

                        row.ConstantItem(180).Column(col =>
                        {
                            col.Item().AlignRight().Text($"Замовлення: #{order.OrderNumber}").Bold().FontSize(12);
                            col.Item().AlignRight().Text($"Дата: {order.VisitDate:dd.MM.yyyy}").FontSize(11);
                            col.Item().AlignRight().Text($"Клієнт: {order.CustomerFirstName} {order.CustomerLastName}").FontSize(10);
                        });
                    });

                    // Тіло з квитками
                    page.Content().PaddingVertical(20).Column(column =>
                    {
                        column.Spacing(15);

                        foreach (var ticket in order.Tickets)
                        {
                            var qrBytes = GenerateQrCodeBytes(ticket.TicketCode);

                            column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).CornerRadius(8).Padding(12).Row(row =>
                            {
                                row.RelativeItem().Column(ticketCol =>
                                {
                                    ticketCol.Spacing(4);
                                    ticketCol.Item().Text(ticket.EntranceTariff?.Name ?? "Вхідний квиток").FontSize(14).Bold().FontColor(Colors.Grey.Darken4);

                                    if (ticket.Sunbed != null)
                                    {
                                        ticketCol.Item().Text($"🏖️ Ряд {ticket.Sunbed.Row}, №{ticket.Sunbed.Number}").FontSize(11).SemiBold().FontColor(Colors.Blue.Medium);
                                    }

                                    ticketCol.Item().Text($"Код квитка: {ticket.TicketCode}").FontFamily("Courier").FontSize(10);
                                    ticketCol.Item().Text($"Вартість: {ticket.TotalPrice} ₴").FontSize(11).Bold();
                                });

                                if (qrBytes.Length > 0)
                                {
                                    row.ConstantItem(90).AlignRight().Image(qrBytes);
                                }
                            });
                        }

                        // Підсумок
                        column.Item().AlignRight().PaddingTop(10).Text($"Разом до сплати: {order.TotalAmount} ₴").FontSize(16).Bold().FontColor(Colors.Blue.Darken2);

                        // Інструкція для входу
                        column.Item().PaddingTop(15).BorderTop(1).BorderColor(Colors.Grey.Lighten2).Column(rules =>
                        {
                            rules.Spacing(3);
                            rules.Item().Text("Правила пред'явлення квитка:").Bold().FontSize(10);
                            rules.Item().Text("1. Пред'явіть даний PDF або відкрийте QR-код на екрані смартфона касиру на вході.").FontSize(9);
                            rules.Item().Text("2. Кожен QR-код є одноразовим для активації браслета в день візиту.").FontSize(9);
                        });
                    });

                    // Футер
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("AquaPass Support: support@aquapass.ua | Дякуємо, що обрали нас!");
                        x.DefaultTextStyle(t => t.FontSize(9).FontColor(Colors.Grey.Medium));
                    });
                });
            });

            return document.GeneratePdf();
        }

        private byte[] GenerateQrCodeBytes(string code)
        {
            if (string.IsNullOrEmpty(code)) return Array.Empty<byte>();

            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(code, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(20);
        }
    }
}

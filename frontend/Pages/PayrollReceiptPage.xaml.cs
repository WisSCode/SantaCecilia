using iText.Kernel.Pdf;
using iText.Layout;
using iTextDocument = iText.Layout.Document;
using iTextCell = iText.Layout.Element.Cell;
using iTextTable = iText.Layout.Element.Table;
using iTextParagraph = iText.Layout.Element.Paragraph;
using iTextTextAlignment = iText.Layout.Properties.TextAlignment;
using iText.Layout.Properties;

namespace frontend.Pages;

public partial class PayrollReceiptPage : ContentPage
{
    private string? payrollId;
    private string? workerId;
    private string? workerName;
    private DateTime weekStart;
    private decimal totalAmount;

    public PayrollReceiptPage(string html)
    {
        InitializeComponent();

        var source = new HtmlWebViewSource { Html = html };
        ReceiptWebView.Source = source;
    }

    public PayrollReceiptPage(string html, string payrollIdParam, string workerIdParam, string workerNameParam, DateTime weekStartParam, decimal totalAmountParam)
    {
        InitializeComponent();

        payrollId = payrollIdParam;
        workerId = workerIdParam;
        workerName = workerNameParam;
        weekStart = weekStartParam;
        totalAmount = totalAmountParam;

        var source = new HtmlWebViewSource { Html = html };
        ReceiptWebView.Source = source;
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void OnDownloadClicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(payrollId) || string.IsNullOrEmpty(workerName))
            {
                await DisplayAlertAsync("Error", "No hay datos de nómina para descargar", "OK");
                return;
            }

            // Sanitize filename to remove invalid characters
            var safeWorkerName = string.Join("_", workerName.Split(Path.GetInvalidFileNameChars()));
            var fileName = $"Recibo_{safeWorkerName}_{weekStart:yyyy-MM-dd}.pdf";
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var filePath = Path.Combine(documentsPath, fileName);

            // Delete existing file if it exists (to avoid locked file issues)
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch
                {
                    // If can't delete, create a new filename with timestamp
                    fileName = $"Recibo_{safeWorkerName}_{weekStart:yyyy-MM-dd}_{DateTime.Now:HHmmss}.pdf";
                    filePath = Path.Combine(documentsPath, fileName);
                }
            }

            GenerateReceiptPdf(filePath);

            // Ensure file exists before proceeding
            if (!File.Exists(filePath))
            {
                await DisplayAlertAsync("Error", "No se pudo generar el archivo PDF", "OK");
                return;
            }

            var openFile = await DisplayAlertAsync("Éxito", $"Recibo guardado en:\n{filePath}\n\n¿Desea abrirlo?", "Abrir", "Cerrar");

            if (openFile)
            {
#if WINDOWS
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
#else
                await Launcher.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(filePath)
                });
#endif
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo descargar el recibo: {ex.Message}\n\nDetalle: {ex.GetType().Name}", "OK");
        }
    }

    private async void OnPrintClicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(payrollId) || string.IsNullOrEmpty(workerName))
            {
                await DisplayAlertAsync("Error", "No hay datos de nómina para imprimir", "OK");
                return;
            }

            // Sanitize filename to remove invalid characters
            var safeWorkerName = string.Join("_", workerName.Split(Path.GetInvalidFileNameChars()));
            var fileName = $"Recibo_{safeWorkerName}_{weekStart:yyyy-MM-dd}.pdf";
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var filePath = Path.Combine(documentsPath, fileName);

            // Delete existing file if it exists
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch
                {
                    fileName = $"Recibo_{safeWorkerName}_{weekStart:yyyy-MM-dd}_{DateTime.Now:HHmmss}.pdf";
                    filePath = Path.Combine(documentsPath, fileName);
                }
            }

            GenerateReceiptPdf(filePath);

            // Ensure file exists before proceeding
            if (!File.Exists(filePath))
            {
                await DisplayAlertAsync("Error", "No se pudo generar el archivo PDF", "OK");
                return;
            }

#if WINDOWS
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
#else
            await Launcher.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(filePath)
            });
#endif
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo abrir para imprimir: {ex.Message}\n\nDetalle: {ex.GetType().Name}", "OK");
        }
    }

    private void GenerateReceiptPdf(string filePath)
    {
        PdfWriter? writer = null;
        PdfDocument? pdf = null;
        iTextDocument? document = null;

        try
        {
            writer = new PdfWriter(filePath);
            pdf = new PdfDocument(writer);
            document = new iTextDocument(pdf);
            
            document.SetMargins(20, 20, 20, 20);

            // Encabezado
            var header = new iTextParagraph("BOLETA DE PAGO")
                .SetFontSize(18)
                .SetBold()
                .SetTextAlignment(iTextTextAlignment.CENTER);
            document.Add(header);

            var company = new iTextParagraph("Santa Cecilia")
                .SetFontSize(10)
                .SetTextAlignment(iTextTextAlignment.CENTER);
            document.Add(company);

            document.Add(new iTextParagraph("\n"));

            // Información del trabajador
            var infoTable = new iTextTable(UnitValue.CreatePercentArray(2)).UseAllAvailableWidth();
            infoTable.AddCell(new iTextCell().Add(new iTextParagraph("Trabajador:").SetBold().SetFontSize(10)));
            infoTable.AddCell(new iTextCell().Add(new iTextParagraph(workerName ?? "").SetFontSize(10)));
            infoTable.AddCell(new iTextCell().Add(new iTextParagraph("Período:").SetBold().SetFontSize(10)));
            var weekEnd = weekStart.AddDays(6);
            infoTable.AddCell(new iTextCell().Add(new iTextParagraph($"{weekStart:dd/MM/yyyy} - {weekEnd:dd/MM/yyyy}").SetFontSize(10)));

            document.Add(infoTable);

            document.Add(new iTextParagraph("\n"));

            // Línea divisoria
            document.Add(new iTextParagraph("_".PadRight(50, '_')));
            document.Add(new iTextParagraph("\n"));

            // Resumen de pago
            var summaryTable = new iTextTable(UnitValue.CreatePercentArray(2)).UseAllAvailableWidth();
            summaryTable.AddCell(new iTextCell().Add(new iTextParagraph("Concepto:").SetBold().SetFontSize(11)));
            summaryTable.AddCell(new iTextCell().Add(new iTextParagraph("Monto").SetBold().SetFontSize(11)).SetTextAlignment(iTextTextAlignment.RIGHT));
            summaryTable.AddCell(new iTextCell().Add(new iTextParagraph("Sueldo Bruto").SetFontSize(10)));
            summaryTable.AddCell(new iTextCell().Add(new iTextParagraph($"B/.{totalAmount:F2}").SetFontSize(10)).SetTextAlignment(iTextTextAlignment.RIGHT));

            // Totales remarcados
            summaryTable.AddCell(new iTextCell().Add(new iTextParagraph("TOTAL A PAGAR").SetBold().SetFontSize(12)));
            summaryTable.AddCell(new iTextCell().Add(new iTextParagraph($"B/.{totalAmount:F2}").SetBold().SetFontSize(12)).SetTextAlignment(iTextTextAlignment.RIGHT));

            document.Add(summaryTable);

            document.Add(new iTextParagraph("\n"));
            document.Add(new iTextParagraph("_".PadRight(50, '_')));

            // Pie de página
            document.Add(new iTextParagraph("\n"));
            var footer = new iTextParagraph($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                .SetFontSize(8)
                .SetTextAlignment(iTextTextAlignment.CENTER);
            document.Add(footer);

            var thanks = new iTextParagraph("Gracias por su trabajo")
                .SetFontSize(9)
                .SetTextAlignment(iTextTextAlignment.CENTER)
                .SetItalic();
            document.Add(thanks);
        }
        finally
        {
            // Ensure proper disposal in correct order
            document?.Close();
            pdf?.Close();
            writer?.Close();
        }
    }
}

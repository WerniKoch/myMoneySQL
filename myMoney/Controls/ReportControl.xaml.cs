using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;
using PdfSharpCore.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System;
using System.IO;
using System.Linq;

namespace myMoney.Controls
{
    /// <summary>
    /// Interaction logic for ReportControl.xaml
    /// </summary>
    public partial class ReportControl : UserControl
    {
#if DEBUG
        private static string ReportVerzeichnis = Environment.CurrentDirectory + "\\Reports\\";
#else
        private static string ReportVerzeichnis = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\myMoney\\Daten\\Reports\\";
#endif

        public ReportControl()
        {
            InitializeComponent();
            SetDatum();

            if (!Directory.Exists(ReportVerzeichnis))
            {
                Directory.CreateDirectory(ReportVerzeichnis);
            }

            if (GlobalFontSettings.FontResolver == null)
            {
                GlobalFontSettings.FontResolver = new FontResolver();
            }
        }

        private void SetDatum()
        {
            int year = DateTime.Now.Year;
            dteDatumVon.SelectedDate = new DateTime(year, 1, 1);
            dteDatumBis.SelectedDate = new DateTime(year, 12,31);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // https://github.com/ststeiger/PdfSharpCore#readme

            // Alle Daten innerhalb vom Datum gruppiert nach Kategorie
            var kategorieList = DataAccess.ReadKategorien();
            var listDaten = DataAccess.ReadBuchungen(Guid.Empty).Where(x => x.Datum >= dteDatumVon.SelectedDate && x.Datum <= dteDatumBis.SelectedDate);

            var results = listDaten.GroupBy(p => p.Kategorie)
                .Select(res => new
                {
                    res.Key,
                    BetragMitVorzeichen = res.Sum(s => s.BetragMitVorzeichen),
                    OberKategorie = kategorieList?.FirstOrDefault(k => k.Id == res.Key)?.OberKategorie ?? string.Empty,
                    UnterKategorie = kategorieList?.FirstOrDefault(k => k.Id == res.Key)?.UnterKategorie ?? string.Empty,
                }).OrderBy(o => o.OberKategorie).ThenBy(u => u.UnterKategorie);
               
            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            string datumVon = DateTime.Now.ToString("dd.MM.yyyy");
            string datumBis = DateTime.Now.ToString("dd.MM.yyyy");

            if (dteDatumVon?.SelectedDate != null)
                datumVon = dteDatumVon.SelectedDate.Value.ToString("dd.MM.yyyy");
            if (dteDatumBis?.SelectedDate != null)
                datumBis = dteDatumBis.SelectedDate.Value.ToString("dd.MM.yyyy");

            // Definition Fonts aus Einstellungen
            string font = DataAccess.ReadFont();
            var fontTitel = new XFont(font, 24, XFontStyle.Bold);
            var fontSubTitel = new XFont(font, 13, XFontStyle.Regular);
            var fontBold = new XFont(font, 10, XFontStyle.Bold);
            var fontNormal = new XFont(font, 10, XFontStyle.Regular);
            var fontBoldBig = new XFont(font, 13, XFontStyle.Bold);

            gfx.DrawString("Einnahmen/Ausgaben pro Kategorie", fontTitel, XBrushes.Black, new XRect(20, 50, page.Width, page.Height), XStringFormats.TopCenter);
            gfx.DrawString("Von " + datumVon + " bis " + datumBis, fontSubTitel, XBrushes.Black, new XRect(20, 80, page.Width, page.Height), XStringFormats.TopCenter);

            var zeilenPos = 120;
            string titelText = string.Empty;
            decimal totalBetrag = 0.0m;
            decimal totalBetragEinnahmen = 0.0m;
            decimal totalBetragAusgaben = 0.0m;
            bool first = true;
            int counter = 0;

            foreach (var item in results)
            {
                if (!first && item.OberKategorie != titelText)
                {
                    gfx.DrawString("TOTAL " + titelText, fontBold, XBrushes.Black, new XRect(50, zeilenPos, page.Width, page.Height), XStringFormats.TopLeft);
                    gfx.DrawString(totalBetrag.ToString("###,###,##0.00"), fontBold, XBrushes.Black, new XRect(0, zeilenPos, page.Width - 50, page.Height), XStringFormats.TopRight);
                    zeilenPos += 13;
                    totalBetrag = 0.0m;
                    counter = 0;
                }

                if (titelText != item.OberKategorie)
                {
                    zeilenPos += 13;
                    titelText = item.OberKategorie;
                    gfx.DrawString(titelText, fontBold, XBrushes.Black, new XRect(50, zeilenPos, page.Width, page.Height), XStringFormats.TopLeft);
                    zeilenPos += 13;
                }

                if (counter % 2 == 0)
                {
                    gfx.DrawString(item.UnterKategorie, fontNormal, XBrushes.Black, new XRect(50, zeilenPos, page.Width, page.Height), XStringFormats.TopLeft);
                    gfx.DrawString(item.BetragMitVorzeichen.ToString("###,###,##0.00"), fontNormal, XBrushes.Black, new XRect(0, zeilenPos, page.Width - 50, page.Height), XStringFormats.TopRight);
                }
                else
                {
                    gfx.DrawString(item.UnterKategorie, fontNormal, XBrushes.DarkGray, new XRect(50, zeilenPos, page.Width, page.Height), XStringFormats.TopLeft);
                    gfx.DrawString(item.BetragMitVorzeichen.ToString("###,###,##0.00"), fontNormal, XBrushes.Gray, new XRect(0, zeilenPos, page.Width - 50, page.Height), XStringFormats.TopRight);
                }

                counter++;
                zeilenPos += 13;
                first = false;
                totalBetrag += item.BetragMitVorzeichen;

                if (item.BetragMitVorzeichen > 0.0m)
                {
                    totalBetragEinnahmen += item.BetragMitVorzeichen;
                }
                else
                {
                    totalBetragAusgaben += item.BetragMitVorzeichen;
                }


                // Neue Seite
                if (zeilenPos > 740)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    zeilenPos = 60;
                }
            }

            // Letzte Totalzeile Kategorie
            gfx.DrawString("TOTAL " + titelText, fontBold, XBrushes.Black, new XRect(50, zeilenPos, page.Width, page.Height), XStringFormats.TopLeft);
            gfx.DrawString(totalBetrag.ToString("###,###,##0.00"), fontBold, XBrushes.Black, new XRect(0, zeilenPos, page.Width - 50, page.Height), XStringFormats.TopRight);

            // Total Einnahmen und Ausgaben
            zeilenPos += 13;
            zeilenPos += 13;
            gfx.DrawString("TOTAL EINNAHMEN", fontBoldBig, XBrushes.Black, new XRect(50, zeilenPos, page.Width, page.Height), XStringFormats.TopLeft);
            gfx.DrawString(totalBetragEinnahmen.ToString("###,###,##0.00"), fontBoldBig, XBrushes.Black, new XRect(0, zeilenPos, page.Width - 50, page.Height), XStringFormats.TopRight);
            zeilenPos += 14;
            gfx.DrawString("TOTAL AUSGABEN", fontBoldBig, XBrushes.Black, new XRect(50, zeilenPos, page.Width, page.Height), XStringFormats.TopLeft);
            gfx.DrawString(totalBetragAusgaben.ToString("###,###,##0.00"), fontBoldBig, XBrushes.Black, new XRect(0, zeilenPos, page.Width - 50, page.Height), XStringFormats.TopRight);
            zeilenPos += 14;
            gfx.DrawString("MEHREINNAHMEN", fontBoldBig, XBrushes.Black, new XRect(50, zeilenPos, page.Width, page.Height), XStringFormats.TopLeft);
            gfx.DrawString((totalBetragEinnahmen + totalBetragAusgaben).ToString("###,###,##0.00"), fontBoldBig, XBrushes.Black, new XRect(0, zeilenPos, page.Width - 50, page.Height), XStringFormats.TopRight);

            document.Save(ReportVerzeichnis + "EinAusProKategorie.pdf");

            // PDF anzeigen
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = ReportVerzeichnis + "EinAusProKategorie.pdf";
            processStartInfo.UseShellExecute = true;
            Process.Start(processStartInfo);
        }
    }
}

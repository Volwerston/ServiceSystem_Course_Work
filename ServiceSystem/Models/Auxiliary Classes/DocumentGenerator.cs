using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace ServiceSystem.Models.Auxiliary_Classes
{
    public static class DocumentGenerator
    {
        public static void GeneratePDFDocument(Tuple<Bill, Dictionary<string, string>> toAdd, BillType type)
        {
            string directoryToAdd = String.Format(HostingEnvironment.MapPath(@"~\Common\Bills") + @"\{0}\{1}\{2}",
                                              toAdd.Item1.StatusChangeTime.Year.ToString(),
                                              toAdd.Item1.StatusChangeTime.Month.ToString(),
                                              toAdd.Item1.StatusChangeTime.Day.ToString());

            string pathToAdd = directoryToAdd + @"\" + toAdd.Item1.Id.ToString() + (type == BillType.ADVANCE ? "_advance" : "_main") + ".pdf";

            DirectoryInfo info = new DirectoryInfo(directoryToAdd);

            if (!info.Exists)
            {
                info.Create();
            }

            BaseFont baseFont = BaseFont.CreateFont(HostingEnvironment.MapPath("~/Common/arial.ttf"), BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
            Font font = new Font(baseFont, Font.DEFAULTSIZE, Font.NORMAL);

            Document document = new Document(PageSize.LETTER, 10, 10, 42, 35);

            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(pathToAdd, FileMode.Create));


            document.Open();

            Paragraph p = new Paragraph("Квитанція №" + toAdd.Item1.Id.ToString(), font);
            p.Alignment = Element.ALIGN_CENTER;
            p.SpacingAfter = 30;

            document.Add(p);

            PdfPTable table = new PdfPTable(2);

            table.AddCell(new Phrase("Послугу надав", font));
            table.AddCell(new Phrase(toAdd.Item1.ProviderLastName + " " + toAdd.Item1.ProviderFirstName + " " + toAdd.Item1.ProviderFatherName, font));

            table.AddCell(new Phrase("Замовник", font));
            table.AddCell(new Phrase(toAdd.Item1.CustomerLastName + " " + toAdd.Item1.CustomerFirstName + " " + toAdd.Item1.CustomerFatherName, font));

            table.AddCell(new Phrase("Ціна", font));

            if (type == BillType.ADVANCE)
            {
                table.AddCell((toAdd.Item1.Price * toAdd.Item1.AdvancePercent / Convert.ToDouble(100)).ToString());
            }
            else
            {
                table.AddCell((toAdd.Item1.Price * (1 - toAdd.Item1.AdvancePercent / Convert.ToDouble(100))).ToString());
            }

            table.AddCell(new Phrase("Валюта", font));

            switch (toAdd.Item1.Currency)
            {
                case "hryvnia":
                    table.AddCell(new Phrase("гривня", font));
                    break;
                case "dollar":
                    table.AddCell(new Phrase("долар", font));
                    break;
                case "euro":
                    table.AddCell(new Phrase("євро", font));
                    break;
            }

            table.AddCell(new Phrase("Сплатити до", font));

            if (type == BillType.ADVANCE)
            {
                table.AddCell(toAdd.Item1.AdvanceTimeLimit.ToString());
            }
            else
            {
                table.AddCell(toAdd.Item1.MainTimeLimit.ToString());
            }

            table.HorizontalAlignment = Element.ALIGN_CENTER;

            if (toAdd.Item1.Type == "WEBMONEY")
            {
                table.AddCell(new Phrase("Гаманець для оплати", font));
                table.AddCell(toAdd.Item2["WMPurse"].ToString());
            }
            else
            {
                table.AddCell(new Phrase("Номер рахунку для оплати", font));
                table.AddCell(toAdd.Item2["Account"].ToString());

                table.AddCell(new Phrase("ЄДРПОУ", font));
                table.AddCell(toAdd.Item2["EDRPOU"].ToString());

                table.AddCell(new Phrase("МФО", font));
                table.AddCell(toAdd.Item2["MFO"].ToString());
            }


            document.Add(table);

            document.Close();
        }

        public static void GenerateLocalDocument(HttpPostedFileBase local_file, string service_name)
        {
            DirectoryInfo info = new DirectoryInfo(HostingEnvironment.MapPath("~/Common/MediaFiles/") + service_name);

            if (!info.Exists)
            {
                info.Create();
            }

            BinaryWriter writer = new BinaryWriter(new FileStream(HostingEnvironment.MapPath("~/Common/MediaFiles/") + service_name + @"/" + local_file.FileName, FileMode.OpenOrCreate));

            byte[] toWrite = new byte[local_file.ContentLength];
            local_file.InputStream.Read(toWrite, 0, local_file.ContentLength);

            writer.Write(toWrite);

            writer.Close();
        }

        public static Tuple<byte[], Bill, Dictionary<string, string>> GetApplication(int application_id, Application app, string uri)
        {

            Tuple<Bill, Dictionary<string, string>> billData = null;

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                client.BaseAddress = new Uri(uri);

                HttpResponseMessage response = client.GetAsync("api/Bill?application_id=" + application_id).Result;

                if (response.IsSuccessStatusCode)
                {
                    billData = response.Content.ReadAsAsync<Tuple<Bill, Dictionary<string, string>>>().Result;
                }
            }

            if (billData != null)
            {
                string fullPath = String.Format(HostingEnvironment.MapPath(@"~\Common\Bills") + @"\{0}\{1}\{2}\{3}"
                                                + (app.Status == "ADVANCE_PENDING" ? "_advance" : "_main") + ".pdf",
                                      billData.Item1.StatusChangeTime.Year.ToString(),
                                      billData.Item1.StatusChangeTime.Month.ToString(),
                                      billData.Item1.StatusChangeTime.Day.ToString(),
                                      billData.Item1.Id);

                FileInfo fileInfo = new FileInfo(fullPath);

                if (!fileInfo.Exists)
                {
                    if (app.Status == "ADVANCE_PENDING")
                    {
                        GeneratePDFDocument(billData, BillType.ADVANCE);
                    }
                    else if (app.Status == "MAIN_PENDING")
                    {
                        GeneratePDFDocument(billData, BillType.MAIN);
                    }

                    fileInfo = new FileInfo(fullPath);
                }

                long fileLength = fileInfo.Length;
                FileStream fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                BinaryReader br = new BinaryReader(fs);

                return new Tuple<byte[], Bill, Dictionary<string, string>>(br.ReadBytes((int)fileLength), billData.Item1, billData.Item2);
            }
            else
            {
                return new Tuple<byte[], Bill, Dictionary<string, string>>(null, null, null);
            }
        }

        public static Dictionary<string, string> GetConstructorBlocks()
        {
            Dictionary<string, string> toReturn = new Dictionary<string, string>();

            string[] paths = new string[] { "week_gradation_widget_hours", "week_gradation_widget_duration",
                                            "duration_measure_from_till", "duration_measure_from", "duration_measure_till",
                                            "service_session_block", "duration_measure_widget", "price_measure_widget",
                                            "default_property_widget", "service_deadline_block", "service_course_block",
                                            "week_gradation_widget"};

            foreach (var path in paths)
            {
                StreamReader reader = new StreamReader(HostingEnvironment.MapPath("~/Common/" + path + ".html"));

                StringBuilder builder = new StringBuilder("");

                string buf = "";

                while ((buf = reader.ReadLine()) != null)
                {
                    builder.Append(buf);
                }

                toReturn.Add(path, builder.ToString());
            }

            return toReturn;
        }
    }
}
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Linq;
using System;
using System.Collections.Generic;

namespace ChronologyGenerator
{
    class Program
    {
        const float A4_WIDTH = 210f;
        const float A4_HEIGHT = 297f;

        const int CARDS_BY_ROWS = 4;
        const int CARDS_BY_COLUMN = 4;

        const float CARD_WIDTH = A4_WIDTH / CARDS_BY_ROWS;
        const float CARD_HEIGHT = A4_HEIGHT / CARDS_BY_COLUMN;

        static void Main(string[] args)
        {
            var result = @"C:\tmp\output.pdf";
            using (var fs = new FileStream(result, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var doc = new Document(PageSize.A4, 0, 0, 0, 0);

                var writer = PdfWriter.GetInstance(doc, fs);

                doc.Open();

                var questionsBatches = GetQuestions().Split(CARDS_BY_COLUMN * CARDS_BY_ROWS);
                foreach (var questions in questionsBatches)
                {
                    // page with questions
                    doc.NewPage();
                    for (int row = 0; row < CARDS_BY_COLUMN; row++)
                    {
                        for (int column = 0; column < CARDS_BY_ROWS; column++)
                        {
                            var question = questions.GetByIndexOrDefault(column + CARDS_BY_COLUMN * row);
                            if (question != null)
                            {
                                GenerateThumbnail(doc, writer, question, column, row, false);
                            }
                        }
                    }

                    // page with answers (in reverse order)
                    doc.NewPage();
                    for (int row = 0; row < CARDS_BY_COLUMN; row++)
                    {
                        for (int column = 0; column < CARDS_BY_ROWS; column++)
                        {
                            var question = questions.GetByIndexOrDefault((CARDS_BY_ROWS - 1 - column) + CARDS_BY_COLUMN * row);
                            if (question != null)
                            {
                                GenerateThumbnail(doc, writer, question, column, row, true);
                            }
                        }
                    }
                }
                doc.Close();
            }
        }

        private static void GenerateThumbnail(Document doc, PdfWriter writer, Question question, int x, int y, bool withAnswer)
        {
            Console.WriteLine($"{question.Date} {withAnswer} {question.Label}");

            var basePositionX = CARD_WIDTH * x;
            var basePositionY = CARD_HEIGHT * y;

            var background = Image.GetInstance("background.png");
            background.ScaleAbsoluteHeight(Utilities.MillimetersToPoints(CARD_HEIGHT));
            background.ScaleAbsoluteWidth(Utilities.MillimetersToPoints(CARD_WIDTH));
            background.SetAbsolutePosition(Utilities.MillimetersToPoints(basePositionX), Utilities.MillimetersToPoints(basePositionY));
            doc.Add(background);
            
            AddText(question.Label, basePositionX, basePositionY, writer, 15.0f, 2.625f, 14.025f, 49.875f, 71.775f, new Font(Font.FontFamily.HELVETICA, 16.0f, Font.NORMAL));
            if (withAnswer)
            {
                AddText(question.Date, basePositionX, basePositionY, writer, 0.0f, 2.625f, 6.150f, 49.875f, 14.025f, new Font(Font.FontFamily.HELVETICA, 26.0f, Font.BOLD));
                AddText(question.Hint, basePositionX, basePositionY, writer, 0.0f, 2.625f, 0.9f, 49.875f, 5.150f, new Font(Font.FontFamily.HELVETICA, 10.0f, Font.ITALIC), Element.ALIGN_RIGHT);
            }
        }

        private static void AddText(string text, float basePositionX, float basePositionY, PdfWriter writer, float leading, float llxmm, float llymm, float urxmm, float urymm, Font font, int alignement = Element.ALIGN_CENTER)
        {
            var llx = Utilities.MillimetersToPoints(basePositionX + llxmm);
            var lly = Utilities.MillimetersToPoints(basePositionY + llymm);
            var urx = Utilities.MillimetersToPoints(basePositionX + urxmm);
            var ury = Utilities.MillimetersToPoints(basePositionY + urymm);

            var ct = new ColumnText(writer.DirectContent);
            Phrase myText = new Phrase(text, font);
            ct.SetSimpleColumn(myText, llx, lly, urx, ury, leading, alignement);
            ct.Go();
        }

        private static IList<Question> GetQuestions()
        {
            return File
                .ReadAllLines("questions.txt")
                .Where(l => !string.IsNullOrEmpty(l))
                .Select(l => l.Split('\t'))
                .Select(s => new Question()
                {
                    Date = s[0],
                    Label = s[1],
                    Hint = s[2],
                }).ToList();
        }
    }
}

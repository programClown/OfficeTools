using NPOI.OpenXmlFormats.Wordprocessing;
using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace OfficeTools.Extensions;

public static class XWPFDocumentExtensions
{
    /// <summary>
    ///     增加自定义标题样式
    /// </summary>
    /// <param name="document">目标文档</param>
    /// <param name="styleId">样式名称</param>
    /// <param name="headingLevel">样式级别</param>
    public static void AddCustomHeadingStyle(this XWPFDocument document, string styleId, int headingLevel)
    {
        var ctStyle = new CT_Style();
        ctStyle.styleId = styleId;

        var styleName = new CT_String();
        styleName.val = styleId;
        ctStyle.name = styleName;

        var indentNumber = new CT_DecimalNumber();
        indentNumber.val = Convert.ToString(headingLevel);
        // lower number > style is more prominent in the formats bar
        ctStyle.uiPriority = indentNumber;

        var onOffNull = new CT_OnOff();
        ctStyle.unhideWhenUsed = onOffNull;

        // styles shows up in the formats bar
        ctStyle.qFormat = onOffNull;

        // styles defines a heading of the given level
        var ppr = new CT_PPr();
        ppr.outlineLvl = indentNumber;
        ctStyle.pPr = ppr;

        var style = new XWPFStyle(ctStyle);
        //is a null op if already defined
        XWPFStyles styles = document.CreateStyles();
        style.StyleType = ST_StyleType.paragraph;
        styles.AddStyle(style);
    }

    public static void GetAllParagrapphsContent(this XWPFDocument document, ref List<string> contents)
    {
        foreach (XWPFParagraph? paragraph in document.Paragraphs)
        {
            contents.Add(paragraph.Text);
        }
    }

    public static void GetAllTables(this XWPFDocument document, ref List<List<List<string>>> tables)
    {
        foreach (XWPFTable? table in document.Tables)
        {
            var rows = table.Rows;
            var ts = new List<List<string>>();
            foreach (XWPFTableRow? row in rows)
            {
                var cells = row.GetTableCells();
                var list = new List<string>();
                foreach (XWPFTableCell? cell in cells)
                {
                    list.Add(cell.GetText());
                }
                ts.Add(list);
            }
            tables.Add(ts);
        }
    }

    public static void GetAllPictures(this XWPFDocument document, ref List<Bitmap> pictures)
    {
        foreach (XWPFPictureData? pic in document.AllPictures)
        {
            using (var stream = new MemoryStream(pic.Data))
            {
                var bitmap = new Bitmap(stream);
                pictures.Add(bitmap);
            }
        }
    }
}
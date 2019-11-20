using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Web;
using FirstAgain.Common;
using System.Drawing.Drawing2D;
using System.Web.Script.Serialization;
using LightStreamWeb.ServerState;

namespace LightStreamWeb
{
    public static class SignatureUtility
    {
        public static int Width
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings["SignatureWidth"]);
            }
        }

        public static int Height
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings["SignatureHeight"]);
            }
        }

        public static byte[] GenerateSignatureFromFont(string name)
        {
            Font font;
            Bitmap bmp;
            Graphics graphic;
            StringFormat stringformat;

            try
            {
                // we initialize these here because it's needed to calculate the width
                // and height of the provided text.  the 1,1 is only a temporary 
                // measurement
                bmp = new Bitmap(1, 1);
                graphic = System.Drawing.Graphics.FromImage(bmp);

                // our font for the writing
                font = new Font(ConfigurationManager.AppSettings["SignatureFont"], int.Parse(ConfigurationManager.AppSettings["SignatureFontSize"]), FontStyle.Regular);

                // measure the text
                stringformat = new StringFormat(StringFormat.GenericTypographic);
                int height = Convert.ToInt32(graphic.MeasureString(name, font, new PointF(0, 0), stringformat).Height) + 6;
                int width = Convert.ToInt32(graphic.MeasureString(name, font, new PointF(0, 0), stringformat).Width);

                // Previously, there was a guestimate that 10% extra width was good enough for the signature font.
                // But for some combinations, with fat letters that are particularly slanted, this isn't enought.
                // So now, we'll default to 99% of the width of the control, so the borders don't cut off. 
                // Or 150% of the calculated width, which really, really should be plenty.
                width = (int)Math.Min(Width * 0.99, width * 1.5);

                // recreate our bmp and graphic objects with the new measurements
                bmp = new Bitmap(width, height);

                graphic = System.Drawing.Graphics.FromImage(bmp);

                // our background colour
                graphic.Clear(Color.White);

                // aliasing mode
                graphic.TextRenderingHint = TextRenderingHint.SystemDefault;

                /* 
                the different aliasing modes:
                ---------------------------------------------------------------------------------
                graphic.TextRenderingHint = TextRenderingHint.SystemDefault;
                - each character is drawn using its glyph bitmap, with the system default rendering hint

                graphic.TextRenderingHint = TextRenderingHint.AntiAlias;
                - each character is drawn using its antialiased glyph bitmap without hinting

                graphic.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                - each character is drawn using its antialiased glyph bitmap with hinting

                graphic.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                - each character is drawn using its glyph CT bitmap with hinting

                graphic.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;
                - each character is drawn using its glyph bitmap

                graphic.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
                - each character is drawn using its glyph bitmap
                */

                // our brush which is the writing colour
                SolidBrush brush = new SolidBrush(Color.Black);

                /*
                alternate brushes
                -------------------------------------------------------------------------------
                System.Drawing.Drawing2D.HatchBrush
                System.Drawing.Drawing2D.LinearGradientBrush
                System.Drawing.Drawing2D.PathGradientBush
                System.Drawing.SolidBrush
                System.Drawing.TextureBrush
                */

                // create our graphic

                graphic.DrawString(name, font, brush, new Rectangle(0, 0, width, height));

                // Set the content type and return the image
                //Response.ContentType = "image/JPEG";
                return BmpToBytes(bmp);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// This used to store info in the session state
        /// Also will be used to persist to DB
        /// Reference http://www.vbforums.com/showthread.php?t=358917
        /// </summary>
        /// <param name="bmp">Input bitmap to be converted to bytes.</param>
        /// <returns>Byte array of image in JPEG format.</returns>
        public static byte[] BmpToBytes(Bitmap bmp)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Save to memory using the Jpeg format
                bmp.Save(ms, ImageFormat.Jpeg);

                // read to end
                byte[] bmpBytes = ms.ToArray();
                bmp.Dispose();

                return bmpBytes;
            }
        }


        public static string GenerateTimeStamp()
        {
            return DateTime.Now.ToLightStreamTimeStampString();
        }

        public static string GetImageUrl(bool isCoApplicant)
        {
            return string.Format("/signature/display?isCoApplicant={0}&UniqueID={1}", isCoApplicant, DateTime.Now.Ticks.ToString());
        }

        // adapted from https://github.com/parrots/SignatureToImageDotNet/blob/master/SignatureToImage.cs
        /// <summary>
        /// Get a blank bitmap using instance properties for dimensions and background color.
        /// </summary>
        /// <returns>Blank bitmap image.</returns>
        private static Bitmap GetBlankCanvas()
        {
            var blankImage = new Bitmap(Width, Height);
            blankImage.MakeTransparent();
            using (var signatureGraphic = Graphics.FromImage(blankImage))
            {
                signatureGraphic.Clear(Color.White);
            }
            return blankImage;
        }

        // adapted from https://github.com/parrots/SignatureToImageDotNet/blob/master/SignatureToImage.cs
        /// <summary>
        /// Draws a signature based on the JSON provided by Signature Pad.
        /// </summary>
        /// <param name="json">JSON string of line drawing commands.</param>
        /// <returns>Bitmap image containing the signature.</returns>
        public static Bitmap SigJsonToImage(string json)
        {
            var signatureImage = GetBlankCanvas();
            if (!string.IsNullOrWhiteSpace(json))
            {
                using (var signatureGraphic = Graphics.FromImage(signatureImage))
                {
                    signatureGraphic.SmoothingMode = SmoothingMode.AntiAlias;
                    var pen = new Pen(Color.Black, 2);
                    var serializer = new JavaScriptSerializer();
                    // Next line may throw System.ArgumentException if the string
                    // is an invalid json primitive for the SignatureLine structure
                    var lines = serializer.Deserialize<List<SignatureLine>>(json);
                    foreach (var line in lines)
                    {
                        signatureGraphic.DrawLine(pen, line.lx, line.ly, line.mx, line.my);
                    }
                }
            }
            return signatureImage;
        }

        /// <summary>
        /// Line drawing commands as generated by the Signature Pad JSON export option.
        /// </summary>
        private class SignatureLine
        {
            public int lx { get; set; }
            public int ly { get; set; }
            public int mx { get; set; }
            public int my { get; set; }
        }

    }
}
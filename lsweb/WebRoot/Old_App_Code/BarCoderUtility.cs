using System;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.Web;

namespace FirstAgain.Web.UI
{
	public static class BarCoderUtility
	{

		public static Bitmap GetBarcodeBitmap(string id, int textPadding)
		{
			string code39 = GetBarcodeLabel(id);
			Color borderColor = Color.Black;

			using (Font font = CreateBarCodeFont())
			{
				SizeF textSize = GetRequiredBitmapSizeForText(font, code39);

                Bitmap bm = new Bitmap((int)Math.Ceiling(textSize.Width) + textPadding, (int)Math.Ceiling(textSize.Height) + textPadding);

				using (Graphics graphic = Graphics.FromImage(bm))
				{
					// Create a white background
					graphic.PageUnit = GraphicsUnit.Pixel;
					graphic.FillRectangle(Brushes.White, 0, 0, bm.Width, bm.Height);

					//Draw the text in the bitmap
					int bcy = (int)((bm.Height - font.GetHeight(graphic)) / 3);
                    int bcx = textPadding / 2;
					graphic.DrawString(code39, font, Brushes.Black, new Point(bcx, bcy));

					return bm;
				}
			}
		}

		public static EncoderParameters GetEncoderParams()
		{
			// Create quality parameter
			EncoderParameters encoderParams = new EncoderParameters(1);
			EncoderParameter qualityParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
			encoderParams.Param[0] = qualityParam;
			return encoderParams;
		}

        public static ImageCodecInfo GetImageCodecInfo()
		{
			ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
			for (int i = 0; i < codecs.Length; i++)
			{
				if (codecs[i].FormatDescription.Equals("JPEG"))
				{
					return codecs[i];
				}
			}

			throw new ArgumentOutOfRangeException("formatDescription");
		}

		private static Font CreateBarCodeFont()
		{
			int fontSize = 48;
			PrivateFontCollection pfc = new PrivateFontCollection();
			pfc.AddFontFile(HttpContext.Current.Server.MapPath("~/Content/fonts/code39.ttf"));
			FontFamily ff = new FontFamily("Code 39", pfc);
			return new Font(ff, fontSize);
		}

        public static SizeF GetRequiredBitmapSizeForText(Font font, string text)
		{
			SizeF textSize;
			using (Bitmap bmp = new Bitmap(10, 10))
			{
				using (Graphics g = Graphics.FromImage(bmp))
				{
					g.PageUnit = GraphicsUnit.Pixel;
					textSize = g.MeasureString(text, font);
				}
			}
			return textSize;
		}

		private static string GetBarcodeLabel(string id)
		{
			return "*" + id.Trim() + "*";
		}
	}
}
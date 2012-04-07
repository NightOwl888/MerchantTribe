using System;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Web;

namespace MerchantTribe.Commerce.Utilities
{	
	public sealed class ImageHelper
	{

		private ImageHelper()
		{

		}

		public static string SafeImage(string imagePath)
		{
			string result = "~/Images/System/Clear.png";

			if (imagePath.ToLower().StartsWith("http:")) {
				result = imagePath;
			}
			else {
				if (HttpContext.Current != null) {
					if (File.Exists(HttpContext.Current.Server.MapPath(imagePath))) {
						result = imagePath;
					}
				}
			}

			return result;
		}

		public static string GetValidImage(string fileName, bool useAppRoot)
		{
			string result = "images/System/NoImageAvailable.gif";
			if (fileName.Length != 0) {
				// Allow direct URLs as file names
				if ((fileName.ToLower().StartsWith("http://") == false) & (fileName.ToLower().StartsWith("https://") == false)) {
					string fileNameReplaced = fileName.Replace("/", "\\");
					if (fileNameReplaced.StartsWith("\\")) {
						fileNameReplaced = fileNameReplaced.Remove(0, 1);
					}
					if (File.Exists(Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, fileNameReplaced))) {
						result = fileName.Replace("\\", "/");
					}
					if (useAppRoot) {
						if (result.StartsWith("/")) {
							result = "~" + result;
						}
						else {
							result = "~/" + result;
						}
					}
				}
				else {
					result = fileName.Replace("\\", "/");
				}
			}
			else {
				if (useAppRoot) {
					result = "~/" + result;
				}
			}

			return result;
		}

		public static ImageInfo GetImageInformation(string fileName)
		{
			ImageInfo result = new ImageInfo();
			result.Width = 0;
			result.Height = 0;
			result.FormattedDimensions = "unknown";
			result.FormattedSize = "unknown";
			result.SizeInBytes = 0;

			string fullName = string.Empty;
			if (fileName != null) {
				fullName = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, fileName.Replace("/", "\\"));
			}


			if (File.Exists(fullName) == true) {
				FileInfo f = new FileInfo(Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, fileName));
				if (f != null) {

					result.SizeInBytes = f.Length;
					if (result.SizeInBytes < 1024) {
						result.FormattedSize = result.SizeInBytes + " bytes";
					}
					else {
						if (result.SizeInBytes < 1048576) {
                            result.FormattedSize = Math.Round(result.SizeInBytes / 1024, 1, MerchantTribeApplication.Current.CurrentStore.Settings.MidpointRoundingMode) + " KB";
						}
						else {
                            result.FormattedSize = Math.Round(result.SizeInBytes / 1048576, 1, MerchantTribeApplication.Current.CurrentStore.Settings.MidpointRoundingMode) + " MB";
						}
					}

				}
				f = null;

				if (File.Exists(fullName) == true) {
					System.Drawing.Image WorkingImage;
					WorkingImage = System.Drawing.Image.FromFile(fullName);
					if (WorkingImage != null) {
						result.Width = WorkingImage.Width;
						result.Height = WorkingImage.Height;
						result.FormattedDimensions = result.Width.ToString(System.Globalization.CultureInfo.InvariantCulture) + " x " + result.Height.ToString(System.Globalization.CultureInfo.InvariantCulture);
					}
					WorkingImage.Dispose();
					WorkingImage = null;
				}

			}

			return result;
		}

		public static ImageInfo GetProportionalImageDimensionsForImage(ImageInfo oldInfo, int maxWidth, int maxHeight)
		{
			ImageInfo result = new ImageInfo();

			if (oldInfo != null) {
				System.Drawing.Size s = MerchantTribe.Web.Images.GetNewDimensions(maxWidth, maxHeight, oldInfo.Width, oldInfo.Height);
				result.Height = s.Height;
				result.Width = s.Width;
			}

			return result;
		}

		public static bool CompressJpeg(string filePath, long quality)
		{
			bool result = true;
			string fullFile = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, filePath);

			if (File.Exists(fullFile) == true) {

				if (quality < 1L) {
					quality = 1L;
				}
				else {
					if (quality > 100L) {
						quality = 100L;
					}
				}

				System.Drawing.Image WorkingImage;
				WorkingImage = System.Drawing.Image.FromFile(filePath);

				System.Drawing.Image FinalImage;
				FinalImage = new System.Drawing.Bitmap(WorkingImage.Width, WorkingImage.Height, PixelFormat.Format24bppRgb);

				Graphics G = Graphics.FromImage(FinalImage);
				G.InterpolationMode = InterpolationMode.HighQualityBicubic;
				G.PixelOffsetMode = PixelOffsetMode.HighQuality;
				G.CompositingQuality = CompositingQuality.HighQuality;
				G.SmoothingMode = SmoothingMode.HighQuality;
				G.DrawImage(WorkingImage, 0, 0, WorkingImage.Width, WorkingImage.Height);

				// Dispose working Image so we can save with the same name
				WorkingImage.Dispose();
				WorkingImage = null;

				// Compression Code
				ImageCodecInfo myCodec = GetEncoderInfo("image/jpeg");
				Encoder myEncoder = Encoder.Quality;
				EncoderParameters myEncoderParams = new EncoderParameters(1);
				EncoderParameter myParam = new EncoderParameter(myEncoder, quality);
				myEncoderParams.Param[0] = myParam;
				// End Compression Code

				File.Delete(fullFile);
				FinalImage.Save(fullFile, myCodec, myEncoderParams);
				FinalImage.Dispose();
				FinalImage = null;
			}
			else {
				result = false;
			}

			return result;
		}

		private static ImageCodecInfo GetEncoderInfo(string mimeType)
		{
			int j;
			ImageCodecInfo[] encoders;
			encoders = ImageCodecInfo.GetImageEncoders();
			for (j = 0; j <= encoders.Length; j++) {
				if (encoders[j].MimeType == mimeType) {
					return encoders[j];
				}
			}
			return null;
		}

		public static bool ResizeImage(string currentImagePath, string newImagePath, int newHeight, int newWidth)
		{
			try {
				MerchantTribe.Web.Images.ShrinkImageFileOnDisk(currentImagePath, newImagePath, newHeight, newWidth);
				return true;
			}
			catch (Exception Ex) {
				throw new ArgumentException("Image Resize Error: " + Ex.Message);
			}			
		}

	}

}

using System.Windows.Media;

namespace RadarLib
{
  public class MyImage
	{
		ImageSource _image;
		string _name;

		public MyImage(ImageSource image, string name)
		{
			_image = image;
			_name = name;
		}

		public override string ToString()
		{
			return _name;
		}

		public ImageSource Image
		{
			get { return _image; }
		}

		public string Name
		{
			get { return _name; }
		}
	}
}

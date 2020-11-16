using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace AAV.SS
{
	[Serializable, StructLayout(LayoutKind.Sequential)]
	public struct RECT
	{
		public int Left;
		public int Top;
		public int Right;
		public int Bottom;

		public RECT(int left_, int top_, int right_, int bottom_)
		{
			Left = left_;
			Top = top_;
			Right = right_;
			Bottom = bottom_;
		}

		public int Height { get { return Bottom - Top; } }
		public int Width { get { return Right - Left; } }
		public Size Size { get { return new Size(Width, Height); } }

		public Point Location { get { return new Point(Left, Top); } }

		// convert to a System.Drawing.Rectangle
		public Rect ToRectangle()
		{
			return new Rect(Left, Top, Right, Bottom);
		}

		public static RECT FromRectangle(Rect rectangle)
		{
			return new Rect(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
		}

		public override int GetHashCode()
		{
			return Left ^ ((Top << 13) | (Top >> 0x13))
				^ ((Width << 0x1a) | (Width >> 6))
				^ ((Height << 7) | (Height >> 0x19));
		}

		#region Operator overloads

		public static implicit operator Rect(RECT rect)
		{
			return rect.ToRectangle();
		}

		public static implicit operator RECT(Rect rect)
		{
			return FromRectangle(rect);
		}

		#endregion

		public static RECT GetClientRect(IntPtr hWnd)
		{
			RECT result = new RECT();
			GetClientRect(hWnd, out result);
			return result;
		}

		[DllImport("user32.dll")]
		static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);
	}
}

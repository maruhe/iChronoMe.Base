using AColor = Android.Graphics.Color;

namespace iChronoMe.Core.Types
{
    public partial struct xColor
    {
		public AColor ToAndroid()
		{
			return new AColor((byte)(byte.MaxValue * this.R), (byte)(byte.MaxValue * this.G), (byte)(byte.MaxValue * this.B), (byte)(byte.MaxValue * this.A));
		}
	}

	public static class xColorExtention
	{
		public static xColor ToColor(this AColor color)
		{
			return xColor.FromUint((uint)color.ToArgb());
		}
	}

}

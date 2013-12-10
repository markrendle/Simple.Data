using System.Reflection;

namespace Simple.Data.SqlTest.Resources
{
    public static class GetImage
    {
        public static byte[] Image
        {
            get
            {
                var type = typeof(GetImage);
                var s = Assembly.GetAssembly(type).GetManifestResourceStream(type,"test.png");
                try
                {
                    var image = new byte[s.Length];
                    s.Read(image, 0, image.Length);
                    return image;
                }
                finally
                {
                    if (s != null)
                        s.Close();
                }
            }
        }
    }
}
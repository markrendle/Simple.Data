namespace Shitty.Data.SqlTest
{
    public class DbImage
    {
        public byte[] TheImage { get; set; }
    }

    public class Blob
    {
        public int Id { get; set; }
        public byte[] Data { get; set; }
    }
}
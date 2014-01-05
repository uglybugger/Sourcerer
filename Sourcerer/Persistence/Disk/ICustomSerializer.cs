using System.IO;

namespace Sourcerer.Persistence.Disk
{
    public interface ICustomSerializer
    {
        void Serialize(Stream stream, object item);
        T Deserialize<T>(FileStream stream);
    }
}
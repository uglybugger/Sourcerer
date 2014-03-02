using System;
using System.IO;

namespace Sourcerer.Persistence.Disk
{
    public interface ICustomSerializer
    {
        [Obsolete]
        void Serialize(Stream stream, object item);

        byte[] Serialize(object o);

        [Obsolete]
        T Deserialize<T>(Stream stream);

        T Deserialize<T>(byte[] bytes);
    }
}
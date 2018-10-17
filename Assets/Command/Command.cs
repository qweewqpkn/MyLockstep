
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace LockStep
{
    enum CommandType
    {
        eNone = 0,
        eMove = 1,
    }

    abstract class Command : ICommand
    {
        private CommandType mCommandType;

        public abstract void Process();

        public virtual T Deserialize<T>(byte[] bytes)
        {
            MemoryStream ms = new MemoryStream(bytes);
            BinaryFormatter bf = new BinaryFormatter();
            return (T)bf.Deserialize(ms);
        }

        public virtual byte[] Serialize()
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, this);
            return ms.ToArray();
        }
    }
}

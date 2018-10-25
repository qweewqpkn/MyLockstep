using Network;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Game
{
    [Serializable]
    public abstract class Command : ICommand
    {
        public CommandType mCommandType;

        public Command(CommandType type)
        {
            mCommandType = type;
        }

        public abstract void Process();

        public static T Deserialize<T>(byte[] bytes)
        {
            MemoryStream ms = new MemoryStream(bytes);
            BinaryFormatter bf = new BinaryFormatter();
            return (T)bf.Deserialize(ms);
        }

        public static byte[] Serialize(object obj)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }
    }
}

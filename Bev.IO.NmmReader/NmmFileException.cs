using System;
namespace Bev.IO.NmmReader
{
    [Serializable()]
    public class NmmFileException : System.Exception
    {
        public NmmFileException() : base() { }
        public NmmFileException(string message) : base(message) { }
        public NmmFileException(string message, System.Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client. 
        protected NmmFileException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}


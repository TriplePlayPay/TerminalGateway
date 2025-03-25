using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace TerminalGateway.Desktop.WPF.Communications.NewPos
{
    /// <summary>
    /// Base exception for all TPP device-related errors.
    /// </summary>
    [Serializable]
    public class TPPDeviceException : Exception
    {
        // Basic constructor
        public TPPDeviceException() : base() { }

        // Constructor with message
        public TPPDeviceException(string message) : base(message) { }

        // Constructor with message and inner exception
        public TPPDeviceException(string message, Exception innerException)
            : base(message, innerException) { }

        // Constructor for serialization
        protected TPPDeviceException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }

    /// <summary>
    /// Exception thrown when there's a connection issue with the TPP device.
    /// </summary>
    [Serializable]
    public class TPPConnectionException : TPPDeviceException
    {
        public string Host { get; }
        public int Port { get; }

        public TPPConnectionException() : base() { }

        public TPPConnectionException(string message) : base(message) { }

        public TPPConnectionException(string message, Exception innerException)
            : base(message, innerException) { }

        public TPPConnectionException(string message, string host, int port)
            : base(message)
        {
            Host = host;
            Port = port;
        }

        public TPPConnectionException(string message, string host, int port, Exception innerException)
            : base(message, innerException)
        {
            Host = host;
            Port = port;
        }

        protected TPPConnectionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Host = info.GetString(nameof(Host));
            Port = info.GetInt32(nameof(Port));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue(nameof(Host), Host);
            info.AddValue(nameof(Port), Port);

            base.GetObjectData(info, context);
        }
    }

    /// <summary>
    /// Exception thrown when a protocol error occurs in TPP communication.
    /// </summary>
    [Serializable]
    public class TPPProtocolException : TPPDeviceException
    {
        public byte ExpectedVersion { get; }
        public byte ReceivedVersion { get; }

        public TPPProtocolException() : base() { }

        public TPPProtocolException(string message) : base(message) { }

        public TPPProtocolException(string message, Exception innerException)
            : base(message, innerException) { }

        public TPPProtocolException(string message, byte expectedVersion, byte receivedVersion)
            : base(message)
        {
            ExpectedVersion = expectedVersion;
            ReceivedVersion = receivedVersion;
        }

        protected TPPProtocolException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ExpectedVersion = info.GetByte(nameof(ExpectedVersion));
            ReceivedVersion = info.GetByte(nameof(ReceivedVersion));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue(nameof(ExpectedVersion), ExpectedVersion);
            info.AddValue(nameof(ReceivedVersion), ReceivedVersion);

            base.GetObjectData(info, context);
        }
    }

    /// <summary>
    /// Exception thrown when data integrity issues occur (like CRC errors).
    /// </summary>
    [Serializable]
    public class TPPDataIntegrityException : TPPDeviceException
    {
        public uint ExpectedCRC { get; }
        public uint ReceivedCRC { get; }

        public TPPDataIntegrityException() : base() { }

        public TPPDataIntegrityException(string message) : base(message) { }

        public TPPDataIntegrityException(string message, Exception innerException)
            : base(message, innerException) { }

        public TPPDataIntegrityException(string message, uint expectedCRC, uint receivedCRC)
            : base(message)
        {
            ExpectedCRC = expectedCRC;
            ReceivedCRC = receivedCRC;
        }

        protected TPPDataIntegrityException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ExpectedCRC = info.GetUInt32(nameof(ExpectedCRC));
            ReceivedCRC = info.GetUInt32(nameof(ReceivedCRC));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue(nameof(ExpectedCRC), ExpectedCRC);
            info.AddValue(nameof(ReceivedCRC), ReceivedCRC);

            base.GetObjectData(info, context);
        }
    }
}

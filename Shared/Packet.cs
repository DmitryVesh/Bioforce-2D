using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Shared
{
    public enum MainServerToServer
    {
        welcome
    }
    public enum ServerToMainServer
    {
        welcomeReceived,
        serverData,
        shuttingDown
    }

    public enum InternetDiscoveryClientPackets
    {
        firstAskForServers,
        askForServerChanges,
        addServer,
        deletedServer,
        modifiedServer,
        joinServerNamed
    }
    public enum InternetDiscoveryServerPackets
    {
        welcome,
        serverData, //Same as added
        serverDeleted,
        serverModified,
        cantJoinServerDeleted,
        noMoreServersAvailable,
        joinServer
    }

    public enum LANDiscoveryClientPackets
    {

    }
    public enum LANDiscoveryServerPackets
    {
        serverData
    }

    /// <summary>Sent from server to client.</summary>
    public enum ServerPackets
    {
        welcome = 1,
        udpTest,
        spawnPlayer,
        playerPosition,
        playerRotationAndVelocity,
        playerMovementStats,
        playerDisconnect,
        bulleShot,
        playerDied,
        playerRespawned,
        tookDamage,
        serverIsFull,
        armPositionRotation,
        playerPausedGame
    }

    /// <summary>Sent from client to server.</summary>
    public enum ClientPackets
    {
        welcomeReceived = 1,
        udpTestReceived,
        playerMovement,
        playerMovementStats,
        bulletShot,
        playerDied,
        playerRespawned,
        tookDamage,
        armPositionRotation,
        pausedGame
    }

    public class Packet : IDisposable
    {
        private List<byte> ByteBuffer { get; set; }
        private byte[] readableBuffer { get; set; }
        private int CurrentReadPosition;

        /// <summary>Creates a new empty packet (without an ID).</summary>
        public Packet()
        {
            ByteBuffer = new List<byte>(); // Intitialize buffer
            CurrentReadPosition = 0; // Set readPos to 0
        }

        /// <summary>Creates a new packet with a given ID. Used for sending.</summary>
        /// <param name="packetID">The packet ID.</param>
        public Packet(int packetID)
        {
            ByteBuffer = new List<byte>(); // Intitialize buffer
            CurrentReadPosition = 0; // Set readPos to 0

            Write(packetID); // Write packet id to the buffer
        }

        /// <summary>Creates a packet from which data can be read. Used for receiving.</summary>
        /// <param name="bytes">The bytes to add to the packet.</param>
        public Packet(byte[] bytes)
        {
            ByteBuffer = new List<byte>(); // Intitialize buffer
            CurrentReadPosition = 0; // Set readPos to 0

            SetBytes(bytes);
        }

        #region Functions
        /// <summary>Sets the packet's content and prepares it to be read.</summary>
        /// <param name="SetBytes">The bytes to add to the packet.</param>
        public void SetBytes(byte[] SetBytes)
        {
            Write(SetBytes);
            readableBuffer = ByteBuffer.ToArray();
        }

        /// <summary>Inserts the length of the packet's content at the start of the buffer.</summary>
        public void WriteLength() =>
            ByteBuffer.InsertRange(0, BitConverter.GetBytes(ByteBuffer.Count)); // Insert the byte length of the packet at the very beginning


        /// <summary>Gets the packet's content in array form.</summary>
        public byte[] ToArray()
        {
            readableBuffer = ByteBuffer.ToArray();
            return readableBuffer;
        }

        /// <summary>Gets the length of the packet's content.</summary>
        public int Length() =>
            ByteBuffer.Count; // Return the length of buffer

        /// <summary>Gets the length of the unread data contained in the packet.</summary>
        public int UnreadLength() =>
            Length() - CurrentReadPosition; // Return the remaining length (unread)

        /// <summary>Resets the packet instance to allow it to be reused.</summary>
        /// <param name="_shouldReset">Whether or not to reset the packet.</param>
        public void Reset(bool _shouldReset = true)
        {
            if (_shouldReset)
            {
                ByteBuffer.Clear(); // Clear buffer
                readableBuffer = null;
                CurrentReadPosition = 0; // Reset readPos
            }
            else
                CurrentReadPosition -= 4; // "Unread" the last read int
        }
        #endregion

        #region Write Data
        /// <summary>Adds a byte to the packet.</summary>
        /// <param name="_value">The byte to add.</param>
        public void Write(byte _value)
        {
            ByteBuffer.Add(_value);
        }
        /// <summary>Adds an array of bytes to the packet.</summary>
        /// <param name="_value">The byte array to add.</param>
        public void Write(byte[] _value)
        {
            ByteBuffer.AddRange(_value);
        }
        /// <summary>Adds a short to the packet.</summary>
        /// <param name="_value">The short to add.</param>
        public void Write(short _value)
        {
            ByteBuffer.AddRange(BitConverter.GetBytes(_value));
        }
        /// <summary>Adds an int to the packet.</summary>
        /// <param name="_value">The int to add.</param>
        public void Write(int _value)
        {
            ByteBuffer.AddRange(BitConverter.GetBytes(_value));
        }
        /// <summary>Adds a long to the packet.</summary>
        /// <param name="_value">The long to add.</param>
        public void Write(long _value)
        {
            ByteBuffer.AddRange(BitConverter.GetBytes(_value));
        }
        /// <summary>Adds a float to the packet.</summary>
        /// <param name="_value">The float to add.</param>
        public void Write(float _value)
        {
            ByteBuffer.AddRange(BitConverter.GetBytes(_value));
        }
        /// <summary>Adds a bool to the packet.</summary>
        /// <param name="_value">The bool to add.</param>
        public void Write(bool _value)
        {
            ByteBuffer.AddRange(BitConverter.GetBytes(_value));
        }
        /// <summary>Adds a string to the packet.</summary>
        /// <param name="_value">The string to add.</param>
        public void Write(string _value)
        {
            Write(_value.Length); // Add the length of the string to the packet
            ByteBuffer.AddRange(Encoding.ASCII.GetBytes(_value)); // Add the string itself
        }
        /// <summary>Adds a Vector3 to the packet.</summary>
        /// <param name="_value">The Vector3 to add.</param>
        public void Write(Vector3 _value)
        {
            Write(_value.X);
            Write(_value.Y);
            Write(_value.Z);
        }
        /// <summary>Adds a Vector2 to the packet.</summary>
        /// <param name="_value">The Vector2 to add.</param>
        public void Write(Vector2 _value)
        {
            Write(_value.X);
            Write(_value.Y);
        }
        /// <summary>Adds a Quaternion to the packet.</summary>
        /// <param name="_value">The Quaternion to add.</param>
        public void Write(Quaternion _value)
        {
            Write(_value.X);
            Write(_value.Y);
            Write(_value.Z);
            Write(_value.W);
        }
        #endregion

        #region Read Data
        /// <summary>Reads a byte from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public byte ReadByte(bool _moveReadPos = true)
        {
            if (ByteBuffer.Count > CurrentReadPosition)
            {
                // If there are unread bytes
                byte _value = readableBuffer[CurrentReadPosition]; // Get the byte at readPos' position
                if (_moveReadPos)
                {
                    // If _moveReadPos is true
                    CurrentReadPosition += 1; // Increase readPos by 1
                }
                return _value; // Return the byte
            }
            else
            {
                throw new Exception("Could not read value of type 'byte'!");
            }
        }

        /// <summary>Reads an array of bytes from the packet.</summary>
        /// <param name="_length">The length of the byte array.</param>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public byte[] ReadBytes(int _length, bool _moveReadPos = true)
        {
            if (ByteBuffer.Count > CurrentReadPosition)
            {
                // If there are unread bytes
                byte[] _value = ByteBuffer.GetRange(CurrentReadPosition, _length).ToArray(); // Get the bytes at readPos' position with a range of _length
                if (_moveReadPos)
                {
                    // If _moveReadPos is true
                    CurrentReadPosition += _length; // Increase readPos by _length
                }
                return _value; // Return the bytes
            }
            else
            {
                throw new Exception("Could not read value of type 'byte[]'!");
            }
        }

        /// <summary>Reads a short from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public short ReadShort(bool _moveReadPos = true)
        {
            if (ByteBuffer.Count > CurrentReadPosition)
            {
                // If there are unread bytes
                short _value = BitConverter.ToInt16(readableBuffer, CurrentReadPosition); // Convert the bytes to a short
                if (_moveReadPos)
                {
                    // If _moveReadPos is true and there are unread bytes
                    CurrentReadPosition += 2; // Increase readPos by 2
                }
                return _value; // Return the short
            }
            else
            {
                throw new Exception("Could not read value of type 'short'!");
            }
        }

        /// <summary>Reads an int from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public int ReadInt(bool _moveReadPos = true)
        {
            if (ByteBuffer.Count > CurrentReadPosition)
            {
                // If there are unread bytes
                int _value = BitConverter.ToInt32(readableBuffer, CurrentReadPosition); // Convert the bytes to an int
                if (_moveReadPos)
                {
                    // If _moveReadPos is true
                    CurrentReadPosition += 4; // Increase readPos by 4
                }
                return _value; // Return the int
            }
            else
            {
                throw new Exception("Could not read value of type 'int'!");
            }
        }

        /// <summary>Reads a long from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public long ReadLong(bool _moveReadPos = true)
        {
            if (ByteBuffer.Count > CurrentReadPosition)
            {
                // If there are unread bytes
                long _value = BitConverter.ToInt64(readableBuffer, CurrentReadPosition); // Convert the bytes to a long
                if (_moveReadPos)
                {
                    // If _moveReadPos is true
                    CurrentReadPosition += 8; // Increase readPos by 8
                }
                return _value; // Return the long
            }
            else
            {
                throw new Exception("Could not read value of type 'long'!");
            }
        }

        /// <summary>Reads a float from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public float ReadFloat(bool _moveReadPos = true)
        {
            if (ByteBuffer.Count > CurrentReadPosition)
            {
                // If there are unread bytes
                float _value = BitConverter.ToSingle(readableBuffer, CurrentReadPosition); // Convert the bytes to a float
                if (_moveReadPos)
                {
                    // If _moveReadPos is true
                    CurrentReadPosition += 4; // Increase readPos by 4
                }
                return _value; // Return the float
            }
            else
            {
                throw new Exception("Could not read value of type 'float'!");
            }
        }

        /// <summary>Reads a bool from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public bool ReadBool(bool _moveReadPos = true)
        {
            if (ByteBuffer.Count > CurrentReadPosition)
            {
                // If there are unread bytes
                bool _value = BitConverter.ToBoolean(readableBuffer, CurrentReadPosition); // Convert the bytes to a bool
                if (_moveReadPos)
                {
                    // If _moveReadPos is true
                    CurrentReadPosition += 1; // Increase readPos by 1
                }
                return _value; // Return the bool
            }
            else
            {
                throw new Exception("Could not read value of type 'bool'!");
            }
        }

        /// <summary>Reads a string from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public string ReadString(bool _moveReadPos = true)
        {
            try
            {
                int _length = ReadInt(); // Get the length of the string
                string _value = Encoding.ASCII.GetString(readableBuffer, CurrentReadPosition, _length); // Convert the bytes to a string
                if (_moveReadPos && _value.Length > 0)
                {
                    // If _moveReadPos is true string is not empty
                    CurrentReadPosition += _length; // Increase readPos by the length of the string
                }
                return _value; // Return the string
            }
            catch
            {
                throw new Exception("Could not read value of type 'string'!");
            }
        }

        /// <summary>Reads a Vector3 from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public Vector3 ReadVector3(bool _moveReadPos = true)
        {
            try
            {
                Vector3 _value = new Vector3(ReadFloat(_moveReadPos), ReadFloat(_moveReadPos), ReadFloat(_moveReadPos));
                return _value; // Return the Vector3
            }
            catch
            {
                throw new Exception("Could not read value of type 'Vector3'!");
            }
        }
        /// <summary>Reads a Vector2 from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public Vector2 ReadVector2(bool _moveReadPos = true)
        {
            try
            {
                Vector2 _value = new Vector2(ReadFloat(_moveReadPos), ReadFloat(_moveReadPos));
                return _value; // Return the Vector2
            }
            catch
            {
                throw new Exception("Could not read value of type 'Vector2'!");
            }
        }
        /// <summary>Reads a Quaternion from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public Quaternion ReadQuaternion(bool _moveReadPos = true)
        {
            try
            {
                Quaternion _value = new Quaternion(ReadFloat(_moveReadPos), ReadFloat(_moveReadPos), ReadFloat(_moveReadPos), ReadFloat(_moveReadPos));
                return _value; // Return the Quaternion
            }
            catch
            {
                throw new Exception("Could not read value of type 'Quaternion'!");
            }
        }
        #endregion

        private bool Disposed { get; set; } = false;

        protected virtual void Dispose(bool toDispose)
        {
            if (!Disposed)
            {
                if (toDispose)
                {
                    ByteBuffer = null;
                    readableBuffer = null;
                    CurrentReadPosition = 0;
                }

                Disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

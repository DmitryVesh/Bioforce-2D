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
        playerPausedGame,
        stillConnected,
        shouldHost,
        askPlayerDetails,
        freeColor,
        takeColor,
        triedTakingTakenColor
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
        pausedGame,
        stillConnected,
        colorToFreeAndTake,
        readyToJoin
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
        public Packet(byte packetID)
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
        //public void Write(Quaternion _value)
        //{
        //    Write(_value.X);
        //    Write(_value.Y);
        //    Write(_value.Z);
        //    Write(_value.W);
        //}
        /// <summary>Adds a Quaternion to the packet.</summary>
        /// <param name="_value">The Quaternion to add.</param>
        /// 
        /// Smallest 3, find the largest absolute of the floats, don't send it, send the smallest 3, 
        /// and give the index for the largest using 2 bits  [00, 01, 10, 11] 00 = x, 01 = y, 10 = z, 11 = w
        /// Use formula x^2 + y^2 + z^2 + w^2 = 1
        /// 
        /// Also can use less precise floating point value, instead of 4 bytes per component, use 1 byte
        /// 
        /// This should decrease the packet size from 16 bytes = 128 bits
        /// To 2 bit (largest component index) + 3 * 7 bits (smallest 3 components) = 23 bits - will send 3 bytes = 24 bits
        /// only 18.75% of the original packet

        public void Write(Quaternion value)
        {
            float[] components = new float[] { Math.Abs(value.X), Math.Abs(value.Y), Math.Abs(value.Z), Math.Abs(value.W) };

            int largestAbsIndex = GetLargestComponentIndex(components);
            bool[] largestAbsIndexBin = ConvertIntMax4ToBin2(largestAbsIndex);

            byte[] bytesToSend = new byte[3];
            byte byteCounter = 0;

            for (int count = 0; count < 4; count++)
            {
                if (count == largestAbsIndex)
                    continue;
                // Writes the 7 bit representation of the float instead of the 32 bit
                bytesToSend[byteCounter++] = Get7BitFractionalFrom32BitFloat(components[count]);
            }

            //Writes the bit indexes of which component not to read fragmented amongst the bits
            if (largestAbsIndexBin[0])
                bytesToSend[0] += 128;
            if (largestAbsIndexBin[1])
                bytesToSend[1] += 128;

            Write(bytesToSend);
        }

        private byte Get7BitFractionalFrom32BitFloat(float floating32Bit)
        {
            byte BitFraction7 = 0;
            if (floating32Bit == 0 || floating32Bit < MathF.Pow(2, -6))
                return BitFraction7;

            for (byte bitCount = 7 - 1; bitCount >= 0; bitCount--)
            {
                float newCalc = floating32Bit - MathF.Pow(2, -(bitCount - 6));

                if (newCalc < 0)
                    continue;

                byte bitValue = (byte)MathF.Pow(2, bitCount);
                BitFraction7 += bitValue;

                if (newCalc == 0)
                    break;

                floating32Bit = newCalc;
            }

            return BitFraction7;
        }
        private bool[] ConvertIntMax4ToBin2(int largestIndex)
        {
            bool[] bits = new bool[2];

            if (largestIndex > 1)
                bits[1] = true; //Most significant bit
            if (largestIndex % 2 != 0)
                bits[0] = true;

            // Above is a more complex version of this
            //
            //switch (largestIndex)
            //{
            //    case 0:
            //        break;
            //    case 1:
            //        bits[0] = true;
            //        break;
            //    case 2:
            //        bits[1] = true;
            //        break;
            //    case 3:
            //        bits[0] = true;
            //        bits[1] = true;
            //        break;
            //}

            return bits;
        }
        private int GetLargestComponentIndex(float[] components)
        {
            float largest = -1;
            int largestIndex = 0;
            for (int count = 0; count < components.Length; count++)
            {
                float component = components[count];
                if (component > largest)
                {
                    largest = component;
                    largestIndex = count;
                }
            }

            return largestIndex;
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
        //public Quaternion ReadQuaternion(bool _moveReadPos = true)
        //{
        //    try
        //    {
        //        Quaternion _value = new Quaternion(ReadFloat(_moveReadPos), ReadFloat(_moveReadPos), ReadFloat(_moveReadPos), ReadFloat(_moveReadPos));
        //        return _value; // Return the Quaternion
        //    }
        //    catch
        //    {
        //        throw new Exception("Could not read value of type 'Quaternion'!");
        //    }
        //}

        /// <summary>Reads a Quaternion from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        /// 
        /// Reads the same as the Smallest 3 Written version
        /// Reads 3 bytes
        /// Reads the 2 fragmented bits that will indicate the index of the missing component from the 0th and 1st bytes
        /// Read the 3 smallest components
        /// Reconstruct the largest component
        public Quaternion ReadQuaternion(bool _moveReadPos = true)
        {
            try
            {
                byte[] components = ReadBytes(3);

                //if (largestAbsIndexBin[0])
                //    bytesToSend[0] += 128;
                //if (largestAbsIndexBin[1])
                //    bytesToSend[1] += 128;

                byte val0 = components[0];
                bool index0 = val0 - 128 <= 0;

                byte val1 = components[1];
                bool index1 = val1 - 128 <= 0;

                if (index0)
                    val0 -= 128;
                if (index1)
                    val1 -= 128;

                //[00, 01, 10, 11] 00 = x, 01 = y, 10 = z, 11 = w
                //
                // Use formula x^2 + y^2 + z^2 + w^2 = 1
                // a^2 + b^2 + c^2 + r^2 = 1
                // r = sqrt(1 - a^2 - b^2 - c^2)

                float x, y, z, w;

                if (index1 && index0) //Reconstruct w
                    Reconstruct(components, out x, out y, out z, out w);
                else if (index1 && !index0) //Reconstruct z
                    Reconstruct(components, out x, out y, out w, out z);
                else if (!index1 && index0) //Reconstruct y
                    Reconstruct(components, out x, out z, out w, out y);
                else //same as -> else if (!index1 && !index1) //Reconstruct x
                    Reconstruct(components, out y, out z, out w, out x);

                Quaternion value = new Quaternion(x, y, z, w);

                return value; // Return the Quaternion
            }
            catch
            {
                throw new Exception("Could not read value of type 'Quaternion'!");
            }
        }
        private static void Reconstruct(byte[] components, out float a, out float b, out float c, out float r)
        {
            a = Read7BitFloat(components[0]);
            b = Read7BitFloat(components[1]);
            c = Read7BitFloat(components[2]);
            //Reconstruct
            r = MathF.Sqrt(1 - (a * a) - (b * b) - (c * c));
        }
        private static float Read7BitFloat(byte bit7Float)
        {
            float value = 0;
            if (bit7Float == 0)
                return value;

            for (int bitCount = 7 - 1; bitCount >= 0; bitCount--)
            {
                byte bitValue = (byte)MathF.Pow(2, bitCount);
                byte newCalc = (byte)(bit7Float - bitValue);

                if (newCalc < 0)
                    continue;

                float addValue = MathF.Pow(2, -(bitCount - 6));
                value += addValue;

                if (newCalc == 0)
                    break;

                bit7Float = newCalc;
            }

            return value;
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

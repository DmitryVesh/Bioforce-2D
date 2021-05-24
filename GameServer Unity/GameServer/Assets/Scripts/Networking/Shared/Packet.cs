using System;
using System.Collections.Generic;
using System.Text;
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE || UNITY_WEBGL
using UnityEngine;
using Mathf = UnityEngine.Mathf;
#else
using System.Numerics;
using Mathf = System.MathF;
#endif

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
        serverData,
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
        stillConnectedTCP,
        stillConnectedUDP,
        shouldHost,
        askPlayerDetails,
        freeColor,
        takeColor,
        triedTakingTakenColor,
        generatedPickup,
        playerPickedUpItem,
        chatMessage,
        gameState,
        constantPlayerData
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
        stillConnectedTCP,
        stillConnectedUDP,
        colorToFreeAndTake,
        readyToJoin,
        chatMessage,
        constantPlayerData,
        pingAckTCP,
        pingAckUDP
    }

    public class Packet : IDisposable
    {
        private List<byte> Buffer;
        private byte[] ReadableBuffer;
        private int ReadPosition;

        /// <summary>Creates a new empty packet (without an ID).</summary>
        public Packet()
        {
            Buffer = new List<byte>(); // Intitialize buffer
            ReadPosition = 0; // Set readPos to 0
        }

        /// <summary>Creates a new packet with a given ID. Used for sending.</summary>
        /// <param name="id">The packet ID.</param>
        public Packet(byte id)
        {
            Buffer = new List<byte>(); // Intitialize buffer
            ReadPosition = 0; // Set readPos to 0

            Write(id); // Write packet id to the buffer
        }

        /// <summary>Creates a packet from which data can be read. Used for receiving.</summary>
        /// <param name="data">The bytes to add to the packet.</param>
        public Packet(byte[] data)
        {
            Buffer = new List<byte>(); // Intitialize buffer
            ReadPosition = 0; // Set readPos to 0

            SetBytes(data);
        }

#region Functions
        /// <summary>Sets the packet's content and prepares it to be read.</summary>
        /// <param name="data">The bytes to add to the packet.</param>
        public void SetBytes(byte[] data)
        {
            Write(data);
            ReadableBuffer = Buffer.ToArray();
        }

        /// <summary>Inserts the length of the packet's content at the start of the buffer.</summary>
        public void WriteLength()
        {
            // For packetLens that are bigger than 1B , e.g. ushort = 2B, uint = 4B
            //buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count)); // Insert the byte length of the packet at the very beginning

            Buffer.Insert(0, (byte)Buffer.Count); // Insert the byte length of the packet at the very beginning
        }

        /// <summary>Inserts the given int at the start of the buffer.</summary>
        /// <param name="value">The int to insert.</param>
        public void InsertInt(int value)
        {
            Buffer.InsertRange(0, BitConverter.GetBytes(value)); // Insert the int at the start of the buffer
        }
        public void InsertByte(byte value)
        {
            Buffer.Insert(0, value); // Insert the int at the start of the buffer
        }

        /// <summary>Gets the packet's content in array form.</summary>
        public byte[] ToArray()
        {
            ReadableBuffer = Buffer.ToArray();
            return ReadableBuffer;
        }

        /// <summary>Gets the length of the packet's content.</summary>
        public int Length()
        {
            return Buffer.Count; // Return the length of buffer
        }

        /// <summary>Gets the length of the unread data contained in the packet.</summary>
        public int UnreadLength()
        {
            return Length() - ReadPosition; // Return the remaining length (unread)
        }

        /// <summary>Resets the packet instance to allow it to be reused.</summary>
        /// <param name="shouldReset">Whether or not to reset the packet.</param>
        public void Reset(bool shouldReset = true)
        {
            if (shouldReset)
            {
                Buffer.Clear(); // Clear buffer
                ReadableBuffer = null;
                ReadPosition = 0; // Reset readPos
            }
            else
            {
                ReadPosition -= 4; // "Unread" the last read int
            }
        }
#endregion

#region Write Data
        //public void Write(bit)

        /// <summary>Adds a byte to the packet.</summary>
        /// <param name="value">The byte to add.</param>
        public void Write(byte value)
        {
            Buffer.Add(value);
        }
        /// <summary>Adds an array of bytes to the packet.</summary>
        /// <param name="value">The byte array to add.</param>
        public void Write(byte[] value)
        {
            Buffer.AddRange(value);
        }
        /// <summary>Adds a short 2B to the packet.</summary>
        /// <param name="value">The short to add.</param>
        public void Write(short value)
        {
            Buffer.AddRange(BitConverter.GetBytes(value));
        }
        /// <summary>Adds a ushort 2B to the packet.</summary>
        /// <param name="value">The short to add.</param>
        public void Write(ushort value)
        {
            Buffer.AddRange(BitConverter.GetBytes(value));
        }
        /// <summary>Adds an int 4B to the packet.</summary>
        /// <param name="value">The int to add.</param>
        public void Write(int value)
        {
            Buffer.AddRange(BitConverter.GetBytes(value));
        }
        /// <summary>Adds a long 8B to the packet.</summary>
        /// <param name="value">The long to add.</param>
        public void Write(long value)
        {
            Buffer.AddRange(BitConverter.GetBytes(value));
        }
        /// <summary>Adds a float 4B to the packet.</summary>
        /// <param name="value">The float to add.</param>
        public void Write(float value)
        {
            Buffer.AddRange(BitConverter.GetBytes(value));
        }
        /// <summary>Adds a bool 1B to the packet.</summary>
        /// <param name="value">The bool to add.</param>
        public void Write(bool value)
        {
            Buffer.AddRange(BitConverter.GetBytes(value));
        }
        /// <summary>Adds a string x*1B to the packet.</summary>
        /// <param name="value">The string to add.</param>
        public void Write(string value)
        {
            Write(value.Length); // Add the length of the string to the packet
            Buffer.AddRange(Encoding.ASCII.GetBytes(value)); // Add the string itself
        }
        /// <summary>Adds a TimeSpan 8B to the packet.</summary>
        /// <param name="value">The float to add.</param>
        public void Write(TimeSpan value)
        {
            Buffer.AddRange(BitConverter.GetBytes(value.Ticks));
        }

        internal void Write8BoolsAs1Byte(bool bit8, bool bit7, bool bit6, bool bit5, bool bit4, bool bit3, bool bit2, bool bit1)
        {
            byte byteToAdd = 0;
            bool[] bits = new bool[] { bit1, bit2, bit3, bit4, bit5, bit6, bit7, bit8 };

            byte bitValue = 1;
            for (int bitCount = 0; bitCount < bits.Length; bitCount++)
            {
                if (bits[bitCount])
                    byteToAdd += bitValue;

                bitValue *= 2;
            }

            Write(byteToAdd);
        }

#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE || UNITY_WEBGL
        /// <summary>Adds a Vector2 to the packet 4B - 512 >= a >= 0 Limited to Range of 0.0078125 -> 511.9921875.</summary>
        /// <param name="value">The Vector2 to add.</param>
        public void WriteWorldUVector2(UnityEngine.Vector2 value)
        {
            ushort componentsX = GetUShortUnsignedFloatSmallerThan512(value.x);
            ushort componentsY = GetUShortUnsignedFloatSmallerThan512(value.y);

            Write(componentsX);
            Write(componentsY);
        }
#else
        /// <summary>Adds a Vector2 to the packet 4B - 512 >= a >= 0 Limited to Range of 0.0078125 -> 511.9921875.</summary>
        /// <param name="value">The Vector2 to add.</param>
        public void WriteWorldUVector2(System.Numerics.Vector2 value)
        {
            ushort componentsX = GetUShortUnsignedFloatSmallerThan512(value.X);
            ushort componentsY = GetUShortUnsignedFloatSmallerThan512(value.Y);

            Write(componentsX);
            Write(componentsY);
        }
#endif
        private ushort GetUShortUnsignedFloatSmallerThan512(float floatVal)
        {
            if (floatVal < 0 || floatVal > 512)
                throw new FormatException($"Error, can only represent values in the following ranges: 0 <= x <= 512, value given: {floatVal}");

            ushort ushortVal = 0;
            for (sbyte bitIndex = 15; bitIndex >= 0; bitIndex--)
            {
                float takeAway = Mathf.Pow(2f, bitIndex - 7);
                float newVal = floatVal - takeAway;

                if (newVal < 0)
                    continue;

                ushort bitValue = (ushort)Mathf.Pow(2f, bitIndex);
                ushortVal += bitValue;

                floatVal = newVal;
            }

            return ushortVal;
        }

#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE || UNITY_WEBGL
        /// <summary> Adds a 2B Vector2 to the packet, the components are 0.9921875 >= a >= -0.9921875, precision 0.0078125 </summary>
        internal void WriteLocalPosition(UnityEngine.Vector2 localPosition)
        {
            byte[] components = new byte[2];
            components[0] = Get1ByteSignedFloatSmallerThan1(localPosition.x);
            components[1] = Get1ByteSignedFloatSmallerThan1(localPosition.y);

            Write(components);
        }
#else
        /// <summary> Adds a 2B Vector2 to the packet, the components are 0.9921875 >= a >= -0.9921875, precision 0.0078125 </summary>
        internal void WriteLocalPosition(System.Numerics.Vector2 localPosition)
        {
            byte[] components = new byte[2];
            components[0] = Get1ByteSignedFloatSmallerThan1(localPosition.X);
            components[1] = Get1ByteSignedFloatSmallerThan1(localPosition.Y);

            Write(components);
        }
#endif

        /// <summary> Gets 1B representation of a float value, the float must be in range: 0.9921875 >= a >= -0.9921875, precision 0.0078125 </summary>
        private static byte Get1ByteSignedFloatSmallerThan1(float val)
        {
            float valAbs = Mathf.Abs(val);
            if (valAbs > 1)
                throw new FormatException($"Error, can only represent values in the following ranges: x < 1, value given: {valAbs}");

            byte byteVal = 0;
            if (val < 0) //Add negative sign bit
                byteVal += 128;
            GetByteFromAbsFloatSmallerThan1(valAbs, ref byteVal, 6);

            return byteVal;
        }
        private static void GetByteFromAbsFloatSmallerThan1(float valAbs, ref byte byteVal, sbyte maxBitIndex)
        {
            for (sbyte bitCount = maxBitIndex; bitCount >= 0; bitCount--)
            {
                float takeAway = Mathf.Pow(2f, bitCount - (maxBitIndex + 1));
                float newVal = valAbs - takeAway;

                if (newVal < 0)
                    continue;

                byte bitValue = (byte)Mathf.Pow(2, bitCount);
                byteVal += bitValue;

                //if (newVal == 0)
                //    break;

                valAbs = newVal;
            }
        }

#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE || UNITY_WEBGL
        /// <summary>Adds a Quaternion to the packet.</summary>
        /// <param name="value">The Quaternion to add.</param>
        /// 
        /// Smallest 3, find the largest absolute of the floats, don't send it, send the smallest 3, 
        /// and give the index for the largest using 2 bits  [00, 01, 10, 11] 00 = x, 01 = y, 10 = z, 11 = w
        /// Use formula x^2 + y^2 + z^2 + w^2 = 1
        /// 
        /// Also can use less precise floating point value, instead of 4 bytes per component, use 1 byte
        /// 
        /// This should decrease the packet size from 16 bytes = 128 bits
        /// To 2 bit (largest component index) + 3 * 7 bits (smallest 3 components) + 1 bit for negating all components = 24 bits - will send 3 bytes = 24 bits
        /// only 18.75% of the original packet
        public void Write(UnityEngine.Quaternion value)
        {
            float[] componentsAbs = new float[] { Math.Abs(value.x), Math.Abs(value.y), Math.Abs(value.z), Math.Abs(value.w) };
            float[] components = new float[] { value.x, value.y, value.z, value.w };            

            int largestAbsIndex = GetLargestComponentIndex(componentsAbs);
            bool largestCompNegative = components[largestAbsIndex] < 0;
            bool[] largestAbsIndexBin = ConvertIntMax4ToBin2(largestAbsIndex);

            byte[] bytesToSend = new byte[3];
            byte byteCounter = 0;

            for (int count = 0; count < 4; count++)
            {
                if (count == largestAbsIndex)
                    continue;
                // Writes the 7 bit representation of the float instead of the 32 bit
                bytesToSend[byteCounter++] = Get7BitFractionalFrom32BitFloat(componentsAbs[count], components[count] < 0);
            }

            //Writes the bit indexes of which component not to read fragmented amongst the bits
            if (largestAbsIndexBin[0])
                bytesToSend[0] += 128;
            if (largestAbsIndexBin[1])
                bytesToSend[1] += 128;

            if (largestCompNegative) //Should negate the smallest 3 (*-1) components
                bytesToSend[2] += 128;

            Write(bytesToSend);
        }
#else
        /// <summary>Adds a Quaternion to the packet.</summary>
        /// <param name="value">The Quaternion to add.</param>
        /// 
        /// Smallest 3, find the largest absolute of the floats, don't send it, send the smallest 3, 
        /// and give the index for the largest using 2 bits  [00, 01, 10, 11] 00 = x, 01 = y, 10 = z, 11 = w
        /// Use formula x^2 + y^2 + z^2 + w^2 = 1
        /// 
        /// Also can use less precise floating point value, instead of 4 bytes per component, use 1 byte
        /// 
        /// This should decrease the packet size from 16 bytes = 128 bits
        /// To 2 bit (largest component index) + 3 * 7 bits (smallest 3 components) + 1 bit for negating all components = 24 bits - will send 3 bytes = 24 bits
        /// only 18.75% of the original packet
        public void Write(System.Numerics.Quaternion value)
        {
            float[] componentsAbs = new float[] { Math.Abs(value.X), Math.Abs(value.Y), Math.Abs(value.Z), Math.Abs(value.W) };
            float[] components = new float[] { value.X, value.Y, value.Z, value.W };

            int largestAbsIndex = GetLargestComponentIndex(componentsAbs);
            bool largestCompNegative = components[largestAbsIndex] < 0;
            bool[] largestAbsIndexBin = ConvertIntMax4ToBin2(largestAbsIndex);

            byte[] bytesToSend = new byte[3];
            byte byteCounter = 0;

            for (int count = 0; count < 4; count++)
            {
                if (count == largestAbsIndex)
                    continue;
                // Writes the 7 bit representation of the float instead of the 32 bit
                bytesToSend[byteCounter++] = Get7BitFractionalFrom32BitFloat(componentsAbs[count], components[count] < 0);
            }

            //Writes the bit indexes of which component not to read fragmented amongst the bits
            if (largestAbsIndexBin[0])
                bytesToSend[0] += 128;
            if (largestAbsIndexBin[1])
                bytesToSend[1] += 128;

            if (largestCompNegative) //Should negate the smallest 3 (*-1) components
                bytesToSend[2] += 128;

            Write(bytesToSend);
        }
#endif
        const float SmallestFloatCanRepresent = 1 / 64; //2^-6
        private static byte Get7BitFractionalFrom32BitFloat(float floating32BitAbs, bool negative)
        {
            byte BitFraction7 = 0;
            if (floating32BitAbs == 0 || floating32BitAbs < SmallestFloatCanRepresent)
                return BitFraction7;

            if (negative) //Negative sign
                BitFraction7 += 64;

            /*
            for (sbyte bitCount = 5; bitCount >= 0; bitCount--)
            {
                float takeAway = Mathf.Pow(2f, bitCount - 5);
                float newCalc = floating32BitAbs - takeAway;

                if (newCalc < 0)
                    continue;

                byte bitValue = (byte)Mathf.Pow(2, bitCount);
                BitFraction7 += bitValue;

                if (newCalc == 0)
                    break;

                floating32BitAbs = newCalc;
            }
            */
            GetByteFromAbsFloatSmallerThan1(floating32BitAbs, ref BitFraction7, 5);

            return BitFraction7;
        }
        private static bool[] ConvertIntMax4ToBin2(int largestIndex)
        {
            bool[] bits = new bool[2];

            if (largestIndex > 1)
                bits[1] = true; //Most significant bit
            if (largestIndex % 2 != 0)
                bits[0] = true;

            // Above is a more complex version of this
            /*
            switch (largestIndex)
            {
                case 0:
                    break;
                case 1:
                    bits[0] = true;
                    break;
                case 2:
                    bits[1] = true;
                    break;
                case 3:
                    bits[0] = true;
                    bits[1] = true;
                    break;
            }
            */

            return bits;
        }
        private static int GetLargestComponentIndex(float[] componentsAbs)
        {
            float largest = -1;
            int largestIndex = 0;
            for (int count = 0; count < componentsAbs.Length; count++)
            {
                float component = componentsAbs[count];
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
        /// <summary>
        /// Gets the packetLen, currently set to byte, so max packet size is 255B
        /// Can increase to ushort so max packet size is 65_535B
        /// Or uint, max packet size 4_294_967_295B
        /// </summary>
        public byte ReadPacketLen() =>
            ReadByte();

        /// <summary>Reads a byte from the packet.</summary>
        /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
        public byte ReadByte(bool moveReadPos = true)
        {
            if (Buffer.Count > ReadPosition)
            {
                // If there are unread bytes
                byte value = ReadableBuffer[ReadPosition]; // Get the byte at readPos' position
                if (moveReadPos)
                {
                    // If moveReadPos is true
                    ReadPosition += 1; // Increase readPos by 1
                }
                return value; // Return the byte
            }
            else
            {
                throw new Exception("Could not read value of type 'byte'!");
            }
        }

        /// <summary>Reads an array of bytes from the packet.</summary>
        /// <param name="length">The length of the byte array.</param>
        /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
        public byte[] ReadBytes(byte length, bool moveReadPos = true)
        {
            if (Buffer.Count > ReadPosition)
            {
                // If there are unread bytes
                byte[] value = Buffer.GetRange(ReadPosition, length).ToArray(); // Get the bytes at readPos' position with a range of length
                if (moveReadPos)
                {
                    // If moveReadPos is true
                    ReadPosition += length; // Increase readPos by length
                }
                return value; // Return the bytes
            }
            else
            {
                throw new Exception("Could not read value of type 'byte[]'!");
            }
        }

        /// <summary>Reads a short from the packet.</summary>
        /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
        public short ReadShort(bool moveReadPos = true)
        {
            if (Buffer.Count > ReadPosition)
            {
                // If there are unread bytes
                short value = BitConverter.ToInt16(ReadableBuffer, ReadPosition); // Convert the bytes to a short
                if (moveReadPos)
                {
                    // If moveReadPos is true and there are unread bytes
                    ReadPosition += 2; // Increase readPos by 2
                }
                return value; // Return the short
            }
            else
            {
                throw new Exception("Could not read value of type 'short'!");
            }
        }
        /// <summary>Reads a ushort from the packet.</summary>
        /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
        public ushort ReadUShort(bool moveReadPos = true)
        {
            if (Buffer.Count > ReadPosition)
            {
                // If there are unread bytes
                ushort value = BitConverter.ToUInt16(ReadableBuffer, ReadPosition); // Convert the bytes to a short
                if (moveReadPos)
                {
                    // If moveReadPos is true and there are unread bytes
                    ReadPosition += 2; // Increase readPos by 2
                }
                return value; // Return the ushort
            }
            else
            {
                throw new Exception("Could not read value of type 'short'!");
            }
        }
        /// <summary>Reads an int from the packet.</summary>
        /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
        public int ReadInt(bool moveReadPos = true)
        {
            if (Buffer.Count > ReadPosition)
            {
                // If there are unread bytes
                int value = BitConverter.ToInt32(ReadableBuffer, ReadPosition); // Convert the bytes to an int
                if (moveReadPos)
                {
                    // If moveReadPos is true
                    ReadPosition += 4; // Increase readPos by 4
                }
                return value; // Return the int
            }
            else
            {
                throw new Exception("Could not read value of type 'int'!");
            }
        }

        /// <summary>Reads a long from the packet.</summary>
        /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
        public long ReadLong(bool moveReadPos = true)
        {
            if (Buffer.Count > ReadPosition)
            {
                // If there are unread bytes
                long value = BitConverter.ToInt64(ReadableBuffer, ReadPosition); // Convert the bytes to a long
                if (moveReadPos)
                {
                    // If moveReadPos is true
                    ReadPosition += 8; // Increase readPos by 8
                }
                return value; // Return the long
            }
            else
            {
                throw new Exception("Could not read value of type 'long'!");
            }
        }

        /// <summary>Reads a float from the packet.</summary>
        /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
        public float ReadFloat(bool moveReadPos = true)
        {
            if (Buffer.Count > ReadPosition)
            {
                // If there are unread bytes
                float value = BitConverter.ToSingle(ReadableBuffer, ReadPosition); // Convert the bytes to a float
                if (moveReadPos)
                {
                    // If moveReadPos is true
                    ReadPosition += 4; // Increase readPos by 4
                }
                return value; // Return the float
            }
            else
            {
                throw new Exception("Could not read value of type 'float'!");
            }
        }

        /// <summary>Reads a bool from the packet.</summary>
        /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
        public bool ReadBool(bool moveReadPos = true)
        {
            if (Buffer.Count > ReadPosition)
            {
                // If there are unread bytes
                bool value = BitConverter.ToBoolean(ReadableBuffer, ReadPosition); // Convert the bytes to a bool
                if (moveReadPos)
                {
                    // If moveReadPos is true
                    ReadPosition += 1; // Increase readPos by 1
                }
                return value; // Return the bool
            }
            else
            {
                throw new Exception("Could not read value of type 'bool'!");
            }
        }

        /// <summary>Reads a string from the packet.</summary>
        /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
        public string ReadString(bool moveReadPos = true)
        {
            try
            {
                int length = ReadInt(); // Get the length of the string
                string value = Encoding.ASCII.GetString(ReadableBuffer, ReadPosition, length); // Convert the bytes to a string
                if (moveReadPos && value.Length > 0)
                {
                    // If moveReadPos is true string is not empty
                    ReadPosition += length; // Increase readPos by the length of the string
                }
                return value; // Return the string
            }
            catch
            {
                throw new Exception("Could not read value of type 'string'!");
            }
        }

        /// <summary>Reads a TimeSpan 8B from the packet.</summary>
        /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
        public TimeSpan ReadTimeSpan(bool moveReadPos = true)
        {
            if (Buffer.Count > ReadPosition)
            {
                // If there are unread bytes
                TimeSpan value = new TimeSpan(BitConverter.ToInt64(ReadableBuffer, ReadPosition)); // Convert the bytes to a long
                if (moveReadPos)
                {
                    // If moveReadPos is true
                    ReadPosition += 8; // Increase readPos by 8
                }
                return value; // Return the TimeSpan
            }
            else
            {
                throw new Exception("Could not read value of type 'TimeSpan'!");
            }
        }

#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE || UNITY_WEBGL
        /// <summary>Reads a Unsigned Vector2 from the packet, Limited to Range of 0.0078125 -> 511.9921875.</summary>
        /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
        /// 2B for each component, so 4B in total
        public UnityEngine.Vector2 ReadUVector2WorldPosition(bool moveReadPos = true)
        {
            try
            {
                ushort valX = ReadUShort(moveReadPos);
                ushort valY = ReadUShort(moveReadPos);

                float componentX = GetUnsignedFloatSmallerThan512FromUShort(valX);
                float componentY = GetUnsignedFloatSmallerThan512FromUShort(valY);

                UnityEngine.Vector2 value = new UnityEngine.Vector2(componentX, componentY);
                return value; // Return the Vector2
            }
            catch
            {
                throw new Exception("Could not read value of type 'Vector2'!");
            }
        }
#else
        /// <summary>Reads a Unsigned Vector2 from the packet, Limited to Range of 0.0078125 -> 511.9921875.</summary>
        /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
        /// 2B for each component, so 4B in total
        public System.Numerics.Vector2 ReadUVector2WorldPosition(bool moveReadPos = true)
        {
            try
            {
                ushort valX = ReadUShort(moveReadPos);
                ushort valY = ReadUShort(moveReadPos);

                float componentX = GetUnsignedFloatSmallerThan512FromUShort(valX);
                float componentY = GetUnsignedFloatSmallerThan512FromUShort(valY);

                System.Numerics.Vector2 value = new System.Numerics.Vector2(componentX, componentY);
                return value; // Return the Vector2
            }
            catch
            {
                throw new Exception("Could not read value of type 'Vector2'!");
            }
        }
#endif
        private float GetUnsignedFloatSmallerThan512FromUShort(ushort ushortVal)
        {
            float floatVal = 0;
            for (sbyte bitIndex = 15; bitIndex >= 0; bitIndex--)
            {
                ushort takeAway = (ushort)Mathf.Pow(2f, bitIndex);
                int newUShortVal = ushortVal - takeAway;

                if (newUShortVal < 0)
                    continue;

                float addVal = Mathf.Pow(2f, bitIndex - 7);
                floatVal += addVal;

                ushortVal = (ushort)newUShortVal;
            }
            return floatVal;
        }

#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE || UNITY_WEBGL
        internal UnityEngine.Vector2 ReadLocalVector2()
        {
            byte[] components = ReadBytes(2);

            float x = GetSignedFloatSmallerThan1FromByte(components[0]);
            float y = GetSignedFloatSmallerThan1FromByte(components[1]);

            return new UnityEngine.Vector2(x, y);
        }
#else
        internal System.Numerics.Vector2 ReadLocalVector2()
        {
            byte[] components = ReadBytes(2);

            float x = GetSignedFloatSmallerThan1FromByte(components[0]);
            float y = GetSignedFloatSmallerThan1FromByte(components[1]);

            return new System.Numerics.Vector2(x, y);
        }
#endif
        private static float GetSignedFloatSmallerThan1FromByte(byte byteVal)
        {
            float floatVal = 0;

            bool isNegative = false;
            if (byteVal - 128 >= 0) //Is negative
            {
                isNegative = true;
                byteVal -= 128;
            }

            GetAbsFloatSmallerThan1From1Byte(byteVal, ref floatVal, 6);

            return isNegative ? -floatVal : floatVal;
        }

        private static void GetAbsFloatSmallerThan1From1Byte(byte byteVal, ref float floatVal, sbyte maxBitIndex)
        {
            for (sbyte bitCount = maxBitIndex; bitCount >= 0; bitCount--)
            {
                byte bitValue = (byte)Mathf.Pow(2, bitCount);
                short newByteVal = (short)(byteVal - bitValue);

                if (newByteVal < 0)
                    continue;

                float addVal = Mathf.Pow(2f, bitCount - (maxBitIndex + 1));
                floatVal += addVal;
                byteVal = (byte)newByteVal;
            }
        }

#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE || UNITY_WEBGL
        /// <summary>Reads a Quaternion from the packet.</summary>
        /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
        /// 
        /// Reads the same as the Smallest 3 Written version
        /// Reads 3 bytes
        /// Reads the 2 fragmented bits that will indicate the index of the missing component from the 0th and 1st bytes
        /// Read the 3 smallest components
        /// Reconstruct the largest component
        public UnityEngine.Quaternion ReadQuaternion(bool moveReadPos = true)
        {
            try
            {
                byte[] components = ReadBytes(3);

                //if (largestAbsIndexBin[0])
                //    bytesToSend[0] += 128;
                //if (largestAbsIndexBin[1])
                //    bytesToSend[1] += 128;

                bool index0 = components[0] - 128 >= 0;
                bool index1 = components[1] - 128 >= 0;

                bool shouldNegateComponents = components[2] - 128 >= 0;

                if (index0)
                    components[0] -= 128;
                if (index1)
                    components[1] -= 128;

                if (shouldNegateComponents)
                    components[2] -= 128;

                //[00, 01, 10, 11] 00 = x, 01 = y, 10 = z, 11 = w
                //
                // Use formula x^2 + y^2 + z^2 + w^2 = 1
                // a^2 + b^2 + c^2 + r^2 = 1
                // r = sqrt(1 - a^2 - b^2 - c^2)

                float x, y, z, w;

                if (index1 && index0) //Reconstruct w
                    Reconstruct(components, out x, out y, out z, out w, shouldNegateComponents);
                else if (index1 && !index0) //Reconstruct z
                    Reconstruct(components, out x, out y, out w, out z, shouldNegateComponents);
                else if (!index1 && index0) //Reconstruct y
                    Reconstruct(components, out x, out z, out w, out y, shouldNegateComponents);
                else //same as -> else if (!index1 && !index1) //Reconstruct x
                    Reconstruct(components, out y, out z, out w, out x, shouldNegateComponents);

                UnityEngine.Quaternion value = new UnityEngine.Quaternion(x, y, z, w);

                return value; // Return the Quaternion
            }
            catch
            {
                throw new Exception("Could not read value of type 'Quaternion'!");
            }
        }
#else
        /// <summary>Reads a Quaternion from the packet.</summary>
        /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
        /// 
        /// Reads the same as the Smallest 3 Written version
        /// Reads 3 bytes
        /// Reads the 2 fragmented bits that will indicate the index of the missing component from the 0th and 1st bytes
        /// Read the 3 smallest components
        /// Reconstruct the largest component
        public System.Numerics.Quaternion ReadQuaternion(bool moveReadPos = true)
        {
            try
            {
                byte[] components = ReadBytes(3);

                //if (largestAbsIndexBin[0])
                //    bytesToSend[0] += 128;
                //if (largestAbsIndexBin[1])
                //    bytesToSend[1] += 128;

                bool index0 = components[0] - 128 >= 0;
                bool index1 = components[1] - 128 >= 0;

                bool shouldNegateComponents = components[2] - 128 >= 0;

                if (index0)
                    components[0] -= 128;
                if (index1)
                    components[1] -= 128;

                if (shouldNegateComponents)
                    components[2] -= 128;

                //[00, 01, 10, 11] 00 = x, 01 = y, 10 = z, 11 = w
                //
                // Use formula x^2 + y^2 + z^2 + w^2 = 1
                // a^2 + b^2 + c^2 + r^2 = 1
                // r = sqrt(1 - a^2 - b^2 - c^2)

                float x, y, z, w;

                if (index1 && index0) //Reconstruct w
                    Reconstruct(components, out x, out y, out z, out w, shouldNegateComponents);
                else if (index1 && !index0) //Reconstruct z
                    Reconstruct(components, out x, out y, out w, out z, shouldNegateComponents);
                else if (!index1 && index0) //Reconstruct y
                    Reconstruct(components, out x, out z, out w, out y, shouldNegateComponents);
                else //same as -> else if (!index1 && !index1) //Reconstruct x
                    Reconstruct(components, out y, out z, out w, out x, shouldNegateComponents);

                System.Numerics.Quaternion value = new System.Numerics.Quaternion(x, y, z, w);

                return value; // Return the Quaternion
            }
            catch
            {
                throw new Exception("Could not read value of type 'Quaternion'!");
            }
        }
#endif
        private static void Reconstruct(byte[] components, out float a, out float b, out float c, out float r, bool shouldNegate)
        {
            a = Read7BitFloat(components[0]);
            b = Read7BitFloat(components[1]);
            c = Read7BitFloat(components[2]);
            //Reconstruct
            r = Mathf.Sqrt(1 - (a * a) - (b * b) - (c * c));

            if (shouldNegate)
            {
                a = -a;
                b = -b;
                c = -c;
            }
        }
        private static float Read7BitFloat(byte bit7Float)
        {
            float value = 0;
            if (bit7Float == 0)
                return value;

            bool negative = bit7Float - 64 >= 0;
            if (negative)
                bit7Float -= 64;

            /*
            for (sbyte bitCount = 5; bitCount >= 0; bitCount--)
            {
                byte bitValue = (byte)Mathf.Pow(2, bitCount);
                short newCalc = (short)(bit7Float - bitValue);

                if (newCalc < 0)
                    continue;

                float addValue = Mathf.Pow(2, bitCount - 5);
                value += addValue;

                if (newCalc == 0)
                    break;

                bit7Float = (byte)newCalc;
            }
            */
            GetAbsFloatSmallerThan1From1Byte(bit7Float, ref value, 5);

            return negative ? -value : value;
        }

        internal bool[] Read1ByteAs8Bools()
        {
            bool[] bits = new bool[8];

            short shortByteToRead = (short)ReadByte();  // short So doesn't cause a wrap around/ overflow, e.g. 127 - 128 -> -1 = 255 as byte
                                                        // Can't be sbyte, because then it can't represent max val 255
            byte bitValue = 128;
            for (int bitCount = bits.Length - 1; bitCount >= 0; bitCount--)
            {
                short result = (short)(shortByteToRead - bitValue);
                if (result >= 0)
                {
                    bits[bitCount] = true;
                    shortByteToRead = result;

                    if (shortByteToRead == 0)
                        break;                                        
                }

                bitValue /= 2;
            }

            return bits;
        }

        #endregion

        private bool Disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    Buffer = null;
                    ReadableBuffer = null;
                    ReadPosition = 0;
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
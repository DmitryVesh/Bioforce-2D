using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Mathematics;
using UnityEngine;

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
    stillConnected,
    shouldHost,
    askPlayerDetails,
    freeColor,
    takeColor,
    triedTakingTakenColor,
    generatedPickup,
    playerPickedUpItem
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
    readyToJoin,
    pickedUpItem
}

public class Packet : IDisposable
{
    private List<byte> buffer;
    private byte[] readableBuffer;
    private int readPos;

    /// <summary>Creates a new empty packet (without an ID).</summary>
    public Packet()
    {
        buffer = new List<byte>(); // Intitialize buffer
        readPos = 0; // Set readPos to 0
    }

    /// <summary>Creates a new packet with a given ID. Used for sending.</summary>
    /// <param name="_id">The packet ID.</param>
    public Packet(byte _id)
    {
        buffer = new List<byte>(); // Intitialize buffer
        readPos = 0; // Set readPos to 0

        Write(_id); // Write packet id to the buffer
    }

    /// <summary>Creates a packet from which data can be read. Used for receiving.</summary>
    /// <param name="_data">The bytes to add to the packet.</param>
    public Packet(byte[] _data)
    {
        buffer = new List<byte>(); // Intitialize buffer
        readPos = 0; // Set readPos to 0

        SetBytes(_data);
    }

    #region Functions
    /// <summary>Sets the packet's content and prepares it to be read.</summary>
    /// <param name="_data">The bytes to add to the packet.</param>
    public void SetBytes(byte[] _data)
    {
        Write(_data);
        readableBuffer = buffer.ToArray();
    }

    /// <summary>Inserts the length of the packet's content at the start of the buffer.</summary>
    public void WriteLength()
    {
        buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count)); // Insert the byte length of the packet at the very beginning
    }

    /// <summary>Inserts the given int at the start of the buffer.</summary>
    /// <param name="_value">The int to insert.</param>
    public void InsertInt(int _value)
    {
        buffer.InsertRange(0, BitConverter.GetBytes(_value)); // Insert the int at the start of the buffer
    }
    public void InsertByte(byte _value)
    {
        buffer.Insert(0, _value); // Insert the int at the start of the buffer
    }

    /// <summary>Gets the packet's content in array form.</summary>
    public byte[] ToArray()
    {
        readableBuffer = buffer.ToArray();
        return readableBuffer;
    }

    /// <summary>Gets the length of the packet's content.</summary>
    public int Length()
    {
        return buffer.Count; // Return the length of buffer
    }

    /// <summary>Gets the length of the unread data contained in the packet.</summary>
    public int UnreadLength()
    {
        return Length() - readPos; // Return the remaining length (unread)
    }

    /// <summary>Resets the packet instance to allow it to be reused.</summary>
    /// <param name="_shouldReset">Whether or not to reset the packet.</param>
    public void Reset(bool _shouldReset = true)
    {
        if (_shouldReset)
        {
            buffer.Clear(); // Clear buffer
            readableBuffer = null;
            readPos = 0; // Reset readPos
        }
        else
        {
            readPos -= 4; // "Unread" the last read int
        }
    }
    #endregion

    #region Write Data
    //public void Write(bit)

    /// <summary>Adds a byte to the packet.</summary>
    /// <param name="_value">The byte to add.</param>
    public void Write(byte _value)
    {
        buffer.Add(_value);
    }
    /// <summary>Adds an array of bytes to the packet.</summary>
    /// <param name="_value">The byte array to add.</param>
    public void Write(byte[] _value)
    {
        buffer.AddRange(_value);
    }
    /// <summary>Adds a short to the packet.</summary>
    /// <param name="_value">The short to add.</param>
    public void Write(short _value)
    {
        buffer.AddRange(BitConverter.GetBytes(_value));
    }
    /// <summary>Adds an int to the packet.</summary>
    /// <param name="_value">The int to add.</param>
    public void Write(int _value)
    {
        buffer.AddRange(BitConverter.GetBytes(_value));
    }
    /// <summary>Adds a long to the packet.</summary>
    /// <param name="_value">The long to add.</param>
    public void Write(long _value)
    {
        buffer.AddRange(BitConverter.GetBytes(_value));
    }
    /// <summary>Adds a float to the packet.</summary>
    /// <param name="_value">The float to add.</param>
    public void Write(float _value)
    {
        buffer.AddRange(BitConverter.GetBytes(_value));
    }
    /// <summary>Adds a bool to the packet.</summary>
    /// <param name="_value">The bool to add.</param>
    public void Write(bool _value)
    {
        buffer.AddRange(BitConverter.GetBytes(_value));
    }
    /// <summary>Adds a string to the packet.</summary>
    /// <param name="_value">The string to add.</param>
    public void Write(string _value)
    {
        Write(_value.Length); // Add the length of the string to the packet
        buffer.AddRange(Encoding.ASCII.GetBytes(_value)); // Add the string itself
    }
    /// <summary>Adds a Vector3 to the packet.</summary>
    /// <param name="_value">The Vector3 to add.</param>
    public void Write(Vector3 _value)
    {
        Write(_value.x);
        Write(_value.y);
        Write(_value.z);
    }
    /// <summary>Adds a Vector2 to the packet.</summary>
    /// <param name="_value">The Vector2 to add.</param>
    public void Write(Vector2 _value)
    {
        Write(_value.x);
        Write(_value.y);
    }

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

    const float SmallestFloatCanRepresent = 1/64; //2^-6
    private byte Get7BitFractionalFrom32BitFloat(float floating32BitAbs, bool negative)
    {
        byte BitFraction7 = 0;
        if (floating32BitAbs == 0 || floating32BitAbs < SmallestFloatCanRepresent)
            return BitFraction7;

        if (negative) //Negative sign
            BitFraction7 += 64;

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
    private int GetLargestComponentIndex(float[] componentsAbs)
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
    /// <summary>Reads a byte from the packet.</summary>
    /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
    public byte ReadByte(bool _moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {
            // If there are unread bytes
            byte _value = readableBuffer[readPos]; // Get the byte at readPos' position
            if (_moveReadPos)
            {
                // If _moveReadPos is true
                readPos += 1; // Increase readPos by 1
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
        if (buffer.Count > readPos)
        {
            // If there are unread bytes
            byte[] _value = buffer.GetRange(readPos, _length).ToArray(); // Get the bytes at readPos' position with a range of _length
            if (_moveReadPos)
            {
                // If _moveReadPos is true
                readPos += _length; // Increase readPos by _length
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
        if (buffer.Count > readPos)
        {
            // If there are unread bytes
            short _value = BitConverter.ToInt16(readableBuffer, readPos); // Convert the bytes to a short
            if (_moveReadPos)
            {
                // If _moveReadPos is true and there are unread bytes
                readPos += 2; // Increase readPos by 2
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
        if (buffer.Count > readPos)
        {
            // If there are unread bytes
            int _value = BitConverter.ToInt32(readableBuffer, readPos); // Convert the bytes to an int
            if (_moveReadPos)
            {
                // If _moveReadPos is true
                readPos += 4; // Increase readPos by 4
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
        if (buffer.Count > readPos)
        {
            // If there are unread bytes
            long _value = BitConverter.ToInt64(readableBuffer, readPos); // Convert the bytes to a long
            if (_moveReadPos)
            {
                // If _moveReadPos is true
                readPos += 8; // Increase readPos by 8
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
        if (buffer.Count > readPos)
        {
            // If there are unread bytes
            float _value = BitConverter.ToSingle(readableBuffer, readPos); // Convert the bytes to a float
            if (_moveReadPos)
            {
                // If _moveReadPos is true
                readPos += 4; // Increase readPos by 4
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
        if (buffer.Count > readPos)
        {
            // If there are unread bytes
            bool _value = BitConverter.ToBoolean(readableBuffer, readPos); // Convert the bytes to a bool
            if (_moveReadPos)
            {
                // If _moveReadPos is true
                readPos += 1; // Increase readPos by 1
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
            string _value = Encoding.ASCII.GetString(readableBuffer, readPos, _length); // Convert the bytes to a string
            if (_moveReadPos && _value.Length > 0)
            {
                // If _moveReadPos is true string is not empty
                readPos += _length; // Increase readPos by the length of the string
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

            Quaternion value = new Quaternion(x, y, z, w);

            return value; // Return the Quaternion
        }
        catch
        {
            throw new Exception("Could not read value of type 'Quaternion'!");
        }
    }
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

        return negative ? -value : value;
    }
    #endregion

    private bool disposed = false;

    protected virtual void Dispose(bool _disposing)
    {
        if (!disposed)
        {
            if (_disposing)
            {
                buffer = null;
                readableBuffer = null;
                readPos = 0;
            }

            disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

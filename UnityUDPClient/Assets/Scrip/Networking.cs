using System;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

public class Networking
{

    public enum IPVersion : int
    {
        IPUnknown,
        IPv4,
        IPv6
    }
    public enum SocketOption : int
    {
        TCP_NoDelay
    }
    public enum SocketType : int
    {
        TCP,
        UDP
    }
    public enum PResult : int
    {
        P_Success,
        P_GenericError
    }

    [StructLayout (LayoutKind.Sequential)]
    public struct SocketData
    {
        public IPVersion m_IPVersion;
        public UInt64 m_hnd;
    };

    [StructLayout (LayoutKind.Sequential)]
    public struct IPEndpointData
    {
        public IPVersion m_IPVersion;
        [MarshalAs (UnmanagedType.LPStr)]
        [NativeDisableUnsafePtrRestriction] public IntPtr m_hostname;
        [MarshalAs (UnmanagedType.LPStr)]
        [NativeDisableUnsafePtrRestriction] public IntPtr m_ipString;
        public uint m_ipBytesSize;
        [NativeDisableUnsafePtrRestriction] public IntPtr m_ipBytes;
        public short m_port;
    };

    const string DLL = "Networking.DLL";

    //ERROR//

    ///<summary>
    ///gets the last error that happened
    ///</summary>
    [DllImport (DLL)] private static extern IntPtr getLastNetworkError (int idk);
    public static string getLastNetworkError ()
    {
        IntPtr ptr = getLastNetworkError (0);
        return Marshal.PtrToStringAnsi (ptr);
    }

    //NETWORK//

    ///<summary>
    ///initializes Winsock
    ///</summary>
    [DllImport (DLL)] public static extern bool initNetwork ();
    ///<summary>
    ///Shutdown Winsoc
    ///</summary>
    [DllImport (DLL)] public static extern void shutdownNetwork ();

    //ENDPOINT//

    ///<summary>
    ///Creates a new IPEndpoint handle for a given IP and port. Multiple IPEndpoints can be created
    ///</summary>
    [DllImport (DLL)] public static extern IPEndpointData createIPEndpointData ([MarshalAs (UnmanagedType.LPStr)] string ip, short port);

    //SOCKET//

    ///<summary>
    ///Creates a new Socket handle for manageing IPEndpoints. 
    ///</summary>
    [DllImport (DLL)] public static extern SocketData initSocketData ();
    ///<summary>
    ///initializes the socket to use UDP or TCP conection types
    ///</summary>
    [DllImport (DLL)] public static extern PResult createSocket (ref SocketData soc, SocketType typ = SocketType.TCP);

    ///<summary>
    ///closes socket so it can not be used unless createSocket() is called once again.
    ///</summary>
    [DllImport (DLL)] public static extern PResult closeSocket (ref SocketData soc);

    ///<summary>
    ///Bind Endpoint to socket.
    ///</summary>
    [DllImport (DLL)] public static extern PResult bindEndpointToSocket (ref IPEndpointData ip, ref SocketData soc);
    ///<summary>
    ///Listens for endpoint connection to the socket. It will block until a new connection is found. 
    ///</summary>
    [DllImport (DLL)] public static extern PResult listenEndpointToSocket (ref IPEndpointData ip, ref SocketData soc, int backlog = 5);
    ///<summary>
    ///Attempts to accept the listened connection. 
    ///</summary>
    [DllImport (DLL)] public static extern PResult acceptSocket (ref SocketData soc, ref SocketData outsoc);
    ///<summary>
    ///Connects endpoint to socket
    ///</summary>
    [DllImport (DLL)] public static extern PResult connectEndpoint (ref IPEndpointData ip, ref SocketData soc);

    // Sending Data
    // TCP

    [DllImport (DLL)] private static extern PResult sendPacketData (ref SocketData soc, IntPtr data, int datasize, ref int bytesSent);
    ///<summary>
    ///Send packet over TCP server. Not guaranteed to send all bytes.
    ///</summary>
    public static PResult sendPacket<T> (SocketData soc, ref T data, int datasize, ref int bytesSent)
    {
        IntPtr tmp = Marshal.AllocHGlobal (Marshal.SizeOf<T> (data));
        Marshal.StructureToPtr (data, tmp, true);
        PResult res = sendPacketData (ref soc, tmp, datasize, ref bytesSent);
        Marshal.FreeHGlobal (tmp);

        return res;
    }
    ///<summary>
    ///Send packet over TCP server. Not guaranteed to send all bytes.
    ///</summary>
    public static PResult sendPacket (SocketData soc, ref string data, int datasize, ref int bytesSent)
    {
        IntPtr tmp = Marshal.StringToHGlobalAnsi (data);
        PResult res = sendPacketData (ref soc, tmp, datasize, ref bytesSent);

        return res;
    }
    ///<summary>
    ///Send packet over TCP server. Not guaranteed to send all bytes.
    ///</summary>
    public static PResult sendPacket (SocketData soc, ref string data, int datasize)
    {
        IntPtr tmp = Marshal.StringToHGlobalAnsi (data);
        int bytesSent = 0;
        PResult res = sendPacketData (ref soc, tmp, datasize, ref bytesSent);

        return res;
    }

    [DllImport (DLL)] private static extern PResult recvPacketData (ref SocketData soc, IntPtr dest, int numberOfBytes, ref int bytesRecv);
    ///<summary>
    ///Receive packet over TCP server. Not guaranteed to recieve all bytes.
    ///</summary>
    public static PResult recvPacket<T> (ref SocketData soc, ref T dest, int numberOfBytes, ref int bytesRecv)
    {

        IntPtr tmp = Marshal.AllocHGlobal (Marshal.SizeOf<T> (dest));
        Marshal.StructureToPtr (dest, tmp, true);
        PResult res = recvPacketData (ref soc, tmp, numberOfBytes, ref bytesRecv);
        if (res == PResult.P_GenericError)
            dest = Marshal.PtrToStructure<T> (tmp);
        Marshal.FreeHGlobal (tmp);

        return res;
    }
    ///<summary>
    ///Receive packet over TCP server. Not guaranteed to recieve all bytes.
    ///</summary>
    public static PResult recvPacket (ref SocketData soc, ref string dest, int numberOfBytes, ref int bytesRecv)
    {

        IntPtr tmp = Marshal.AllocHGlobal (numberOfBytes);
        PResult res = recvPacketData (ref soc, tmp, numberOfBytes, ref bytesRecv);
        dest = Marshal.PtrToStringAnsi (tmp);
        Marshal.FreeHGlobal (tmp);

        return res;
    }
    ///<summary>
    ///Receive packet over TCP server. Not guaranteed to recieve all bytes.
    ///</summary>
    public static PResult recvPacket (ref SocketData soc, ref string dest, int numberOfBytes)
    {

        IntPtr tmp = Marshal.AllocHGlobal (numberOfBytes);
        int bytesRecv = 0;
        PResult res = recvPacketData (ref soc, tmp, numberOfBytes, ref bytesRecv);
        dest = Marshal.PtrToStringAnsi (tmp);
        Marshal.FreeHGlobal (tmp);

        return res;
    }

    [DllImport (DLL)] private static extern PResult sendAllPacketData (ref SocketData soc, IntPtr data, int numberOfBytes);
    ///<summary>
    ///Send entire packet over TCP server. guaranteed to send all bytes.
    ///</summary>
    public static PResult sendAllPacket<T> (ref SocketData soc, ref T data, int numberOfBytes)
    {
        IntPtr tmp = Marshal.AllocHGlobal (Marshal.SizeOf<T> (data));
        Marshal.StructureToPtr (data, tmp, true);
        PResult res = sendAllPacketData (ref soc, tmp, numberOfBytes);

        Marshal.FreeHGlobal (tmp);

        return res;
    }
    ///<summary>
    ///Send entire packet over TCP server. guaranteed to send all bytes.
    ///</summary>
    public static PResult sendAllPacket (ref SocketData soc, ref string data, int numberOfBytes)
    {
        IntPtr tmp = Marshal.StringToHGlobalAnsi (data);
        PResult res = sendAllPacketData (ref soc, tmp, numberOfBytes);

        return res;
    }
    ///<summary>
    ///Send entire packet over TCP server. guaranteed to send all bytes.
    ///</summary>
    public static PResult sendAllPacket (ref SocketData soc, ref string data)
    {
        IntPtr tmp = Marshal.StringToHGlobalAnsi (data);
        PResult res = sendAllPacketData (ref soc, tmp, data.Length + 1);

        return res;
    }

    [DllImport (DLL)] private static extern PResult recvAllPacketData (ref SocketData soc, IntPtr dest, int numberOfBytes);
    ///<summary>
    ///Receive entire packet over TCP server. guaranteed to recieve all bytes.
    ///</summary>
    public static PResult recvAllPacket<T> (ref SocketData soc, ref T dest, int numberOfBytes)
    {
        IntPtr tmp = Marshal.AllocHGlobal (Marshal.SizeOf<T> (dest));
        Marshal.StructureToPtr (dest, tmp, true);
        PResult res = recvAllPacketData (ref soc, tmp, numberOfBytes);
        if (res == PResult.P_GenericError)
            dest = Marshal.PtrToStructure<T> (tmp);
        Marshal.FreeHGlobal (tmp);

        return res;
    }
    ///<summary>
    ///Receive entire packet over TCP server. guaranteed to recieve all bytes.
    ///</summary>
    public static PResult recvAllPacket (ref SocketData soc, ref string dest, int numberOfBytes)
    {
        IntPtr tmp = Marshal.AllocHGlobal (numberOfBytes);
        PResult res = recvAllPacketData (ref soc, tmp, numberOfBytes);
        dest = Marshal.PtrToStringAnsi (tmp);
        Marshal.FreeHGlobal (tmp);

        return res;
    }

    //UDP

    [DllImport (DLL)] private static extern PResult recvFromPacketDataEx (ref SocketData soc, IntPtr data, int numberOfBytes, ref int bytesSent, ref IPEndpointData ip);
    ///<summary>
    ///Receive packet over UDP server. Not guaranteed to recieve all bytes.
    ///</summary>
    public static PResult recvFromPacket<T> (ref SocketData soc, ref T data, int numberOfBytes, ref int bytesRecv, ref IPEndpointData ip)
    {
        IntPtr tmp = Marshal.AllocHGlobal (Marshal.SizeOf<T> (data));
        Marshal.StructureToPtr (data, tmp, true);
        PResult res = recvFromPacketDataEx (ref soc, tmp, numberOfBytes, ref bytesRecv, ref ip);
        if (bytesRecv > 0)
            data = Marshal.PtrToStructure<T> (tmp);
        Marshal.FreeHGlobal (tmp);

        return res;
    }
    ///<summary>
    ///Receive packet over UDP server. Not guaranteed to recieve all bytes.
    ///</summary>
    public static PResult recvFromPacket (ref SocketData soc, ref string data, int numberOfBytes, ref int bytesSent, ref IPEndpointData ip)
    {
        IntPtr tmp = Marshal.AllocHGlobal (numberOfBytes); //allocates to unmanaged memory
        PResult res = recvFromPacketDataEx (ref soc, tmp, numberOfBytes, ref bytesSent, ref ip);
        data = Marshal.PtrToStringAnsi (tmp);
        Marshal.FreeHGlobal (tmp);
        return res;
    }

    [DllImport (DLL)] private static extern PResult recvFromPacketData (ref SocketData soc, IntPtr data, int numberOfBytes, ref IPEndpointData ip);
    ///<summary>
    ///Receive packet over UDP server. Not guaranteed to recieve all bytes.
    ///</summary>
    public static PResult recvFromPacket<T> (ref SocketData soc, ref T data, int numberOfBytes, ref IPEndpointData ip)
    {
        IntPtr tmp = Marshal.AllocHGlobal (Marshal.SizeOf<T> (data));
        Marshal.StructureToPtr (data, tmp, true);
        PResult res = recvFromPacketData (ref soc, tmp, numberOfBytes, ref ip);
        if (res == PResult.P_GenericError)
            data = Marshal.PtrToStructure<T> (tmp);
        Marshal.FreeHGlobal (tmp);

        return res;
    }
    ///<summary>
    ///Receive packet over UDP server. Not guaranteed to recieve all bytes.
    ///</summary>
    public static PResult recvFromPacket (ref SocketData soc, ref string data, int numberOfBytes, ref IPEndpointData ip)
    {
        IntPtr tmp = Marshal.AllocHGlobal (numberOfBytes);
        PResult res = recvFromPacketData (ref soc, tmp, numberOfBytes, ref ip);
        data = Marshal.PtrToStringAnsi (tmp);
        Marshal.FreeHGlobal (tmp);

        return res;
    }

    [DllImport (DLL)] private static extern PResult sendToPacketDataEx (ref SocketData soc, IntPtr data, int numberOfBytes, ref int bytesSent, ref IPEndpointData ip);
    ///<summary>
    ///Send packet over UDP server. Not guaranteed to send all bytes.
    ///</summary>
    public static PResult sendToPacket<T> (ref SocketData soc, ref T data, int numberOfBytes, ref int bytesSent, ref IPEndpointData ip)
    {
        IntPtr tmp = Marshal.AllocHGlobal (Marshal.SizeOf<T> (data));
        Marshal.StructureToPtr (data, tmp, true);
        PResult res = sendToPacketDataEx (ref soc, tmp, numberOfBytes, ref bytesSent, ref ip);
        Marshal.FreeHGlobal (tmp);

        return res;
    }
    ///<summary>
    ///Send packet over UDP server. Not guaranteed to send all bytes.
    ///</summary>
    public static PResult sendToPacket (ref SocketData soc, ref string data, int numberOfBytes, ref int bytesSent, ref IPEndpointData ip)
    {
        IntPtr tmp = Marshal.StringToHGlobalAnsi (data);
        PResult res = sendToPacketDataEx (ref soc, tmp, numberOfBytes, ref bytesSent, ref ip);

        return res;
    }
    ///<summary>
    ///Send packet over UDP server. Not guaranteed to send all bytes.
    ///</summary>
    public static PResult sendToPacket (ref SocketData soc, ref string data, ref int bytesSent, ref IPEndpointData ip)
    {
        IntPtr tmp = Marshal.StringToHGlobalAnsi (data);
        PResult res = sendToPacketDataEx (ref soc, tmp, data.Length + 1, ref bytesSent, ref ip);

        return res;
    }

    [DllImport (DLL)] private static extern PResult sendToPacketData (ref SocketData soc, IntPtr data, int numberOfBytes, ref IPEndpointData ip);
    ///<summary>
    ///Send packet over UDP server. Not guaranteed to send all bytes.
    ///</summary>
    public static PResult sendToPacket<T> (ref SocketData soc, ref T data, int numberOfBytes, ref IPEndpointData ip)
    {
        IntPtr tmp = Marshal.AllocHGlobal (Marshal.SizeOf<T> (data));
        Marshal.StructureToPtr (data, tmp, true);
        PResult res = sendToPacketData (ref soc, tmp, numberOfBytes, ref ip);
        Marshal.FreeHGlobal (tmp);

        return res;
    }
    ///<summary>
    ///Send packet over UDP server. Not guaranteed to send all bytes.
    ///</summary>
    public static PResult sendToPacket (ref SocketData soc, ref string data, int numberOfBytes, ref IPEndpointData ip)
    {
        IntPtr tmp = Marshal.StringToHGlobalAnsi (data);
        PResult res = sendToPacketData (ref soc, tmp, numberOfBytes, ref ip);

        return res;
    }
    ///<summary>
    ///Send packet over UDP server. Not guaranteed to send all bytes.
    ///</summary>
    public static PResult sendToPacket (ref SocketData soc, ref string data, ref IPEndpointData ip)
    {
        IntPtr tmp = Marshal.StringToHGlobalAnsi (data);
        PResult res = sendToPacketData (ref soc, tmp, data.Length + 1, ref ip);

        return res;
    }

}
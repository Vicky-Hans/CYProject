using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine;
#endif

namespace DH.Foundations.FileSystem
{
    /// <summary>
    /// 安卓文件系统流。
    /// </summary>
    public class AndroidFileSystemStream : FileSystemStream
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        private class InternalArgs
        {
            public readonly string SplitFlag = "!/assets/";
            public readonly int SplitFlagLength;
            public readonly AndroidJavaObject s_AssetManager = null;
            public readonly IntPtr s_InternalReadMethodId = IntPtr.Zero;
            public readonly jvalue[] s_InternalReadArgs = null;
            public readonly AndroidJavaClass s_Channel = null;
            public readonly IntPtr s_BufferReadMethodId = IntPtr.Zero;
            public readonly jvalue[] s_BufferReadArgs = null;
            public readonly IntPtr s_BufferResetMethodId = IntPtr.Zero;
            private readonly bool runInMainThread;
            
            public InternalArgs(bool runMainThread)
            {
                runInMainThread = runMainThread;
                try
                {
                    AttachCurrentThread();
                    SplitFlagLength = SplitFlag.Length;
                    AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    s_Channel = new AndroidJavaClass("java.nio.channels.Channels");
                    if (unityPlayer == null)
                    {
                        throw new Exception("Unity player is invalid.");
                    }

                    if (s_Channel == null)
                    {
                        throw new Exception($"AndroidFileSystemStream found Channels Failed");
                    }


                    AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    if (currentActivity == null)
                    {
                        throw new Exception("Current activity is invalid.");
                    }

                    AndroidJavaObject assetManager = currentActivity.Call<AndroidJavaObject>("getAssets");
                    if (assetManager == null)
                    {
                        throw new Exception("Asset manager is invalid.");
                    }

                    s_AssetManager = assetManager;

                    IntPtr inputStreamClassPtr = AndroidJNI.FindClass("java/io/InputStream");
                    s_InternalReadMethodId = AndroidJNIHelper.GetMethodID(inputStreamClassPtr, "read", "([BII)I");
                    s_InternalReadArgs = new jvalue[3];

                    AndroidJNI.DeleteLocalRef(inputStreamClassPtr);


                    IntPtr channelClassPtr = AndroidJNI.FindClass("java/nio/channels/ReadableByteChannel");
                    s_BufferReadMethodId =
                        AndroidJNIHelper.GetMethodID(channelClassPtr, "read", "(Ljava/nio/ByteBuffer;)I");
                    s_BufferReadArgs = new jvalue[1];
                    AndroidJNI.DeleteLocalRef(channelClassPtr);

                    IntPtr bufferClassPtr = AndroidJNI.FindClass("java.nio.ByteBuffer");
                    s_BufferResetMethodId = AndroidJNIHelper.GetMethodID(bufferClassPtr, "clear");
                    AndroidJNI.DeleteLocalRef(bufferClassPtr);

                    currentActivity.Dispose();
                    unityPlayer.Dispose();

                }
                finally
                {
                    DetachCurrentThread();
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void AttachCurrentThread()
            {
                if (!runInMainThread)
                {
                    AttachCurrentThread();
                }
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void DetachCurrentThread()
            {
                if (!runInMainThread)
                {
                    DetachCurrentThread();
                }
            }
        }

        private static InternalArgs Args;
        private readonly AndroidJavaObject m_FileStream;
        private readonly IntPtr m_FileStreamRawObject;
        private readonly AndroidJavaObject m_StreamChannel;
        private readonly IntPtr m_StreamChannelRawObject;

        private static void CheckInternalArgs(bool runInMainThread)
        {
            if (Args == null)
            {
                Args = new InternalArgs(runInMainThread);
            }
        }
#endif
        private readonly bool runInMainThread;
        /// <summary>
        /// 初始化安卓文件系统流的新实例。
        /// </summary>
        /// <param name="fullPath">要加载的文件系统的完整路径。</param>
        /// <param name="access">要加载的文件系统的访问方式。</param>
        /// <param name="createNew">是否创建新的文件系统流。</param>
        public AndroidFileSystemStream(string fullPath, FileSystemAccess access, bool createNew)
        {
            runInMainThread = Thread.CurrentThread.ManagedThreadId == 1;
#if UNITY_ANDROID && !UNITY_EDITOR
            CheckInternalArgs(runInMainThread);
            
            if (string.IsNullOrEmpty(fullPath))
            {
                throw new Exception("Full path is invalid.");
            }

            if (access != FileSystemAccess.Read)
            {
                throw new Exception($"'{access.ToString()}' is not supported in AndroidFileSystemStream.");
            }

            if (createNew)
            {
                throw new Exception("Create new is not supported in AndroidFileSystemStream.");
            }

            int position = fullPath.LastIndexOf(Args.SplitFlag, StringComparison.Ordinal);
            if (position < 0)
            {
                throw new Exception("Can not find split flag in full path.");
            }

            string fileName = fullPath.Substring(position + Args.SplitFlagLength);
            try
            {
                AttachCurrentThread();
                m_FileStream = InternalOpen(fileName);
                if (m_FileStream == null)
                {
                    throw new Exception($"Open file '{fullPath}' from Android asset manager failure.");
                }

                m_FileStreamRawObject = m_FileStream.GetRawObject();
                m_StreamChannel = Args.s_Channel.CallStatic<AndroidJavaObject>("newChannel", m_FileStream);
                if (m_StreamChannel == null)
                {
                    throw new Exception(
                        $"Open file '{fullPath}' from Android asset manager failure with create channel failed");
                }

                m_StreamChannelRawObject = m_StreamChannel.GetRawObject();
            }
            finally
            {
                DetachCurrentThread();
            }
#endif
        }

        /// <summary>
        /// 获取或设置文件系统流位置。
        /// </summary>
        protected internal override long Position
        {
            get { throw new Exception("Get position is not supported in AndroidFileSystemStream."); }
            set { Seek(value, SeekOrigin.Begin); }
        }

        /// <summary>
        /// 获取文件系统流长度。
        /// </summary>
        public override long Length
        {
            get { return InternalAvailable(); }
        }

        /// <summary>
        /// 设置文件系统流长度。
        /// </summary>
        /// <param name="length">要设置的文件系统流的长度。</param>
        protected override void SetLength(long length)
        {
            throw new Exception("SetLength is not supported in AndroidFileSystemStream.");
        }

        /// <summary>
        /// 定位文件系统流位置。
        /// </summary>
        /// <param name="offset">要定位的文件系统流位置的偏移。</param>
        /// <param name="origin">要定位的文件系统流位置的方式。</param>
        public override void Seek(long offset, SeekOrigin origin)
        {

            if (origin == SeekOrigin.End)
            {
                Seek(Length + offset, SeekOrigin.Begin);
                return;
            }

            if (origin == SeekOrigin.Begin)
            {
                InternalReset();
            }

            while (offset > 0)
            {
                long skip = InternalSkip(offset);
                if (skip < 0)
                {
                    return;
                }

                offset -= skip;
            }

        }

        /// <summary>
        /// 从文件系统流中读取一个字节。
        /// </summary>
        /// <returns>读取的字节，若已经到达文件结尾，则返回 -1。</returns>
        public override int ReadByte()
        {
            return InternalRead();
        }

        /// <summary>
        /// 从文件系统流中读取二进制流。
        /// </summary>
        /// <param name="buffer">存储读取文件内容的二进制流。</param>
        /// <param name="startIndex">存储读取文件内容的二进制流的起始位置。</param>
        /// <param name="length">存储读取文件内容的二进制流的长度。</param>
        /// <returns>实际读取了多少字节。</returns>
        public override int Read(byte[] buffer, int startIndex, int length)
        {
            try
            {
                byte[] result = null;
                int bytesRead = InternalRead(length, out result);
                Array.Copy(result, 0, buffer, startIndex, bytesRead);
                Debug.Log($"bytes read {bytesRead}");
                return bytesRead;
            }
            catch (Exception e)
            {
                Debug.Log(e);
                return 0;
            }
        }

        public int ReadWithPtr(IntPtr buffer, int length)
        {
            return InternalRead(buffer,length);
        }

        /// <summary>
        /// 向文件系统流中写入一个字节。
        /// </summary>
        /// <param name="value">要写入的字节。</param>
        public override void WriteByte(byte value)
        {
            throw new Exception("WriteByte is not supported in AndroidFileSystemStream.");
        }

        /// <summary>
        /// 向文件系统流中写入二进制流。
        /// </summary>
        /// <param name="buffer">存储写入文件内容的二进制流。</param>
        /// <param name="startIndex">存储写入文件内容的二进制流的起始位置。</param>
        /// <param name="length">存储写入文件内容的二进制流的长度。</param>
        public override void Write(byte[] buffer, int startIndex, int length)
        {
            throw new Exception("Write is not supported in AndroidFileSystemStream.");
        }

        /// <summary>
        /// 将文件系统流立刻更新到存储介质中。
        /// </summary>
        public override void Flush()
        {
            throw new Exception("Flush is not supported in AndroidFileSystemStream.");
        }

        /// <summary>
        /// 关闭文件系统流。
        /// </summary>
        public override void Close()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            InternalClose();
            m_FileStream.Dispose();
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private AndroidJavaObject InternalOpen(string fileName)
        {
            CheckInternalArgs(runInMainThread);
            return Args.s_AssetManager.Call<AndroidJavaObject>("open", fileName);
        }
#endif

        private int InternalAvailable()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                AttachCurrentThread();
                var result = m_FileStream.Call<int>("available");
                return result;
            }
            finally
            {
                DetachCurrentThread();
            }
#else
            return 0;
#endif
        }

        private void InternalClose()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                AttachCurrentThread();
                m_FileStream.Call("close");
            }
            finally
            {
                DetachCurrentThread();
            }
#endif
        }

        private int InternalRead()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                AttachCurrentThread();
                var result = m_FileStream.Call<int>("read");
                return result;
            }
            finally
            {
                DetachCurrentThread();
            }
#else
            return 0;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AttachCurrentThread()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!runInMainThread)
            {
                AndroidJNI.AttachCurrentThread();
            }
#endif
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DetachCurrentThread()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!runInMainThread)
            {
                AndroidJNI.DetachCurrentThread();
            }
#endif
        }

        private int InternalRead(IntPtr resultPtr,int length)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            CheckInternalArgs(runInMainThread);
            Args.s_BufferReadArgs[0] = new jvalue() { l = resultPtr };
            int offset = 0;
            int bytesLeft = length;
            try
            {
                AttachCurrentThread();
                AndroidJNI.CallVoidMethod(resultPtr, Args.s_BufferResetMethodId, new jvalue[0]);
                while (bytesLeft > 0)
                {
                    int bytesRead =
                        AndroidJNI.CallIntMethod(m_StreamChannelRawObject, Args.s_BufferReadMethodId,
                            Args.s_BufferReadArgs);
                    if (bytesRead <= 0)
                    {
                        break;
                    }

                    offset += bytesRead;
                    bytesLeft -= bytesRead;
                }
            }
            finally
            {
                DetachCurrentThread();
            }

            return offset;
#else
            return 0;
#endif
        }

        private int InternalRead(int length, out byte[] result)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                CheckInternalArgs(runInMainThread);
                AttachCurrentThread();
                IntPtr resultPtr = AndroidJNI.NewByteArray(length);
                int offset = 0;
                int bytesLeft = length;
                while (bytesLeft > 0)
                {
                    Args.s_InternalReadArgs[0] = new jvalue() { l = resultPtr };
                    Args.s_InternalReadArgs[1] = new jvalue() { i = offset };
                    Args.s_InternalReadArgs[2] = new jvalue() { i = bytesLeft };
                    int bytesRead =
                        AndroidJNI.CallIntMethod(m_FileStreamRawObject, Args.s_InternalReadMethodId,
                            Args.s_InternalReadArgs);
                    if (bytesRead <= 0)
                    {
                        break;
                    }

                    offset += bytesRead;
                    bytesLeft -= bytesRead;
                }
                result = (byte[])(Array)AndroidJNI.FromByteArray(resultPtr);
                AndroidJNI.DeleteLocalRef(resultPtr);
                return offset;
            }
            finally
            {
                DetachCurrentThread();
            }
#else
            result = Array.Empty<byte>();
            return 0;
#endif
        }

        private void InternalReset()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                AttachCurrentThread();
                m_FileStream.Call("mark", new int[] { 0 });
                m_FileStream.Call("reset");
            }
            finally
            {
                DetachCurrentThread();
            }
#endif
        }

        private long InternalSkip(long offset)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                var result = m_FileStream.Call<long>("skip", offset);
                return result;
            }
            finally
            {
                DetachCurrentThread();
            }
#else
            return 0;
#endif
        }
    }
}
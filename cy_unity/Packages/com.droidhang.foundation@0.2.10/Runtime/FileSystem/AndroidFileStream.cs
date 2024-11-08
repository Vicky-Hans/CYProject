using System;
using System.IO;
using System.Threading;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace DH.Foundations.FileSystem
{
    /// <summary>
    /// 非线程安全，主要用于Unity部分读取Android平台只读文件
    /// </summary>
    public class AndroidFileStream : Stream
    {
        private AndroidFileSystemStream stream;
        private NativeArray<byte> cacheBuffer;
        private int cacheBytesCount;
        private int cacheBufferOffset;
        private string path;
        /// <summary>
        /// 缓存二进制流的长度。
        /// </summary>
        protected int bufferSize;

        private IntPtr androidByteArray;

        public AndroidFileStream(string fullPath, FileSystemAccess access, bool createNew,int bufferSize = 0x1000)
        {
            stream = new AndroidFileSystemStream(fullPath, FileSystemAccess.Read, false);
            path = fullPath;
            if(bufferSize > 0)
            {
                this.bufferSize = bufferSize;
                cacheBuffer = new NativeArray<byte>(bufferSize, Allocator.Persistent);
                androidByteArray = AndroidJNI.NewDirectByteBuffer(cacheBuffer);
            }
        }
        
        public override void Flush()
        {
            throw new System.NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (stream == null)
            {
                throw new ObjectDisposedException ("Stream has been closed");
            }
            
            int remainCount = count;
            int readTotalCount = 0;
            while (remainCount > 0 && cacheBytesCount > 0 && cacheBufferOffset < cacheBytesCount)
            {
                int dataCount = cacheBytesCount - cacheBufferOffset;
                int preferCount = remainCount > dataCount ? dataCount : remainCount;
                NativeArray<byte>.Copy(cacheBuffer,cacheBufferOffset,buffer,offset,preferCount);
                offset += preferCount;
                cacheBufferOffset += preferCount;
                readTotalCount += preferCount;
                remainCount -= preferCount;
                if (cacheBufferOffset >= cacheBytesCount)
                {
                    break;
                }
            }

            if (remainCount <= 0)
            {
                return readTotalCount;
            }

            cacheBufferOffset = 0;
            cacheBytesCount = stream.ReadWithPtr(androidByteArray, bufferSize);
            //Debug.Log($"{path} Read bytes count {cacheBytesCount} need length {count} bufferSize {bufferSize} IntPtr {androidByteArray} thread id {Thread.CurrentThread.ManagedThreadId}");
            if (cacheBytesCount == 0)
            {
                return readTotalCount;
            }
            
            readTotalCount += Read(buffer, offset, remainCount);
            return readTotalCount;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (stream == null || offset != 0)
            {
                throw new ObjectDisposedException ("Stream has been closed");
            }
            // 设置缓存数据失效
            //Debug.Log($"{path} seek {offset} thread id {Thread.CurrentThread.ManagedThreadId}");
            cacheBufferOffset = cacheBytesCount = 0;
            stream.Seek(offset, origin);
            return 0;
        }

        public override void SetLength(long value)
        {
            throw new System.NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => stream?.Length ?? throw new ObjectDisposedException ("Stream has been closed");

        public override long Position
        {
            get => stream?.Position  ?? throw new ObjectDisposedException ("Stream has been closed");
            set
            {
                if (stream == null)
                {
                    throw new ObjectDisposedException ("Stream has been closed");
                }
                
                stream.Position = value;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (stream != null)
            {
                stream.Close();
                cacheBuffer.Dispose();
                stream = null;
            }

            if (disposing)
            {
                GC.SuppressFinalize (this);
            }
        }
    }
}
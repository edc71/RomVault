// DeflateStream.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2009-2010 Dino Chiesa.
// All rights reserved.
//
// This code module is part of DotNetZip, a zipfile class library.
//
// ------------------------------------------------------------------
//
// This code is licensed under the Microsoft Public License.
// See the file License.txt for the license details.
// More info on: http://dotnetzip.codeplex.com
//
// ------------------------------------------------------------------
//
// last saved (in emacs):
// Time-stamp: <2011-July-31 14:48:11>
//
// ------------------------------------------------------------------
//
// This module defines the DeflateStream class, which can be used as a replacement for
// the System.IO.Compression.DeflateStream class in the .NET BCL.
//
// ------------------------------------------------------------------


using System;

namespace ROMVault2.SupportedFiles.Zip.ZLib
{
     public class DeflateStream : System.IO.Stream
    {
        internal ZlibBaseStream _baseStream;
        internal System.IO.Stream _innerStream;
        bool _disposed;

        public DeflateStream(System.IO.Stream stream, CompressionMode mode)
            : this(stream, mode, CompressionLevel.Default, false)
        {
        }

        public DeflateStream(System.IO.Stream stream, CompressionMode mode, CompressionLevel level)
            : this(stream, mode, level, false)
        {
        }

        public DeflateStream(System.IO.Stream stream, CompressionMode mode, bool leaveOpen)
            : this(stream, mode, CompressionLevel.Default, leaveOpen)
        {
        }

        public DeflateStream(System.IO.Stream stream, CompressionMode mode, CompressionLevel level, bool leaveOpen)
        {
            _innerStream = stream;
            _baseStream = new ZlibBaseStream(stream, mode, level, ZlibStreamFlavor.DEFLATE, leaveOpen);
        }

        #region Zlib properties

        /// <summary>
        /// This property sets the flush behavior on the stream.
        /// </summary>
        /// <remarks> See the ZLIB documentation for the meaning of the flush behavior.
        /// </remarks>
        virtual public FlushType FlushMode
        {
            get { return (this._baseStream._flushMode); }
            set
            {
                if (_disposed) throw new ObjectDisposedException("DeflateStream");
                this._baseStream._flushMode = value;
            }
        }

        /// <summary>
        ///   The size of the working buffer for the compression codec.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   The working buffer is used for all stream operations.  The default size is
        ///   1024 bytes.  The minimum size is 128 bytes. You may get better performance
        ///   with a larger buffer.  Then again, you might not.  You would have to test
        ///   it.
        /// </para>
        ///
        /// <para>
        ///   Set this before the first call to <c>Read()</c> or <c>Write()</c> on the
        ///   stream. If you try to set it afterwards, it will throw.
        /// </para>
        /// </remarks>
        public int BufferSize
        {
            get
            {
                return this._baseStream._bufferSize;
            }
            set
            {
                if (_disposed) throw new ObjectDisposedException("DeflateStream");
                if (this._baseStream._workingBuffer != null)
                    throw new ZlibException("The working buffer is already set.");
                if (value < ZlibConstants.WorkingBufferSizeMin)
                    throw new ZlibException(String.Format("Don't be silly. {0} bytes?? Use a bigger buffer, at least {1}.", value, ZlibConstants.WorkingBufferSizeMin));
                this._baseStream._bufferSize = value;
            }
        }

        /// <summary>
        ///   The ZLIB strategy to be used during compression.
        /// </summary>
        ///
        /// <remarks>
        ///   By tweaking this parameter, you may be able to optimize the compression for
        ///   data with particular characteristics.
        /// </remarks>
        public CompressionStrategy Strategy
        {
            get
            {
                return this._baseStream.Strategy;
            }
            set
            {
            if (_disposed) throw new ObjectDisposedException("DeflateStream");
                this._baseStream.Strategy = value;
            }
        }

        /// <summary> Returns the total number of bytes input so far.</summary>
        virtual public long TotalIn
        {
            get
            {
                return this._baseStream._z.TotalBytesIn;
            }
        }

        /// <summary> Returns the total number of bytes output so far.</summary>
        virtual public long TotalOut
        {
            get
            {
                return this._baseStream._z.TotalBytesOut;
            }
        }

        #endregion

        #region System.IO.Stream methods
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!_disposed)
                {
                    if (disposing && (this._baseStream != null))
                        this._baseStream.Close();
                    _disposed = true;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }



        public override bool CanRead
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException("DeflateStream");
                return _baseStream._stream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get { return false; }
        }


        public override bool CanWrite
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException("DeflateStream");
                return _baseStream._stream.CanWrite;
            }
        }

        /// <summary>
        /// Flush the stream.
        /// </summary>
        public override void Flush()
        {
            if (_disposed) throw new ObjectDisposedException("DeflateStream");
            _baseStream.Flush();
        }

        /// <summary>
        /// Reading this property always throws a <see cref="NotImplementedException"/>.
        /// </summary>
        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// The position of the stream pointer.
        /// </summary>
        ///
        /// <remarks>
        ///   Setting this property always throws a <see
        ///   cref="NotImplementedException"/>. Reading will return the total bytes
        ///   written out, if used in writing, or the total bytes read in, if used in
        ///   reading.  The count may refer to compressed bytes or uncompressed bytes,
        ///   depending on how you've used the stream.
        /// </remarks>
        public override long Position
        {
            get
            {
                if (this._baseStream._streamMode == ZlibBaseStream.StreamMode.Writer)
                    return this._baseStream._z.TotalBytesOut;
                if (this._baseStream._streamMode == ZlibBaseStream.StreamMode.Reader)
                    return this._baseStream._z.TotalBytesIn;
                return 0;
            }
            set { throw new NotImplementedException(); }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_disposed) throw new ObjectDisposedException("DeflateStream");
            return _baseStream.Read(buffer, offset, count);
        }


        /// <summary>
        /// Calling this method always throws a <see cref="NotImplementedException"/>.
        /// </summary>
        /// <param name="offset">this is irrelevant, since it will always throw!</param>
        /// <param name="origin">this is irrelevant, since it will always throw!</param>
        /// <returns>irrelevant!</returns>
        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calling this method always throws a <see cref="NotImplementedException"/>.
        /// </summary>
        /// <param name="value">this is irrelevant, since it will always throw!</param>
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///   Write data to the stream.
        /// </summary>
        /// <remarks>
        ///
        /// <para>
        ///   If you wish to use the <c>DeflateStream</c> to compress data while
        ///   writing, you can create a <c>DeflateStream</c> with
        ///   <c>CompressionMode.Compress</c>, and a writable output stream.  Then call
        ///   <c>Write()</c> on that <c>DeflateStream</c>, providing uncompressed data
        ///   as input.  The data sent to the output stream will be the compressed form
        ///   of the data written.  If you wish to use the <c>DeflateStream</c> to
        ///   decompress data while writing, you can create a <c>DeflateStream</c> with
        ///   <c>CompressionMode.Decompress</c>, and a writable output stream.  Then
        ///   call <c>Write()</c> on that stream, providing previously compressed
        ///   data. The data sent to the output stream will be the decompressed form of
        ///   the data written.
        /// </para>
        ///
        /// <para>
        ///   A <c>DeflateStream</c> can be used for <c>Read()</c> or <c>Write()</c>,
        ///   but not both.
        /// </para>
        ///
        /// </remarks>
        ///
        /// <param name="buffer">The buffer holding data to write to the stream.</param>
        /// <param name="offset">the offset within that data array to find the first byte to write.</param>
        /// <param name="count">the number of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_disposed) throw new ObjectDisposedException("DeflateStream");
            _baseStream.Write(buffer, offset, count);
        }
        #endregion

        public static byte[] CompressString(String s)
        {
            using (var ms = new System.IO.MemoryStream())
            {
                System.IO.Stream compressor =
                    new DeflateStream(ms, CompressionMode.Compress, CompressionLevel.BestCompression);
                ZlibBaseStream.CompressString(s, compressor);
                return ms.ToArray();
            }
        }


        /// <summary>
        ///   Compress a byte array into a new byte array using DEFLATE.
        /// </summary>
        ///
        /// <remarks>
        ///   Uncompress it with <see cref="DeflateStream.UncompressBuffer(byte[])"/>.
        /// </remarks>
        ///
        /// <seealso cref="DeflateStream.CompressString(string)">DeflateStream.CompressString(string)</seealso>
        /// <seealso cref="DeflateStream.UncompressBuffer(byte[])">DeflateStream.UncompressBuffer(byte[])</seealso>
        /// <seealso cref="GZipStream.CompressBuffer(byte[])">GZipStream.CompressBuffer(byte[])</seealso>
        /// <seealso cref="ZlibStream.CompressBuffer(byte[])">ZlibStream.CompressBuffer(byte[])</seealso>
        ///
        /// <param name="b">
        ///   A buffer to compress.
        /// </param>
        ///
        /// <returns>The data in compressed form</returns>
        public static byte[] CompressBuffer(byte[] b)
        {
            using (var ms = new System.IO.MemoryStream())
            {
                System.IO.Stream compressor =
                    new DeflateStream( ms, CompressionMode.Compress, CompressionLevel.BestCompression );

                ZlibBaseStream.CompressBuffer(b, compressor);
                return ms.ToArray();
            }
        }


        /// <summary>
        ///   Uncompress a DEFLATE'd byte array into a single string.
        /// </summary>
        ///
        /// <seealso cref="DeflateStream.CompressString(String)">DeflateStream.CompressString(String)</seealso>
        /// <seealso cref="DeflateStream.UncompressBuffer(byte[])">DeflateStream.UncompressBuffer(byte[])</seealso>
        /// <seealso cref="GZipStream.UncompressString(byte[])">GZipStream.UncompressString(byte[])</seealso>
        /// <seealso cref="ZlibStream.UncompressString(byte[])">ZlibStream.UncompressString(byte[])</seealso>
        ///
        /// <param name="compressed">
        ///   A buffer containing DEFLATE-compressed data.
        /// </param>
        ///
        /// <returns>The uncompressed string</returns>
        public static String UncompressString(byte[] compressed)
        {
            using (var input = new System.IO.MemoryStream(compressed))
            {
                System.IO.Stream decompressor =
                    new DeflateStream(input, CompressionMode.Decompress);

                return ZlibBaseStream.UncompressString(compressed, decompressor);
            }
        }


        /// <summary>
        ///   Uncompress a DEFLATE'd byte array into a byte array.
        /// </summary>
        ///
        /// <seealso cref="DeflateStream.CompressBuffer(byte[])">DeflateStream.CompressBuffer(byte[])</seealso>
        /// <seealso cref="DeflateStream.UncompressString(byte[])">DeflateStream.UncompressString(byte[])</seealso>
        /// <seealso cref="GZipStream.UncompressBuffer(byte[])">GZipStream.UncompressBuffer(byte[])</seealso>
        /// <seealso cref="ZlibStream.UncompressBuffer(byte[])">ZlibStream.UncompressBuffer(byte[])</seealso>
        ///
        /// <param name="compressed">
        ///   A buffer containing data that has been compressed with DEFLATE.
        /// </param>
        ///
        /// <returns>The data in uncompressed form</returns>
        public static byte[] UncompressBuffer(byte[] compressed)
        {
            using (var input = new System.IO.MemoryStream(compressed))
            {
                System.IO.Stream decompressor =
                    new DeflateStream( input, CompressionMode.Decompress );

                return ZlibBaseStream.UncompressBuffer(compressed, decompressor);
            }
        }

    }

}


/******************************************************
 *     ROMVault2 is written by Gordon J.              *
 *     Contact gordon@romvault.com                    *
 *     Copyright 2014                                 *
 ******************************************************/

using System.IO;
using System.Security.Cryptography;
using ROMVault2.SupportedFiles.Zip.ZLib;
using System.Threading;
using System.Windows.Forms;

namespace ROMVault2.SupportedFiles.Files
{
    public static class UnCompFiles
    {
        private const int Buffersize = 4096*256*6;
        static byte[] Buffer;
        static byte[] Buffer2;

        static UnCompFiles()
        {
            Buffer = new byte[Buffersize];
            Buffer2 = new byte[Buffersize];

        }

        public static int CheckSumRead(string filename, bool testDeep, out byte[] crc, out byte[] bMD5, out byte[] bSHA1)
        {
            bMD5 = null;
            bSHA1 = null;
            crc = null;

            Stream ds=null;
            CRC32Hash crc32 = new CRC32Hash();

            MD5 md5 = null;
            if (testDeep) md5 = MD5.Create();
            SHA1 sha1 = null;
            if (testDeep) sha1 = SHA1.Create();

            try
            {
                int errorCode = IO.FileStream.OpenFileRead(filename, out ds);
                if (errorCode != 0)
                    return errorCode;

                long sizetogo = ds.Length;

                int sizenow = sizetogo > Buffersize ? Buffersize : (int)sizetogo;
                
                ds.Read(Buffer, 0, sizenow);

                Thread t2= null, t3 = null;
                
                while (sizetogo > 0)
                {

                    Thread t1 = new Thread(() => { crc32.TransformBlock(Buffer, 0, sizenow, null, 0); });
                    t1.Start();
                    if (testDeep)
                    {
                        t2 = new Thread(() => { md5.TransformBlock(Buffer, 0, sizenow, null, 0); });
                        t3 = new Thread(() => { sha1.TransformBlock(Buffer, 0, sizenow, null, 0); });
                        t2.Start();
                        t3.Start();
                    }

                    Thread t4 = new Thread(() =>
                    {
                        sizetogo -= sizenow;
                        sizenow = sizetogo > Buffersize ? Buffersize : (int)sizetogo;
                        ds.Read(Buffer2, 0, sizenow);
                    });

                    t4.Start();

                    if (testDeep)
                    {
                        t2.Join();
                        t3.Join();
                    }
                    t4.Join();
                    t1.Join();
                    byte[] tmpbuffer = Buffer2;
                    Buffer2 = Buffer;
                    Buffer = tmpbuffer;
                }

                crc32.TransformFinalBlock(Buffer, 0, 0);
                if (testDeep) md5.TransformFinalBlock(Buffer, 0, 0);
                if (testDeep) sha1.TransformFinalBlock(Buffer, 0, 0);

                ds.Close();
            }
            catch
            {
                if (ds != null)
                    ds.Close();
                
                return 0x17;
            }


            crc = crc32.Hash;
            if (testDeep) bMD5 = md5.Hash;
            if (testDeep) bSHA1 = sha1.Hash;

            return 0;
        }



    }
}

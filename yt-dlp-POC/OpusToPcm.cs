﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Concentus.Structs;
using NEbml.Core;

namespace yt_dlp_POC
{
    public class OpusToPcm
    {
        //1f43b675
        static readonly byte[] TRACKS = { 0x16, 0x54, 0xAE, 0x6B };

        static readonly byte[] TRACKENTRY = { 0xAE };

        static readonly byte[] CODECID = { 0x86 }; // A_OPUS String
        static readonly byte[] AUDIO = { 0xE1 };

        static readonly byte[] SAMPLING = { 0xB5 }; //float
        static readonly byte[] CHANNELS = { 0x9F }; //uint


        static readonly byte[] CLUSTER = { 0x1f, 0x43, 0xb6, 0x75 };
        static readonly byte[] TIMECODE = { 0xE7 }; //binary
        static readonly byte[] SIMPLEBLOCK = { 0xA3 }; //binary
        


        public static List<OpusPacket> GetPackets(YtStream songStream)
        {
            MemoryStream auxStream = new MemoryStream(songStream.GetBuffer());
            List<OpusPacket> opusContent = new List<OpusPacket>();
            //espero a que descargue algo y creo un nuevo memorystream con el mismo buffer para resetear la posicion
            WaitForDownloadedBytes(songStream, 1024 / 2);
            //posicion absoluta del cluster
            long posCluster = 0;

            while (posCluster != -1)
            {
                posCluster = FindPosition(auxStream, CLUSTER,true);

                if (posCluster != -1)
                {
                    EbmlReader ebmlReader = new EbmlReader(auxStream);
                    //ebmlReader.EnterContainer();
                    long clusterSize = EnterCluster(ebmlReader, posCluster);

                    long nextClusterPos = clusterSize + posCluster;

                    WaitForDownloadedBytes(songStream, posCluster + clusterSize);

                    //position relative to the cluster position
                    long posBlock = 0;
                    long startPos = FindPosition(auxStream, SIMPLEBLOCK);
                    OpusDecoder opusDecoder = new OpusDecoder(48000, 2);
                    bool isError = false;
                    while (posBlock >= 0 && !isError && auxStream.Position < nextClusterPos)
                    {
                        if (posBlock != 1)
                        {
                            isError = false;
                            try
                            {
                                ebmlReader.ReadAt(posBlock);
                            }
                            catch (Exception ex) { ebmlReader.LeaveContainer(); isError = true; }

                            if (!isError)
                            {
                                OpusPacket opusPacket = new OpusPacket(GetBuffer(ebmlReader, auxStream));
                                opusContent.Add(opusPacket);
                                
                            }
                            //relativo al clusterPos
                            posBlock = FindPosition(auxStream, SIMPLEBLOCK, false) - startPos;

                        }

                    }
                    //seek to the element position
                    auxStream.Seek(ebmlReader.ElementPosition, SeekOrigin.Begin);
                    ebmlReader.LeaveContainer();
                }

            }
            
            return opusContent;
        }

        public static byte[] GetPcm(List<OpusPacket> opusPackets)
        {
            OpusDecoder opusDecoder = new OpusDecoder(48000,2);
            List<byte> pcm = new List<byte>();
            foreach(OpusPacket opusPacket in opusPackets)
            {
                try
                {
                    short[] pcmBuffer = new short[opusPacket.ChannelCount * opusPacket.Frames * opusPacket.FrameSize];
                    int decodedSamples = opusDecoder.Decode(opusPacket.OpusBuffer, 0, opusPacket.OpusBuffer.Length, pcmBuffer, 0, opusPacket.FrameSize);
                    byte[] pcmBufferInBytes = new byte[pcmBuffer.Length * 2];
                    Buffer.BlockCopy(pcmBuffer, 0, pcmBufferInBytes, 0, pcmBufferInBytes.Length);
                    pcm.AddRange(pcmBufferInBytes);
                }

                catch (Exception ex) { }
            }
            return pcm.ToArray();
        }

        private static byte[] GetBuffer(EbmlReader ebmlReader,Stream auxStream)
        {
            byte[] opusBuffer = new byte[ebmlReader.ElementSize - 4];
            auxStream.Seek(auxStream.Position + 4, SeekOrigin.Begin);
            ebmlReader.ReadBinary(opusBuffer, 0, opusBuffer.Length);
            return opusBuffer;
        }

        private static void WaitForDownloadedBytes(YtStream songStream,long bytes)
        {
            while (songStream.DownloadedBytes < bytes) { }

        }

        private static long EnterCluster(EbmlReader ebmlReader,long posCluster)
        {
            ebmlReader.ReadAt(posCluster);
            long clusterSize = ebmlReader.ElementSize;
            ebmlReader.EnterContainer();
            return clusterSize;

            //waits to download the first cluster
        }

        private static long FindPosition(Stream stream, byte[] byteSequence, bool reset = false)
        {
            long result = -1;
            if (byteSequence.Length > stream.Length)
                return result;

            byte[] buffer = new byte[byteSequence.Length];

            int i;
            bool isFound = false;
            while ((i = stream.Read(buffer, 0, byteSequence.Length)) == byteSequence.Length && !isFound)
            {
                if (byteSequence.SequenceEqual(buffer))
                {
                    result = stream.Position - byteSequence.Length;
                    isFound = true;
                }
                else
                    stream.Position -= byteSequence.Length - PadLeftSequence(buffer, byteSequence);
            }

            if (reset && result != -1)
                stream.Seek(0, SeekOrigin.Begin);
            else if (result != -1)
                stream.Seek(result, SeekOrigin.Begin);


            return result;
        }

        private static int PadLeftSequence(byte[] bytes, byte[] seqBytes)
        {
            int i = 1;
            while (i < bytes.Length)
            {
                int n = bytes.Length - i;
                byte[] aux1 = new byte[n];
                byte[] aux2 = new byte[n];
                Array.Copy(bytes, i, aux1, 0, n);
                Array.Copy(seqBytes, aux2, n);
                if (aux1.SequenceEqual(aux2))
                    return i;
                i++;
            }
            return i;
        }


    }

}

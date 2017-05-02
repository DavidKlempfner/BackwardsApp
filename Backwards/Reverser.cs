/*

http://www.topherlee.com/software/pcm-tut-wavformat.html

http://blogs.msdn.com/b/dawate/archive/2009/06/23/intro-to-audio-programming-part-2-demystifying-the-wav-format.aspx

http://www-mmsp.ece.mcgill.ca/Documents/AudioFormats/WAVE/WAVE.html

Positions Sample Value Description

0 - 3 "RIFF" Marks the file as a riff file. Characters are each 1 byte long.

4 - 7 File size (integer) Size of the overall file - 8 bytes, in bytes (32-bit integer). Typically, you'd fill this in after creation.

8 -11 "WAVE" File Type Header. For our purposes, it always equals "WAVE".

12-15 "fmt " Format chunk marker. Includes trailing null

16-19 16 Length of format data as listed above

20-21 1 Type of format (1 is PCM) - 2 byte integer

22-23 2 Number of Channels - 2 byte integer

24-27 44100 Sample Rate - 32 byte integer. Common values are 44100 (CD), 48000 (DAT). Sample Rate = Number of Samples per second, or Hertz.

28-31 176400 (Sample Rate * BitsPerSample * Channels) / 8.

32-33 4 (BitsPerSample * Channels) / 8.1 - 8 bit mono2 - 8 bit stereo/16 bit mono4 - 16 bit stereo

34-35 16 Bits per sample

36-39 "data" "data" chunk header. Marks the beginning of the data section.

40-43 File size (data) Size of the data section.

Sample values are given above for a 16-bit stereo source.

*

The above is correct until "data" (36 to 39).

36 to 39 is actually "LIST".

"data" starts at 70.

Not sure what is between index 40 and index 69? There are 70 Bytes unaccounted for.

*

*/

using System;
using System.IO;

namespace BackwardsApp.Backwards
{
    internal static class Reverser
    {
        public static void Start()
        {            
            string forwardsWavFilePath = @"C:\Users\david.klempfner\Documents\Sound recordings\RecordingWav.wav";
            byte[] forwardsWavFileStreamByteArray = PopulateForwardsWavFileByteArray(forwardsWavFilePath);            

            byte[] forwardsArrayWithOnlyHeaders = CreateForwardsArrayWithOnlyHeaders(forwardsWavFileStreamByteArray, Constants.StartIndexOfAudioDataChunk);
            byte[] forwardsArrayWithOnlyAudioData = CreateForwardsArrayWithOnlyAudioData(forwardsWavFileStreamByteArray, Constants.StartIndexOfAudioDataChunk);

            int bytesPerSample = MetadataGatherer.GetBitsPerSample(forwardsWavFileStreamByteArray) / Constants.BitsPerByte;
            byte[] reversedArrayWithOnlyAudioData = ReverseTheForwardsArrayWithOnlyAudioData(bytesPerSample, forwardsArrayWithOnlyAudioData);
            byte[] reversedWavFileStreamByteArray = CombineArrays(forwardsArrayWithOnlyHeaders, reversedArrayWithOnlyAudioData);

            string reversedWavFilePath = @"C:\Users\david.klempfner\Documents\Sound recordings\reversedFile.wav";

            WriteReversedWavFileByteArrayToFile(reversedWavFileStreamByteArray, reversedWavFilePath);
        }

        private static void WriteReversedWavFileByteArrayToFile(byte[] reversedWavFileStreamByteArray, string reversedWavFilePath)
        {
            using (FileStream reversedFileStream = new FileStream(reversedWavFilePath, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                reversedFileStream.Write(reversedWavFileStreamByteArray, 0, reversedWavFileStreamByteArray.Length);
            }
        }

        private static byte[] CombineArrays(byte[] forwardsArrayWithOnlyHeaders, byte[] reversedArrayWithOnlyAudioData)
        {
            byte[] reversedWavFileStreamByteArray = new byte[forwardsArrayWithOnlyHeaders.Length + reversedArrayWithOnlyAudioData.Length];
            Array.Copy(forwardsArrayWithOnlyHeaders, reversedWavFileStreamByteArray, forwardsArrayWithOnlyHeaders.Length);
            Array.Copy(reversedArrayWithOnlyAudioData, 0, reversedWavFileStreamByteArray, forwardsArrayWithOnlyHeaders.Length, reversedArrayWithOnlyAudioData.Length);
            return reversedWavFileStreamByteArray;
        }

        private static byte[] ReverseTheForwardsArrayWithOnlyAudioData(int bytesPerSample, byte[] forwardsArrayWithOnlyAudioData)
        {
            int length = forwardsArrayWithOnlyAudioData.Length;
            byte[] reversedArrayWithOnlyAudioData = new byte[length];
            int sampleIdentifier = 0;
            for (int i = 0; i < length; i++)
            {
                if (i != 0 && i % bytesPerSample == 0)
                {
                    sampleIdentifier += 2 * bytesPerSample;
                }
                int index = length - bytesPerSample - sampleIdentifier + i;
                reversedArrayWithOnlyAudioData[i] = forwardsArrayWithOnlyAudioData[index];
            }
            return reversedArrayWithOnlyAudioData;
        }

        private static byte[] CreateForwardsArrayWithOnlyAudioData(byte[] forwardsWavFileStreamByteArray, int startIndexOfDataChunk)
        {
            byte[] forwardsArrayWithOnlyAudioData = new byte[forwardsWavFileStreamByteArray.Length - startIndexOfDataChunk];
            Array.Copy(forwardsWavFileStreamByteArray, startIndexOfDataChunk, forwardsArrayWithOnlyAudioData, 0, forwardsWavFileStreamByteArray.Length - startIndexOfDataChunk);
            return forwardsArrayWithOnlyAudioData;
        }

        private static byte[] CreateForwardsArrayWithOnlyHeaders(byte[] forwardsWavFileStreamByteArray, int startIndexOfDataChunk)
        {
            byte[] forwardsArrayWithOnlyHeaders = new byte[startIndexOfDataChunk];
            Array.Copy(forwardsWavFileStreamByteArray, 0, forwardsArrayWithOnlyHeaders, 0, startIndexOfDataChunk);
            return forwardsArrayWithOnlyHeaders;
        }

        private static byte[] PopulateForwardsWavFileByteArray(string forwardsWavFilePath)
        {
            byte[] forwardsWavFileStreamByteArray;
            using (FileStream forwardsWavFileStream = new FileStream(forwardsWavFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                forwardsWavFileStreamByteArray = new byte[forwardsWavFileStream.Length];
                forwardsWavFileStream.Read(forwardsWavFileStreamByteArray, 0, (int)forwardsWavFileStream.Length);
            }
            
            return forwardsWavFileStreamByteArray;
        }
    }
}
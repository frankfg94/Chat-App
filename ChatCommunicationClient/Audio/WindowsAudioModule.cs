using ChatCommunicationClient.Audio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ChatCommunicationClient
{
    public class WindowsAudioModule : IAudioModule
    {
        // We use winmm to avoid importing external libraries
        [DllImport("winmm.dll", EntryPoint = "mciSendStringA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern int mciSendString(string lpstrCommand, string lpstrReturnString, int uReturnLength, int hwndCallback);

        public bool IsCompatible()
        {
            throw new NotImplementedException();
        }

        public void Play(byte[] audioData)
        {
            if(audioData == null)
            {
                Console.WriteLine("Error, no audio data has been found in the message");
            }
            else
            {
                var tempPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\tempAudio.wav"; 
                File.WriteAllBytes(tempPath, audioData);
                Console.WriteLine("Playing audio at "  + tempPath);
                Process.Start(@"powershell", $@"-c (New-Object Media.SoundPlayer '{tempPath}').PlaySync();");
            }
        }

        public void Record()
        {
            Console.WriteLine("Started recording audio on your computer");
            mciSendString("open new Type waveaudio Alias recsound", "", 0, 0);
            mciSendString("record recsound", "", 0, 0);
           // Console.WriteLine("Recording audio, press Enter to stop and send ...");
        }

        public void StopRecording()
        {
            mciSendString("save recsound  c:\\temp\\toSend.wav", "", 0, 0);
            mciSendString("close recsound ", "", 0, 0);
        }
    }
}

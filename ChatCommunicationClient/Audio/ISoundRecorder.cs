using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ChatCommunicationClient.Audio
{
    /// <summary>
    /// An interface can be useful to provide different implementations, for example an implentation of the audio for linux
    /// one for MacOs and one for Windows
    /// </summary>
    interface ISoundRecorder
    {
        void Record();

        void Play(byte[] audioData);

        /// <summary>
        /// Check if the system is compatible with the sound module
        /// </summary>
        /// <returns></returns>
        bool IsCompatible();

    }
}

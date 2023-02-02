using System;
using vp.util;

namespace vp.orchestrations.processaudio
{
    public class ProcessAudioTransaction
    {
        public string transactionId { get; set; } = $"{Guid.NewGuid()}";
        public string incomingFileName { get; set; } = "";
        public string tempFolderPath { get; set; } = "";

        public string getPreviewFilename()
        {
            return $"{transactionId}{Config.SamplePreviewFileFormat}";
        }
        public string getOutgoingFileName()
        {
            return $"{transactionId}{Utils.GetFileExtension(incomingFileName)}";
        }

        public string getTempFilePath()
        {
            return $"{tempFolderPath}\\{getOutgoingFileName()}";
        }

        public string getPreviewFilePath()
        {
            return $"{tempFolderPath}\\{getPreviewFilename()}";
        }
    }
}

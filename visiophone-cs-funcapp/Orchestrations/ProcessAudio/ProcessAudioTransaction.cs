using System;
using System.Collections.Generic;
using vp.util;

namespace vp.orchestrations.processaudio
{
    public class ProcessAudioTransaction
    {
        public string incomingFileId { get; set; } = $"{Guid.NewGuid()}";
        public string outgoingFileId { get; set; } = $"{Guid.NewGuid()}";
        public string incomingFileName { get; set; } = "";
        public string tempFolderPath { get; set; } = "";
        public string tempFilePath { get; set; } = "";
        public string sampleId { get; set; } = "";
        public string fileExtension { get; set; } = "";

        public List<TranscodeParams> transcodeProfiles { get; set; } = new List<TranscodeParams>();
        public List<string> transcodePaths { get; set; } = new List<string>();

        public List<string> errors { get; set; } = new List<string>();

        public string getPreviewFilename()
        {
            return $"{incomingFileId}{Config.SamplePreviewFileFormat}";
        }
        public string getOutgoingFileName()
        {
            return $"{outgoingFileId}{Utils.GetFileExtension(incomingFileName)}";
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

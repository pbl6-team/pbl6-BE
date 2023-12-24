namespace PBL6.Application.Contract.ExternalServices.Meetings.Dtos
{
    public class Session
    {
        public string mediaMode { get; set; }
        public string recordingMode { get; set; }
        public string customSessionId { get; set; }
        public string forcedVideoCodec { get; set; }
        public bool allowTranscoding { get; set; }
        public DefaultRecordingProperties defaultRecordingProperties { get; set; }
        public MediaNode mediaNode { get; set; }

        public Session()
        {
            this.mediaMode = "ROUTED";
            this.recordingMode = "MANUAL";
            this.customSessionId = "CUSTOM_SESSION_ID";
            this.forcedVideoCodec = "VP8";
            this.allowTranscoding = false;
            this.defaultRecordingProperties = new DefaultRecordingProperties();
            this.defaultRecordingProperties.name = "MyRecording";
            this.defaultRecordingProperties.hasAudio = true;
            this.defaultRecordingProperties.hasVideo = true;
            this.defaultRecordingProperties.outputMode = "COMPOSED";
            this.defaultRecordingProperties.recordingLayout = "BEST_FIT";
            this.defaultRecordingProperties.resolution = "1280x720";
            this.defaultRecordingProperties.frameRate = 25;
            this.defaultRecordingProperties.shmSize = 536870912;
            this.defaultRecordingProperties.mediaNode = new MediaNode();
            this.defaultRecordingProperties.mediaNode.id = "media_i-0c58bcdd26l11d0sd";
            this.mediaNode = new MediaNode();
            this.mediaNode.id = "media_i-0c58bcdd26l11d0sd";
        }
    }

    public class DefaultRecordingProperties
    {
        public string name { get; set; }
        public bool hasAudio { get; set; }
        public bool hasVideo { get; set; }
        public string outputMode { get; set; }
        public string recordingLayout { get; set; }
        public string resolution { get; set; }
        public int frameRate { get; set; }
        public int shmSize { get; set; }
        public MediaNode mediaNode { get; set; }
    }

    public class MediaNode
    {
        public string id { get; set; }
    }
}

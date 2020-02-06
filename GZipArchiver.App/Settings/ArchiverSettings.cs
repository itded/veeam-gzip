namespace GZipArchiver.App.Settings
{
    public class ArchiverSettings
    {
        [Setting(Key= "WorkThreadCount", DefaultValue = 0, Type="System.Int32")]
        public int WorkThreadCount { get; set; }

        [Setting(Key = "BlockSize", DefaultValue = 1024*1024, Type = "System.Int32")]
        public int BlockSize { get; set; }

        [Setting(Key = "QueueInSize", DefaultValue = 100, Type = "System.Int32")]
        public int QueueInSize { get; set; }

        [Setting(Key = "QueueOutSize", DefaultValue = 100, Type = "System.Int32")]
        public int QueueOutSize { get; set; }
    }
}

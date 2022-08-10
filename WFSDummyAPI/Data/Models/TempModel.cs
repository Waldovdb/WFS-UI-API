namespace WFS_UI_API.Data.Models
{
    public class TempModel
    {
        public int? SourceID { get; set; }
        public int? ServiceID { get; set; }
        public int? LoadID { get; set; }
        public int? Priority { get; set; }
        public string IntroType { get; set; }
        public string Bucket { get; set; }
        public string SegmentName { get; set; }
        public string StrategySegmentID { get; set; }
    }
}

namespace Press.Core.Publications
{
    public class Publication
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public DateTime Date { get; set; }
        public string Contents { get; set; }
        public PublicationSource Source { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
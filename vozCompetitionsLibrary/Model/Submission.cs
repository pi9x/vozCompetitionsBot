namespace vozCompetitionsLibrary.Model
{
    public class Submission
    {
        public string CompetitionHashtag { get; set; }
        public int UserId { get; set; }
        public string UserInfo { get; set; }
        public long ChatId { get; set; }
        public int MessageId { get; set; }
        public int Point { get; set; }
    }
}

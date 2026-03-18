namespace backend.Models
{
    public class TrainingPlan
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int MemberId { get; set; }
        public Member? Member { get; set; }
        public int TrainingId { get; set; }
        public Training? Training { get; set; }
    }
}

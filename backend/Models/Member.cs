namespace backend.Models
{
    public class Member
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime JoinDate { get; set; }
        public List<TrainingPlan> TrainingPlans { get; set; } = new List<TrainingPlan>();
    }
}

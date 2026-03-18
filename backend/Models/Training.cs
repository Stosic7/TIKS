namespace backend.Models
{
    public class Training
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DurationInMinutes { get; set; }
        public int TrainerId { get; set; }
        public Trainer? Trainer { get; set; }
        public List<TrainingPlan> TrainingPlans { get; set; } = new List<TrainingPlan>();
    }
}

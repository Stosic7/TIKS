namespace backend.Models
{
    public class Trainer
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public List<Training> Trainings { get; set; } = new List<Training>();
    }
}

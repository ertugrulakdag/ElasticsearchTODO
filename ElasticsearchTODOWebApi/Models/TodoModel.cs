namespace ElasticsearchTODOWebApi.Models
{
    public class TodoModel
    {
        public Guid? Id { get; set; }
        public required string Name { get; set; }
        public bool Done { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
    }
}

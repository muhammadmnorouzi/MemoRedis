namespace MemoRedis.API.Dtos
{
    public class CreateMemoryDto
    {
        public string Description { get; set; } = default!;
        public DateTimeOffset Date { get; set; }
    }
}
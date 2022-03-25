using System.ComponentModel.DataAnnotations;

namespace MemoRedis.API.Models
{
    public class Memory
    {
        private Memory()
        {

        }

        public Memory(string? id, string desctiption, DateTimeOffset date) : this()
        {
            Id = id ?? CreateId(Guid.NewGuid());
            Desctiption = desctiption;
            _date = date.UtcDateTime;
        }

        [Required]
        public string Id { get; }

        [Required]
        public string Desctiption { get; } = default!;

        [Required]
        public DateTimeOffset Date
        {
            get => _date;
        }

        private DateTimeOffset _date;
        private Guid _id_guid_part;


        public static string CreateId(Guid id)
        {
            return $"memory:{id.ToString()}";
        }
    }
}
using System.ComponentModel.DataAnnotations;

namespace MemoRedis.API.Models
{
    public class Memory
    {
        private Memory()
        {

        }

        public Memory(string desctiption, DateTimeOffset date)
        {
            Desctiption = desctiption;
            _date = date.UtcDateTime;
        }


        [Required]
        public string Id { get; } = $"memory:{Guid.NewGuid().ToString()}";

        [Required]
        public string Desctiption { get; } = default!;

        [Required]
        public DateTimeOffset Date
        {
            get => _date;
        }

        private DateTimeOffset _date;
    }
}

namespace ScraperService.Models
{
    public class Currency
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Currency(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}

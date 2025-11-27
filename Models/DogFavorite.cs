namespace ConsumoAPI2.Api.Models
{
    public class DogFavorite
    {
        public int Id { get; set; }
        public string DogId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string BreedGroup { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime AddedDate { get; set; } = DateTime.UtcNow;
    }
}

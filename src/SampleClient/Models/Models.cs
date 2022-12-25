
namespace SampleClient.Models
{
    public class Pet
    {
        public string Name { get; set; }
    }

    public static class Mock
    {
        public static IList<Pet> MakePets()
        {
            return new List<Pet>
            {
                new Pet { Name = "Rin Tin Tin" },
                new Pet { Name = "Mr. Bigglesworth" },
                new Pet { Name = "K-9" }
            };
        }
    }
}

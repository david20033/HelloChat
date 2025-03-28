namespace HelloChat.ViewModels
{
    public class InfoViewModel
    {
        public string? ProfileImagePath { get; set; }
        public string Name { get; set; }
        public List<string> ImagesUrls { get; set; } = [];
    }
}

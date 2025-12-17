namespace Mikhaylova_lr2.Models.Auth
{
    public class TokenResponse
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public string Role { get; set; }
        public string Username { get; set; }
        public int? StudentId { get; set; }
        public int? TeacherId { get; set; }
    }
}

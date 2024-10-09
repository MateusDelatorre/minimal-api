namespace MinimalApi.DTOs
{
    public class AdministratorDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public Profile Profile { get;set; } = default!;
    }
}
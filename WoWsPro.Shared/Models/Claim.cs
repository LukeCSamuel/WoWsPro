using System.Text.Json;

namespace WoWsPro.Shared.Models {
    public class Claim
    {
        public Claim () { }

        public long ClaimId { get; set; }
        public long AccountId { get; set; }
        public string Title { get; set; }
        public string Value { get; set; }

        public Account Account { get; set; }

        public T GetValue<T>()
        {
            return JsonSerializer.Deserialize<T>(Value);
        }

        public void SetValue<T>(T value)
        {
            Value = JsonSerializer.Serialize(value);
        }
    } 
}
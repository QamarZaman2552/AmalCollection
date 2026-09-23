using System.Text.Json;
using ShoppingApp.Models;

namespace ShoppingApp.Services
{
    public class GuestCartService
    {
        private const string Key = "GuestCart";

        public List<GuestCartItem> Get(ISession session)
        {
            var json = session.GetString(Key);
            if (string.IsNullOrEmpty(json)) return new List<GuestCartItem>();
            try
            {
                return JsonSerializer.Deserialize<List<GuestCartItem>>(json) ?? new List<GuestCartItem>();
            }
            catch
            {
                return new List<GuestCartItem>();
            }
        }

        public void Save(ISession session, List<GuestCartItem> items)
        {
            session.SetString(Key, JsonSerializer.Serialize(items));
        }

        public void Clear(ISession session) => session.Remove(Key);

        public int Count(ISession session) => Get(session).Sum(i => i.Quantity);
    }

    public class GuestCartItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
        public string Size { get; set; } = "";
        public string Color { get; set; } = "";
    }
}

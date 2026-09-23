using System.Text.Json;

namespace ShoppingApp.Services
{
    public class GuestWishlistService
    {
        private const string Key = "GuestWishlist";

        public List<int> Get(ISession session)
        {
            var json = session.GetString(Key);
            if (string.IsNullOrEmpty(json)) return new List<int>();
            try
            {
                return JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
            }
            catch
            {
                return new List<int>();
            }
        }

        public void Save(ISession session, List<int> productIds)
        {
            session.SetString(Key, JsonSerializer.Serialize(productIds.Distinct().ToList()));
        }

        public void Clear(ISession session) => session.Remove(Key);

        public int Count(ISession session) => Get(session).Count;
    }
}

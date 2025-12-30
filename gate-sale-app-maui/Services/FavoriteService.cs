using System.Collections.Generic;
using System.Linq;

namespace GateSale.Services
{
    public class FavoriteService
    {
        private List<FavoriteItem> _favorites = new();
        public event Action? OnFavoritesChanged;

        public List<FavoriteItem> GetFavorites()
        {
            return _favorites.ToList();
        }

        public bool IsFavorite(string itemId)
        {
            return _favorites.Any(f => f.Id == itemId);
        }

        public void AddToFavorites(FavoriteItem item)
        {
            if (!IsFavorite(item.Id))
            {
                _favorites.Add(item);
                OnFavoritesChanged?.Invoke();
            }
        }

        public void RemoveFromFavorites(string itemId)
        {
            var item = _favorites.FirstOrDefault(f => f.Id == itemId);
            if (item != null)
            {
                _favorites.Remove(item);
                OnFavoritesChanged?.Invoke();
            }
        }

        public void ToggleFavorite(FavoriteItem item)
        {
            if (IsFavorite(item.Id))
            {
                RemoveFromFavorites(item.Id);
            }
            else
            {
                AddToFavorites(item);
            }
        }
    }

    public class FavoriteItem
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Price { get; set; } = "";
        public string Details { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public bool IsVerified { get; set; } = false;
        public string Rating { get; set; } = "";
    }
}

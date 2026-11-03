namespace NotificationLib
{
    public static class NotificationProcessor
    {
        public static IEnumerable<Notification> FilterAndSort(
            IEnumerable<Notification> notifications,
            NotificationFilterOptions options)
        {
            ArgumentNullException.ThrowIfNull(notifications);
            ArgumentNullException.ThrowIfNull(options);

            return Sort(Filter(notifications, options), options);
        }

        private static IEnumerable<Notification> Filter(
            IEnumerable<Notification> notifications,
            NotificationFilterOptions options)
        {
            var result = notifications.Where(n => n != null);

            if (options.IsRead.HasValue)
                result = result.Where(n => n.IsRead == options.IsRead.Value);

            if (options.Types is { Length: > 0 })
                result = result.Where(n => options.Types.Contains(n.Type));

            if (options.MinPriority.HasValue)
                result = result.Where(n => n.Priority >= options.MinPriority.Value);

            if (!string.IsNullOrWhiteSpace(options.SearchText))
            {
                var text = options.SearchText.Trim();
                result = result.Where(n =>
                (n.Title?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (n.Content?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            return result;
        }

        private static IEnumerable<Notification> Sort(
            IEnumerable<Notification> items,
            NotificationFilterOptions options)
        {
            return options.SortBy switch
            {
                null => items,
                SortNotificationBy.Date => Order(items, n => n.CreatedAt, options.Descending),
                SortNotificationBy.Priority => Order(items, n => n.Priority, options.Descending),
                SortNotificationBy.Title => Order(items, n => n.Title, options.Descending,
                    StringComparer.OrdinalIgnoreCase),
                _ => throw new ArgumentOutOfRangeException(nameof(options), "Неизвестный тип сортировки")
            };
        }

        private static IEnumerable<Notification> Order<TKey>(
            IEnumerable<Notification> items,
            Func<Notification, TKey> keySelector,
            bool descending,
            IComparer<TKey>? comparer = null)
            => descending
                ? items.OrderByDescending(keySelector, comparer)
                : items.OrderBy(keySelector, comparer);
    }
}

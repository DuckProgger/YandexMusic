using System.Collections.ObjectModel;

namespace Yandex.Music.Core;

public static class ObservableCollectionExtensions
{
    public static void RemoveRange<TSource>(this ObservableCollection<TSource> collection, int index, int count) {
        while (count > 0) {
            collection.RemoveAt(index);
            --count;
        }
    }

    public static void RemoveRange<TSource>(this ObservableCollection<TSource> collection, IEnumerable<TSource> items) {
        foreach (TSource item in items.ToArray()) {
            collection.Remove(item);
        }
    }

    public static void AddRange<TSource>(this ObservableCollection<TSource> collection, IEnumerable<TSource> items) {
        foreach (TSource item in items) {
            collection.Add(item);
        }
    }

    public static int RemoveAll<TSource>(this ObservableCollection<TSource> collection, Predicate<TSource> match) {
        int count = collection.Count;
        int index = 0;
        while (index < collection.Count) {
            TSource item = collection[index];
            if (match(item)) {
                collection.RemoveAt(index);
            }
            else {
                ++index;
            }
        }
        return count - collection.Count;
    }

    public static TSource RefreshItem<TSource>(this ObservableCollection<TSource> collection, TSource item) {
        TSource tempItem = item;
        int index = collection.IndexOf(item);
        collection.RemoveAt(index);
        collection.Insert(index, tempItem);
        return tempItem;
    }
}

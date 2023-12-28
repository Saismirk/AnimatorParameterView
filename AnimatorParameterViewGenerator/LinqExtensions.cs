namespace AnimatorParameterViewGenerator;

public static class LinqExtensions {
    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable) {
        return new(enumerable);
    }  
    
    public static Dictionary<TKey, TValue> AddToDictionary<TKey, TValue>(this Dictionary<TKey, TValue> source, 
        Dictionary<TKey, TValue> target) {
        foreach (var kvp in source) {
            if (target.ContainsKey(kvp.Key)) continue;
            target.Add(kvp.Key, kvp.Value);
        }
        
        return target;
    }
}
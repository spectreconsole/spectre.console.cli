// #if NETSTANDARD2_0
// namespace Spectre.Console.Cli;
//
// internal static class DictionaryExtensions
// {
//     public static void TryAdd<TKey, TValue>(
//         this IDictionary<TKey, TValue> dictionary,
//         TKey key, TValue value)
//         where TKey : notnull
//     {
//         if (!dictionary.ContainsKey(key))
//         {
//             dictionary.Add(key, value);
//         }
//     }
// }
// #endif
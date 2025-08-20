using MySimpleDictionary;
using BlazorServer.Data;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System;

public class DictionaryService
{
    public MySimpleDictionary<string, string> Dict { get; } = new MySimpleDictionary<string, string>();

    public ObservableCollection<DictItem> Entries { get; } = new ObservableCollection<DictItem>();

    public void AddEntry(string key, string value)
    {
        Dict[key] = value;

        var existing = Entries.FirstOrDefault(e => e.Key == key);
        if (existing != null)
        {
            existing.Value = value;
            Console.WriteLine($"Updated: {existing.Key} = {existing.Value}");
        }
        else
        {
            Entries.Add(new DictItem { Key = key, Value = value });
            Console.WriteLine($"Added: {key} = {value}");
        }

        Console.WriteLine("Current Entries:");
        foreach (var e in Entries)
            Console.WriteLine($"- {e.Key} = {e.Value}");
    }

    public void RemoveEntry(string key)
    {
        Dict.Remove(key);
        var existing = Entries.FirstOrDefault(e => e.Key == key);
        if (existing != null)
            Entries.Remove(existing);
    }

    public void ClearAll()
    {
        Dict.Clear();
        Entries.Clear();
    }
}

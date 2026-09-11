namespace StockTrader.AI.Memory;

public sealed class ConversationMemory
{
    private readonly List<MemoryRecord> records = [];

    public IReadOnlyCollection<MemoryRecord> Records => records;

    public void Add(MemoryRecord record) => records.Add(record);

    public IReadOnlyCollection<MemoryRecord> Search(string symbol) => [.. records.Where(x => x.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase))];
}

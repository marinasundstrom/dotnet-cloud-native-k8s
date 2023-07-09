namespace WebApplication2.Models;

public sealed class Item
{
    private Item() { }

    public Item(string title)
    {
        Title = title;
    }

    public Guid Id { get; private set; }

    public string Title { get; set; }
}
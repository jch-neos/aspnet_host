using MediatR;
 using System.Windows.Input;

public class GetBookCommand : IRequest<Book>
{
    private int id;
    public int Id => id;

    public GetBookCommand(int id)
    {
        this.id = id;
    }
}

public class GetBookCommandHandler : IRequestHandler <GetBookCommand, Book>
{
    public Task<Book> Handle(GetBookCommand query, CancellationToken ct)
    {
        return Task.FromResult(new Book("H2G2","Douglas Adams", query.Id));
    }
}

public record class Book(string Title, string Author, int id);
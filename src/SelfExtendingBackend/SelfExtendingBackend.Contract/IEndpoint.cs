namespace SelfExtendingBackend.Contract;

public interface IEndpoint
{
    public HttpContent Request(string body);
    public string Url { get; }
}
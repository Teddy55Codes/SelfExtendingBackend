using Microsoft.Extensions.Primitives;

namespace SelfExtendingBackend.Backend.DynamicEndpoints;

public abstract class MutableEndpointDataSource : EndpointDataSource
{
    private readonly object _lock = new object();

    private IReadOnlyList<Endpoint> _endpoints;

    private CancellationTokenSource _cancellationTokenSource;

    private IChangeToken _changeToken;

    public MutableEndpointDataSource() : this(Array.Empty<Endpoint>()) { }

    public MutableEndpointDataSource(IReadOnlyList<Endpoint> endpoints)
    {
        SetEndpoints(endpoints);
    }

    public override IChangeToken GetChangeToken() => _changeToken;

    public override IReadOnlyList<Endpoint> Endpoints => _endpoints;

    public void SetEndpoints(IReadOnlyList<Endpoint> endpoints)
    {
        lock (_lock)
        {
            var oldCancellationTokenSource = _cancellationTokenSource;

            _endpoints = endpoints;

            _cancellationTokenSource = new CancellationTokenSource();
            _changeToken = new CancellationChangeToken(_cancellationTokenSource.Token);

            oldCancellationTokenSource?.Cancel();
        }
    }
}

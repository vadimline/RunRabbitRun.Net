using RunRabbitRun.Net.Attributes;

namespace RunRabbitRun.Net.Sample
{
    [Consumer]

    [Exchange(name: "hub", Type = "topic", Durable = true, AutoDelete = false)]
    [Exchange(name: "api", Type = "fanout", Durable = true, AutoDelete = false)]
    [Exchange(name: "users", Type = "topic", Durable = true, AutoDelete = false)]
    [Exchange(name: "orders", Type = "topic", Durable = true, AutoDelete = false)]

    [ExchangeToExchangeBinding(source: "hub", destination: "users", routingKey: "user.#")]
    [ExchangeToExchangeBinding(source: "hub", destination: "orders", routingKey: "order.#")]
    [ExchangeToExchangeBinding(source: "hub", destination: "api", routingKey: "api.#")]
    public abstract class Module
    {
        
    }
}
# Simple.Owin
### A toolkit/framework for OWIN component and application authors.

## Goals
Simple.Owin offers useful extension methods and provides a few typed constructs for OWIN server, middleware, application framework, and application authors.


## Typed Constructs
- **OwinRequest : IRequest**
    - An abstraction around `IDictionary<string, object> environment` to provide easy access to typed instances of values commonly used in request processing.
- **OwinResponse : IResponse**
    - An abstraction around `IDictionary<string, object> environment` to provide easy access to typed instances of values commonly used in response processing.
- **OwinContext : IContext**
    -  The root abstraction around `IDictionary<string, object> environment` that provides a handle to the Request and Response abstractions, as well as OWIN environment properties. This is *the preferred* abstraction for use in the OWIN pipeline. The instance is cached in the OWIN environment with the key "simple.Context", so it only gets created once per request. Use the static `OwinContext.Get(environment)` to acquire the context.
- **HTTP Headers**
    - Both request and response headers provide abstractions around the `IDictionary<string, string[]> headers` OWIN header specification, as well as properties for commonly used headers. They are available on `OwinRequest` and `OwinResponse`.
- **HTTP Cookies**
    - FUTURE - An abstraction around HTTP cookies.

## Hosting
A simple hosting environment that more closely follows the OWIN specification on start-up sequence. **Application component start-up is not currently defined well, and is left for each OWIN Host to implement.
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentMediator;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UnitTests.PingPong;
using Xunit;

namespace UnitTests
{
    public class MediatorTests
    {
        [Fact]
        public void PipelineBuilder()
        {
            var pingHandler = new Mock<IPingHandler>();

            var services = new ServiceCollection();
            services.AddScoped<IPingHandler>(provider => pingHandler.Object);
            services.AddTransient<GetService>(c => c.GetService);
            services.AddSingleton<IMediator, Mediator>();

            var provider = services.BuildServiceProvider();

            var mediator = provider.GetRequiredService<IMediator>();
            mediator.For<PingRequest>().Pipeline()
                .With<IPingHandler>((handler, req) => handler.MyMethod(req))
                .With<IPingHandler>((handler, req) => handler.MyLongMethod(req))
                .Build();

            var ping = new PingRequest("Ping");
            mediator.Publish(ping);

            pingHandler.Verify(e => e.MyMethod(ping), Times.Once);
            pingHandler.Verify(e => e.MyLongMethod(ping), Times.Once);
        }

        [Fact]
        public async Task BuildAsyncPipeline()
        {
            var pingHandler = new Mock<IPingHandler>();

            var services = new ServiceCollection();
            services.AddScoped<IPingHandler>(provider => pingHandler.Object);
            services.AddTransient<GetService>(c => c.GetService);
            services.AddSingleton<IMediator, Mediator>();

            var provider = services.BuildServiceProvider();

            var mediator = provider.GetRequiredService<IMediator>();
            mediator.For<PingRequest>().AsyncPipeline()
                .With<IPingHandler>(async(handler, req) => await handler.MyMethodAsync(req))
                .Build();

            var ping = new PingRequest("Async Ping");
            await mediator.PublishAsync(ping);

            pingHandler.Verify(e => e.MyMethodAsync(ping), Times.Once);
        }

        [Fact]
        public async Task BuildCancellableAsyncPipeline()
        {
            var pingHandler = new Mock<IPingHandler>();

            var services = new ServiceCollection();
            services.AddScoped<IPingHandler>(provider => pingHandler.Object);
            services.AddTransient<GetService>(c => c.GetService);
            services.AddSingleton<IMediator, Mediator>();

            var provider = services.BuildServiceProvider();

            var mediator = provider.GetRequiredService<IMediator>();
            mediator.For<PingRequest>().CancellablePipeline()
                .With<IPingHandler>(async(handler, req, ct) => await handler.MyMethodAsync(req, ct));

            var cts = new CancellationTokenSource();
            var ping = new PingRequest("Cancellable Async Ping");
            await mediator.PublishAsync(ping, cts.Token);

            pingHandler.Verify(e => e.MyMethodAsync(ping, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void BuildSendPipeline()
        {
            var services = new ServiceCollection();
            services.AddScoped<IPingHandler, PingHandler>();
            services.AddTransient<GetService>(c => c.GetService);
            services.AddSingleton<IMediator, Mediator>();

            var provider = services.BuildServiceProvider();

            var mediator = provider.GetRequiredService<IMediator>();
            mediator.Direct<PingRequest, PingResponse, IPingHandler>(
                (handler, req) => handler.MyMethod(req)
            );

            var ping = new PingRequest("Ping");
            var response = mediator.Send<PingResponse>(ping);

            Assert.NotNull(response);
        }

        [Fact]
        public async Task BuildSendAsyncPipeline()
        {
            var services = new ServiceCollection();
            services.AddScoped<IPingHandler, PingHandler>();
            services.AddTransient<GetService>(c => c.GetService);
            services.AddSingleton<IMediator, Mediator>();

            var provider = services.BuildServiceProvider();

            var mediator = provider.GetRequiredService<IMediator>();
            mediator.DirectAsync<PingRequest, PingResponse, IPingHandler>(
                (handler, req) => handler.MyMethodAsync(req)
            );

            var ping = new PingRequest("Ping");
            var response = await mediator.SendAsync<PingResponse>(ping);

            Assert.NotNull(response);
        }

        [Fact]
        public void BuildSendAsyncPipeline_ThrowsException()
        {
            var services = new ServiceCollection();
            services.AddScoped<IPingHandler, PingHandler>();
            services.AddTransient<GetService>(c => c.GetService);
            services.AddSingleton<IMediator, Mediator>();

            var provider = services.BuildServiceProvider();

            Exception ex = Record.Exception(() =>
            {
                var mediator = provider.GetRequiredService<IMediator>();
                mediator
                    .DirectAsync<PingRequest, PingResponse, IPingHandler>((handler, req) => handler.MyMethodAsync(req))
                    .DirectAsync<PingRequest, PingResponse, IPingHandler>((handler, req) => handler.MyMethodAsync(req));
            });

            Assert.NotNull(ex);
        }
    }
}
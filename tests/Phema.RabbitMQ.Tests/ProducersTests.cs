using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Phema.RabbitMQ.Tests
{
	public class ProducersTests
	{
		[Fact]
		public void Default()
		{
			var services = new ServiceCollection();

			services.AddRabbitMQ(options => options
					.UseConnectionUrl("amqp://test.test")
					.UseClientProvidedName("test"))
				.AddConnection("connection", connection =>
					connection.AddProducer<ProducersTests>(connection.AddDirectExchange("exchange")));

			var provider = services.BuildServiceProvider();

			var declarations = provider.GetRequiredService<IOptions<RabbitMQOptions>>().Value.ProducerDeclarations;

			var declaration = Assert.Single(declarations.Values);

			Assert.Empty(declaration.Arguments);
			Assert.False(declaration.Die);
			Assert.Equal("exchange", declaration.ExchangeDeclaration.Name);
			Assert.Equal("connection", declaration.ConnectionDeclaration.Name);
			Assert.False(declaration.Mandatory);
			Assert.Empty(declaration.Properties);
			Assert.Null(declaration.RoutingKey);
			Assert.Null(declaration.Timeout);
			Assert.False(declaration.Transactional);
			Assert.False(declaration.WaitForConfirms);
		}

		[Fact]
		public void Specified()
		{
			var services = new ServiceCollection();

			services.AddRabbitMQ(options => options
					.UseConnectionUrl("amqp://test.test")
					.UseClientProvidedName("test"))
				.AddConnection("exchanges", connection =>
					connection.AddProducer<ProducersTests>(connection.AddDirectExchange("exchange"))
						.Argument("x-argument", "argument")
						.WaitForConfirms(TimeSpan.Zero)
						.Mandatory()
						.Property(x => x.Persistent = true)
						.RoutedTo("routing_key")
						.AppId("app1")
						.Transactional());

			var provider = services.BuildServiceProvider();

			var declarations = provider.GetRequiredService<IOptions<RabbitMQOptions>>().Value.ProducerDeclarations;

			var declaration = Assert.Single(declarations.Values);

			var (key, value) = Assert.Single(declaration.Arguments);
			Assert.Equal("x-argument", key);
			Assert.Equal("argument", value);

			Assert.True(declaration.Die);
			Assert.Equal("exchange", declaration.ExchangeDeclaration.Name);
			Assert.Equal("exchanges", declaration.ConnectionDeclaration.Name);
			Assert.True(declaration.Mandatory);
			Assert.Equal(2, declaration.Properties.Count);
			Assert.Equal("routing_key", declaration.RoutingKey);
			Assert.Equal(TimeSpan.Zero, declaration.Timeout);
			Assert.True(declaration.Transactional);
			Assert.True(declaration.WaitForConfirms);
		}
	}
}
using Jaeger.Senders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Jaeger.Core.Tests.Senders
{
    public class SenderResolverTests
    {
        private const string FACTORY_NAME_TEST1 = "test1";
        private const string FACTORY_NAME_TEST2 = "test2";

        private readonly ILoggerFactory _loggerFactory;
        private readonly FlexibleSenderFactory _test1SenderFactory;
        private readonly FlexibleSenderFactory _test2SenderFactory;

        public SenderResolverTests()
        {
            _loggerFactory = NullLoggerFactory.Instance;
            _test1SenderFactory = new FlexibleSenderFactory(FACTORY_NAME_TEST1);
            _test2SenderFactory = new FlexibleSenderFactory(FACTORY_NAME_TEST2);
        }

        [Fact]
        public void TestResolveWithDefaultConfigurationWithoutFactoriesReturnsNoopSender()
        {
            var senderResolver = new SenderResolver(_loggerFactory);

            var sender = senderResolver.Resolve();
            Assert.IsType<NoopSender>(sender);
        }

        [Fact]
        public void TestResolveWithTest1SelectedWithoutFactoriesReturnsNoopSender()
        {
            var senderResolver = new SenderResolver(_loggerFactory);
            var configuration = new Configuration.SenderConfiguration(_loggerFactory)
                .WithSenderFactory(FACTORY_NAME_TEST1);

            var sender = senderResolver.Resolve(configuration);
            Assert.IsType<NoopSender>(sender);
        }

        [Fact]
        public void TestResolveWithTest1SelectedWithTest1FactoryReturnsFlexibleSender()
        {
            var configuration = new Configuration.SenderConfiguration(_loggerFactory)
                .WithSenderFactory(FACTORY_NAME_TEST1);
            var senderResolver = new SenderResolver(_loggerFactory)
                .RegisterSenderFactory(_test1SenderFactory);

            var sender = senderResolver.Resolve(configuration);
            Assert.IsType<FlexibleSenderFactory.Sender>(sender);

            var flexibleSender = (FlexibleSenderFactory.Sender)sender;
            Assert.Equal(FACTORY_NAME_TEST1, flexibleSender.FactoryName);
            Assert.Equal(configuration, flexibleSender.SenderConfiguration);
        }

        [Fact]
        public void TestResolveWithTest2SelectedWithTest1FactoryReturnsNoopSender()
        {
            var configuration = new Configuration.SenderConfiguration(_loggerFactory)
                .WithSenderFactory(FACTORY_NAME_TEST2);
            var senderResolver = new SenderResolver(_loggerFactory)
                .RegisterSenderFactory(_test1SenderFactory);

            var sender = senderResolver.Resolve(configuration);
            Assert.IsType<NoopSender>(sender);
        }

        [Fact]
        public void TestResolveWithTest1SelectedWithTest2FactoryReturnsNoopSender()
        {
            var configuration = new Configuration.SenderConfiguration(_loggerFactory)
                .WithSenderFactory(FACTORY_NAME_TEST1);
            var senderResolver = new SenderResolver(_loggerFactory)
                .RegisterSenderFactory(_test2SenderFactory);

            var sender = senderResolver.Resolve(configuration);
            Assert.IsType<NoopSender>(sender);
        }

        [Fact]
        public void TestResolveWithTest2SelectedWithTest2FactoryReturnsFlexibleSender()
        {
            var configuration = new Configuration.SenderConfiguration(_loggerFactory)
                .WithSenderFactory(FACTORY_NAME_TEST2);
            var senderResolver = new SenderResolver(_loggerFactory)
                .RegisterSenderFactory(_test2SenderFactory);

            var sender = senderResolver.Resolve(configuration);
            Assert.IsType<FlexibleSenderFactory.Sender>(sender);

            var flexibleSender = (FlexibleSenderFactory.Sender)sender;
            Assert.Equal(FACTORY_NAME_TEST2, flexibleSender.FactoryName);
            Assert.Equal(configuration, flexibleSender.SenderConfiguration);
        }
    }
}

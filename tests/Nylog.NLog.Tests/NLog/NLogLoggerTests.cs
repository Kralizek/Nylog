using System;
using System.Collections.Generic;
using Moq;
using NLog;
using NUnit.Framework;
using Nylog.NLog;
using Ploeh.AutoFixture;
using ILogger = NLog.ILogger;
using LogLevel = Nylog.LogLevel;

namespace Tests.NLog
{
    [TestFixture]
    public class NLogLoggerTests
    {
        private IFixture fixture;
        private Mock<ILogger> mockLogger;

        [SetUp]
        public void Initialize()
        {
            fixture = new Fixture();

            mockLogger = new Mock<ILogger>();
        }

        [Test]
        public void Logger_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new NLogLogger(null));
        }

        private NLogLogger CreateSystemUnderTest()
        {
            return new NLogLogger(mockLogger.Object);
        }

        public static IEnumerable<object> GetLogLevels()
        {
            yield return new object[] {LogLevel.Verbose, global::NLog.LogLevel.Trace};

            yield return new object[] {LogLevel.Debug, global::NLog.LogLevel.Debug};

            yield return new object[] {LogLevel.Information, global::NLog.LogLevel.Info};

            yield return new object[] {LogLevel.Error, global::NLog.LogLevel.Error};

            yield return new object[] {LogLevel.Critical, global::NLog.LogLevel.Fatal};

            yield return new object[] {LogLevel.Warning, global::NLog.LogLevel.Warn};

            yield return new object[] {(LogLevel) 0, global::NLog.LogLevel.Debug};
        }

        [Test]
        [TestCaseSource(nameof(GetLogLevels))]
        public void Log_levels_are_correctly_converted(LogLevel level, global::NLog.LogLevel expected)
        {
            var sut = CreateSystemUnderTest();

            var message = fixture.Create<string>();

            IDictionary<string, object> dictionary = new Dictionary<string, object>
            {
                ["message"] = message
            };

            sut.Log(level, dictionary, exception: null);

            mockLogger.Verify(p => p.Log(It.Is<LogEventInfo>(lei => lei.Level == expected)), Times.Once);
        }

        [Test]
        public void Data_is_added_as_properties()
        {
            IDictionary<string, object> dictionary = new Dictionary<string, object>
            {
                ["message"] = fixture.Create<string>(),
                ["text"] = fixture.Create<string>()
            };

            var sut = CreateSystemUnderTest();

            sut.Log(LogLevel.Information, dictionary, exception: null);

            mockLogger.Verify(p => p.Log(It.Is<LogEventInfo>(lei => string.Equals((string)lei.Properties["text"], (string)dictionary["text"]))), Times.Once);
        }

        [Test]
        public void Message_is_added_as_message()
        {
            IDictionary<string, object> dictionary = new Dictionary<string, object>
            {
                ["message"] = fixture.Create<string>(),
                ["text"] = fixture.Create<string>()
            };

            var sut = CreateSystemUnderTest();

            sut.Log(LogLevel.Information, dictionary, exception: null);

            mockLogger.Verify(p => p.Log(It.Is<LogEventInfo>(lei => string.Equals((string)dictionary["message"], lei.Message))), Times.Once);
        }

        [Test]
        public void Exception_information_are_added_to_log()
        {
            IDictionary<string, object> dictionary = new Dictionary<string, object>
            {
                ["message"] = fixture.Create<string>(),
                ["text"] = fixture.Create<string>()
            };

            var sut = CreateSystemUnderTest();

            try
            {
                Exception innerException = new Exception("This is an inner exception");
                throw new Exception("This is a test exception", innerException);
            }
            catch (Exception ex)
            {
                sut.Log(LogLevel.Error, dictionary, ex);
            }

            mockLogger.Verify(p => p.Log(It.Is<LogEventInfo>(lei => string.Equals((string)dictionary["message"], lei.Message))), Times.Once);
            mockLogger.Verify(p => p.Log(It.Is<LogEventInfo>(lei => string.Equals((string)lei.Properties["error-method"], nameof(Exception_information_are_added_to_log)))), Times.Once);
        }

        [Test]
        [TestCaseSource(nameof(GetLogLevels))]
        public void IsEnabled_forwards_to_NLog_logger(LogLevel level, global::NLog.LogLevel expected)
        {
            var sut = CreateSystemUnderTest();

            sut.IsEnabled(level);

            mockLogger.Verify(p => p.IsEnabled(expected), Times.Once);
        }
    }
}

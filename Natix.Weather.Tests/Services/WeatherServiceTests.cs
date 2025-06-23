using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Moq;
using Polly;
using Natix.Weather.Application.Interfaces;
using Natix.Weather.Application.Services;
using Natix.Weather.Domain.Queries;
using Natix.Weather.Infrastructure.Interfaces;
using Natix.Weather.Domain.Commands;

namespace Natix.Weather.Tests.Services;

public class WeatherServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IWeatherQueryRepository> _readerMock;
    private readonly Mock<IWeatherCommandRepository> _writerMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IWeatherProviderFactory> _providerFactoryMock;
    private readonly Mock<IWeatherProvider> _providerMock;
    private readonly IAsyncPolicy<WeatherDto> _resilience;

    private readonly IWeatherService _service;

    public WeatherServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _readerMock = _fixture.Freeze<Mock<IWeatherQueryRepository>>();
        _writerMock = _fixture.Freeze<Mock<IWeatherCommandRepository>>();
        _unitOfWorkMock = _fixture.Freeze<Mock<IUnitOfWork>>();
        _providerFactoryMock = _fixture.Freeze<Mock<IWeatherProviderFactory>>();
        _providerMock = new Mock<IWeatherProvider>();

        // Use a no-op Polly policy for tests
        _resilience = Policy.NoOpAsync<WeatherDto>();

        _providerFactoryMock.Setup(f => f.Create("default")).Returns(_providerMock.Object);


        _service = new WeatherService(
            _readerMock.Object,
            _writerMock.Object,
            _unitOfWorkMock.Object,
            _providerFactoryMock.Object,
            _resilience
        );
    }

    [Fact]
    public async Task Should_Return_Weather_From_Cache_If_Found()
    {
        // Arrange
        var city = "berlin";
        var today = DateTime.UtcNow.Date;
        var cached = _fixture.Create<WeatherDto>();

        _readerMock
            .Setup(r => r.GetAsync(city, today))
            .ReturnsAsync(cached);

        // Act
        var result = await _service.GetTodayWeatherAsync(city);

        // Assert
        result.Should().BeEquivalentTo(cached);
        _providerFactoryMock.Verify(f => f.Create(It.IsAny<string>()), Times.Never);
        _providerMock.Verify(p => p.FetchTodayWeatherAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Should_Fetch_From_Provider_And_Save_When_Not_In_Cache()
    {
        // Arrange
        var city = "berlin";
        var today = DateTime.UtcNow.Date;

        var fetchedDto = _fixture.Build<WeatherDto>()
            .With(d => d.City, city)
            .With(d => d.Date, today)
            .Create();

        _readerMock
            .Setup(r => r.GetAsync(city, today))
            .ReturnsAsync((WeatherDto?)null);

        _providerMock
            .Setup(p => p.FetchTodayWeatherAsync(city))
            .ReturnsAsync(fetchedDto);

        WeatherPayload? capturedPayload = null;

        _writerMock
            .Setup(w => w.SaveAsync(city, today, It.IsAny<WeatherPayload>()))
            .Callback<string, DateTime, WeatherPayload>((_, _, payload) => capturedPayload = payload);

        // Act
        var result = await _service.GetTodayWeatherAsync(city);

        // Assert
        result.Should().BeEquivalentTo(fetchedDto);
        _writerMock.Verify(w => w.SaveAsync(city, today, It.IsAny<WeatherPayload>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);

        capturedPayload.Should().NotBeNull();
        capturedPayload!.City.Should().Be(city);
        capturedPayload!.FetchedAt.Date.Should().Be(today);
        capturedPayload.Hours.Should().BeEquivalentTo(fetchedDto.Weather);
    }
}

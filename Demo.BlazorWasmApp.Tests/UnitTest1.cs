using Bunit;
using Demo.BlazorWasmApp.Pages;
using System.Diagnostics.Metrics;
using Xunit;

namespace Demo.BlazorWasmApp.Tests
{
    public class CounterComponentTests : TestContext
    {
        [Fact]
        public void CounterStartsAtZero()
        {
            // Arrange
            var cut = Render<Counter>();

            // Act
            var initialCount = cut.Find("p").TextContent;

            // Assert
            Assert.Equal("Current count: 0", initialCount);
        }

        [Fact]
        public void ClickingButtonIncrementsCounter()
        {
            // Arrange
            var cut = Render<Counter>();

            // Act
            cut.Find("button").Click();
            var incrementedCount = cut.Find("p").TextContent;

            // Assert
            Assert.Equal("Current count: 1", incrementedCount);
        }
    }
}

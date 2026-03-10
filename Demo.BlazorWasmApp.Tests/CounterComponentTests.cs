// <copyright file="CounterComponentTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Demo.BlazorWasmApp.Tests;

using Bunit;
using Demo.BlazorWasmApp.Pages;
using Xunit;

/// <summary>
/// Tests for the Counter component.
/// </summary>
public class CounterComponentTests : TestContext
{
    /// <summary>
    /// Verifies that the counter starts at zero.
    /// </summary>
    [Fact]
    public void CounterStartsAtZero()
    {
        // Arrange
        using var cut = this.RenderComponent<Counter>();

        // Act
        var initialCount = cut.Find("p").TextContent;

        // Assert
        Assert.Equal("Current count: 0", initialCount);
    }

    /// <summary>
    /// Verifies that clicking the button increments the counter.
    /// </summary>
    [Fact]
    public void ClickingButtonIncrementsCounter()
    {
        // Arrange
        using var cut = this.RenderComponent<Counter>();

        // Act
        cut.Find("button").Click();
        var incrementedCount = cut.Find("p").TextContent;

        // Assert
        Assert.Equal("Current count: 1", incrementedCount);
    }
}

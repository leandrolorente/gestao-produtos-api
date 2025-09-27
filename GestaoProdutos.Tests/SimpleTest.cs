using Xunit;
using FluentAssertions;

namespace GestaoProdutos.Tests;

public class SimpleTest
{
    [Fact]
    public void SimpleTest_ShouldPass()
    {
        // Arrange
        var result = true;

        // Act & Assert
        result.Should().BeTrue();
    }
}
using WorkTracker.Application.Common;

namespace WorkTracker.UnitTests.Application.Common;

[TestFixture]
public class ResultTests
{
    [Test]
    public void Success_ShouldCreateSuccessfulResultWithoutError()
    {
        var result = Result.Success();

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.ErrorMessage, Is.Null);
    }

    [Test]
    public void Failure_ShouldCreateFailedResultWithErrorMessage()
    {
        var result = Result.Failure("Something went wrong");

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Is.EqualTo("Something went wrong"));
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null)]
    public void Failure_ShouldThrow_WhenErrorMessageIsNullOrWhitespace(string? errorMessage)
    {
        Assert.Throws<ArgumentException>(() => Result.Failure(errorMessage!));
    }
}

[TestFixture]
public class ResultOfTTests
{
    [Test]
    public void Success_ShouldCreateSuccessfulResultWithValue()
    {
        var result = Result<string>.Success("hello");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("hello"));
        Assert.That(result.ErrorMessage, Is.Null);
    }

    [Test]
    public void Failure_ShouldCreateFailedResultWithoutValue()
    {
        var result = Result<string>.Failure("Something went wrong");

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Value, Is.Null);
        Assert.That(result.ErrorMessage, Is.EqualTo("Something went wrong"));
    }

    [Test]
    public void Success_ShouldAllowNullValue_ForReferenceTypes()
    {
        var result = Result<string?>.Success(null);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Null);
    }
}

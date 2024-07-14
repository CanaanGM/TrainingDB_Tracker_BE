﻿using DataLibrary.Helpers;

using System.Globalization;

namespace DateLibraryTests;
public class UtilsTests
{

    [Theory]
    [InlineData("Wide Grip Pullup")]
    [InlineData("  DeadLifts ")]
    [InlineData("T-BAR ROWS")]
    [InlineData("P@ssW0rd!")]
    [InlineData("\nc\nT\tx\t")]
    public void NormalizeString_correct_input_returns_Sucess(string @string)
    {
        var normalizedString = Utils.NormalizeString(@string);
        Assert.NotEqual(@string, normalizedString);
        Assert.Equal(normalizedString, CultureInfo.CurrentCulture.TextInfo.ToLower(@string.Trim()));
    }

    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void NormalizeString_incorrect_input_returns_Failure(string @string)
    {
        Assert.Throws<ArgumentException>(() => Utils.NormalizeString(@string));
    }

    [Theory] // how can i test incorrect input here?
    [InlineData("5-13-2024")]
    [InlineData("5-13-2024 17:34:45")]

    public void ParseDate_returns_normalized_string_Success(string dateString)
    {
        var parsedDate = Utils.ParseDate(dateString);
        Assert.NotNull(parsedDate);
        Assert.Equal(DateTime.Parse(dateString), parsedDate);
    }

    [Theory]
    [InlineData(null)]
    public void ParseDate_null_returns_datetime_Success(string? dateString)
    {
        var parsedDate = Utils.ParseDate(dateString);
        Assert.Null(parsedDate);
        Assert.True(parsedDate is null);
    }

    [Theory]
    [InlineData(120)]
    [InlineData(60)]
    [InlineData(240)]
    public void DurationSecondsFromMinutes_returns_int_success(int minutes)
    {
        var durationInSeconds = Utils.DurationSecondsFromMinutes(minutes);
        Assert.Equal(minutes * 60, durationInSeconds);
        Assert.Equal(minutes, durationInSeconds / 60);
    }

    [Theory]
    [InlineData(null)]
    public void DurationSecondsFromMinutes_null_returns_null_success(int? minutes)
    {
        var durationInSeconds = Utils.DurationSecondsFromMinutes(minutes);
        Assert.Null(durationInSeconds);
        Assert.True(durationInSeconds is null);
    }

    [Fact]
    public void NormalizeDtoNames_reuturns_normalized_list_of_strings()
    {
        var listOfStrings = new List<string> { "Wide Grip Pull-Up", "Dragon Flag", "DeadLift", "DeadLift", "BenchPress" };

        var normalizedStrings = Utils.NormalizeStringList(listOfStrings);
        Assert.NotNull(normalizedStrings);
        Assert.Equal(listOfStrings.Distinct().Count(), normalizedStrings.Count());
        Assert.NotEqual(listOfStrings.Count(), normalizedStrings.Count());

        // since they're duplicates, all of them should be in the normalized list.
        foreach (var unString in listOfStrings.Select(x => Utils.NormalizeString(x)).ToList())
            Assert.Contains(unString, normalizedStrings);

    }
}
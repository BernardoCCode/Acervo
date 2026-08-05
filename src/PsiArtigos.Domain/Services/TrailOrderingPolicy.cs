using PsiArtigos.Domain.Enums;
using PsiArtigos.Domain.Exceptions;

namespace PsiArtigos.Domain.Services;

public static class TrailOrderingPolicy
{
    /// <summary>
    /// Suggested progression for AI learning trails: beginner → intermediate → advanced → classic → recent.
    /// </summary>
    public static IReadOnlyList<DifficultyLevel> SuggestedProgression { get; } =
    [
        DifficultyLevel.Beginner,
        DifficultyLevel.Intermediate,
        DifficultyLevel.Advanced,
        DifficultyLevel.Classic,
        DifficultyLevel.RecentResearch
    ];

    public static int Rank(DifficultyLevel difficulty)
    {
        var index = SuggestedProgression.ToList().IndexOf(difficulty);
        if (index < 0)
            throw new DomainException("Unknown difficulty level for trail ordering.");

        return index;
    }

    public static bool IsValidProgression(IEnumerable<DifficultyLevel> difficulties)
    {
        var ranks = difficulties.Select(Rank).ToList();

        for (var i = 1; i < ranks.Count; i++)
        {
            if (ranks[i] < ranks[i - 1])
                return false;
        }

        return true;
    }
}

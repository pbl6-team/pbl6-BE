using Microsoft.AspNetCore.Http;

namespace PBL6.Common.Functions;

public static class CommonFunctions
{
    public static string GetFileExtension(string fileName) => Path.GetExtension(fileName);

    public static async Task<byte[]> GetBytesAsync(this IFormFile formFile)
    {
        using var memoryStream = new MemoryStream();
        await formFile.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    public static Dictionary<string, int> GetReactionCount(IEnumerable<string> reactions)
    {
        var reactionCount = new Dictionary<string, int>();
        foreach (var reaction in reactions)
        {
            if (string.IsNullOrEmpty(reaction))
            {
                continue;
            }
            foreach (var item in reaction.Split(" "))
            {
                if (string.IsNullOrWhiteSpace(item))
                {
                    continue;
                }
                if (reactionCount.ContainsKey(item))
                {
                    reactionCount[item]++;
                }
                else
                {
                    reactionCount.Add(item, 1);
                }
            }
        }

        return reactionCount;
    }
}
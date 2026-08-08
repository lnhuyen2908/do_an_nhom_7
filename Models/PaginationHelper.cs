namespace web_do_an1.Models;

public static class PaginationHelper
{
    public const int Ellipsis = -1;

    public static IReadOnlyList<int> Build(int currentPage, int totalPages)
    {
        if (totalPages <= 7)
        {
            return Enumerable.Range(1, Math.Max(totalPages, 1)).ToList();
        }

        currentPage = Math.Clamp(currentPage, 1, totalPages);
        var pages = new SortedSet<int> { 1, totalPages };

        for (var page = currentPage - 1; page <= currentPage + 1; page++)
        {
            if (page > 1 && page < totalPages)
            {
                pages.Add(page);
            }
        }

        if (currentPage <= 3)
        {
            pages.Add(2);
            pages.Add(3);
        }

        if (currentPage >= totalPages - 2)
        {
            pages.Add(totalPages - 2);
            pages.Add(totalPages - 1);
        }

        var result = new List<int>();
        var previous = 0;
        foreach (var page in pages.Where(x => x >= 1 && x <= totalPages))
        {
            if (previous > 0 && page - previous > 1)
            {
                result.Add(Ellipsis);
            }

            result.Add(page);
            previous = page;
        }

        return result;
    }
}

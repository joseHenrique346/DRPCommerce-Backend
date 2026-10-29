using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public static class CategoryRules
{
    public static Error? ValidateChanges(
        IReadOnlyCollection<Category> current,
        IReadOnlyDictionary<long, Category> updates,
        IReadOnlyList<Category> additions)
    {
        var original = current.ToDictionary(Category => Category.Id);
        var proposed = new Dictionary<long, Category>(original);

        foreach (var (id, category) in updates)
        {
            proposed[id] = category;

            // Evita trocas de slug que violariam o índice único durante os UPDATEs intermediários.
            if (current.Any(Category => Category.Id != id && Category.Slug == category.Slug))
                return Error.Conflict("Category.DuplicateSlug", "Já existe uma categoria com este slug na loja.");
        }

        for (var index = 0; index < additions.Count; index++)
            proposed[-index - 1L] = additions[index];

        if (proposed.Values.GroupBy(Category => Category.Slug, StringComparer.Ordinal).Any(Group => Group.Count() > 1))
            return Error.Conflict("Category.DuplicateSlug", "Já existe uma categoria com este slug na loja.");

        foreach (var (id, category) in proposed)
        {
            var changedParent = !original.TryGetValue(id, out var previous) || previous.ParentCategoryId != category.ParentCategoryId;

            if (changedParent && category.ParentCategoryId is long parentId &&
                (!proposed.TryGetValue(parentId, out var parent) || !parent.IsActive || parent.EnterpriseId != category.EnterpriseId))
                return Error.Validation("Category.InvalidParent", "A categoria pai deve existir, estar ativa e pertencer à mesma loja.");
        }

        return ValidateTopology(proposed);
    }

    public static Error? ValidateTopology(IReadOnlyDictionary<long, Category> categories)
    {
        foreach (var (id, category) in categories)
        {
            var visited = new HashSet<long> { id };
            var current = category;
            var depth = 1;

            while (current.ParentCategoryId is long parentId)
            {
                if (!visited.Add(parentId))
                    return Error.Validation("Category.Cycle", "A categoria não pode ser filha de si mesma ou de um descendente.");

                if (!categories.TryGetValue(parentId, out var parent) || parent.EnterpriseId != category.EnterpriseId)
                    return Error.Validation("Category.InvalidParent", "A categoria pai deve existir e pertencer à mesma loja.");

                depth++;

                if (depth > Category.MaximumDepth)
                    return Error.Validation("Category.MaximumDepth", $"A hierarquia permite no máximo {Category.MaximumDepth} níveis.");

                current = parent;
            }
        }

        return null;
    }

    public static List<Category> VisibleCategories(IEnumerable<Category> categories)
    {
        var byId = categories.Where(Category => !Category.IsDeleted).ToDictionary(Category => Category.Id);

        return byId.Values.Where(Category => IsVisible(Category, byId)).ToList();
    }

    private static bool IsVisible(Category category, IReadOnlyDictionary<long, Category> categories)
    {
        var visited = new HashSet<long>();
        var current = category;

        while (true)
        {
            if (!current.IsActive || current.EnterpriseId != category.EnterpriseId || !visited.Add(current.Id) ||
                visited.Count > Category.MaximumDepth)
                return false;

            if (current.ParentCategoryId is not long parentId)
                return true;

            if (!categories.TryGetValue(parentId, out current!))
                return false;
        }
    }
}

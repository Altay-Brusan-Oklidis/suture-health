using Kendo.Mvc;

namespace SutureHealth.AspNetCore.Mvc.Extensions
{
    public static class KendoExtensions
    {
        public static bool Contains(this IList<IFilterDescriptor> filters, params string[] member)
        {
            if (filters.Any())
            {
                foreach (var filter in filters)
                {
                    switch (filter)
                    {
                        case FilterDescriptor descriptor:
                            if (member.Any(m => string.Equals(descriptor.Member, m, StringComparison.OrdinalIgnoreCase)))
                                return true;
                            break;
                        case CompositeFilterDescriptor composite:
                            if (Contains(composite.FilterDescriptors, member))
                                return true;
                            break;
                    }
                }
            }

            return false;
        }

        public static FilterDescriptor GetFilterDescriptor(this IList<IFilterDescriptor> filters, string member)
        {
            if (filters.Any())
            {
                foreach (var filter in filters)
                {
                    switch (filter)
                    {
                        case FilterDescriptor descriptor:
                            if (string.Equals(descriptor.Member, member, StringComparison.OrdinalIgnoreCase))
                                return descriptor;
                            break;
                        case CompositeFilterDescriptor composite:
                            var _filter = GetFilterDescriptor(composite.FilterDescriptors, member);
                            if (_filter != null)
                                return _filter;
                            break;
                    }
                }
            }

            return null;
        }

        public static void Remove(this IList<IFilterDescriptor> filters, string member)
        {
            if (filters.Any())
            {
                List<IFilterDescriptor> filtersToRemove = new List<IFilterDescriptor>();

                foreach (var filter in filters)
                {
                    switch (filter)
                    {
                        case FilterDescriptor descriptor:
                            if (string.Equals(descriptor.Member, member, StringComparison.OrdinalIgnoreCase))
                                filtersToRemove.Add(filter);
                            break;
                        case CompositeFilterDescriptor composite:
                            Remove(composite.FilterDescriptors, member);
                            break;
                    }
                }
                foreach (var filter in filtersToRemove)
                {
                    filters.Remove(filter);
                }
            }
        }

        public static void RemoveAll(this IList<IFilterDescriptor> filters, IFilterDescriptor item)
        {
            if (filters.Any())
            {
                foreach (var composite in filters.OfType<CompositeFilterDescriptor>())
                    RemoveAll(composite.FilterDescriptors, item);

                filters.Remove(item);
            }
        }

        public static void Transform(this IList<IFilterDescriptor> filters, string original, string replacement, Action<FilterDescriptor> conversion = null)
        {
            if (filters.Any())
            {
                foreach (var filter in filters)
                {
                    switch (filter)
                    {
                        case FilterDescriptor descriptor:
                            if (descriptor != null && string.Equals(descriptor.Member, original, StringComparison.OrdinalIgnoreCase))
                            {
                                descriptor.Member = replacement;
                                if (conversion != null) conversion(descriptor);
                            }
                            break;
                        case CompositeFilterDescriptor composite:
                            Transform(((CompositeFilterDescriptor)filter).FilterDescriptors, original, replacement, conversion);
                            break;
                    }
                }
            }
        }

        public static void Transform(this IList<SortDescriptor> sorts, string memberName, Action<SortDescriptor> conversion) =>
            sorts.Where(s => string.Equals(s.Member, memberName, StringComparison.OrdinalIgnoreCase))
                 .ToList()
                 .ForEach(s => conversion(s));
    }
}

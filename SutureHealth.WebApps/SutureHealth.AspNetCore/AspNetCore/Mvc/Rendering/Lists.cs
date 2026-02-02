using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SutureHealth.AspNetCore.Mvc.Rendering
{
    public static class Lists
    {
        public static IEnumerable<SelectListItem> States =>
            Enum.GetValues(typeof(States)).Cast<States>().Select(s => s == System.Globalization.States.None ? new SelectListItem(string.Empty, string.Empty) : new SelectListItem(s.ToString(), s.ToString()));

        public static IEnumerable<SelectListItem> Suffixes =>
            Enum.GetValues(typeof(Suffixes)).Cast<Suffixes>().Select(s => s == SutureHealth.Suffixes.None ? new SelectListItem(string.Empty, string.Empty) : new SelectListItem(s.GetEnumDescription(), s.GetEnumDescription()));
    }
}

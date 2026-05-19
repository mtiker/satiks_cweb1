using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace WebApp.TagHelpers;

[ExcludeFromCodeCoverage]
[HtmlTargetElement("status-badge")]
public class StatusBadgeTagHelper : TagHelper
{
    public string Status { get; set; } = string.Empty;

    private static readonly Dictionary<string, string> ColorMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Active"]          = "success",
            ["Available"]       = "success",
            ["Passed"]          = "success",
            ["Valid"]           = "success",
            ["Completed"]       = "success",
            ["Approved"]        = "success",
            ["New"]             = "primary",
            ["Good"]            = "success",
            ["Fair"]            = "warning",
            ["NeedsRepair"]     = "danger",
            ["Decommissioned"]  = "secondary",
            ["Yes"]             = "success",
            ["No"]              = "secondary",
            ["Inactive"]        = "secondary",
            ["Unavailable"]     = "secondary",
            ["Expired"]         = "secondary",
            ["Cancelled"]       = "secondary",
            ["Pending"]         = "warning",
            ["Expiring"]        = "warning",
            ["UnderReview"]     = "warning",
            ["Scheduled"]       = "info",
            ["InProgress"]      = "primary",
            ["InUse"]           = "primary",
            ["Failed"]          = "danger",
            ["Overdue"]         = "danger",
            ["OutOfService"]    = "danger",
            ["Rejected"]        = "danger",
        };

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var color = ColorMap.GetValueOrDefault(Status, "secondary");
        output.TagName = "span";
        output.Attributes.SetAttribute("class", $"badge bg-{color}");
        output.Content.SetContent(Status);
    }
}

﻿using ColorCode.Styling;
using Markdig;

namespace Markdown.ColorCode;

/// <summary>
///     Extensions for adding ColorCode to the Markdig pipeline.
/// </summary>
public static class MarkdownPipelineBuilderExtensions
{
    /// <summary>
    ///     Use ColorCode to apply code colorization to markdown within the Markdig pipeline.
    /// </summary>
    /// <param name="pipeline">The pipeline the ColorCode extension is being added to.</param>
    /// <param name="styleDictionary">An optional StyleDictionary for custom styling.</param>
    /// <returns>The MarkdownPipelineBuilder with the added ColorCode extension.</returns>
    public static MarkdownPipelineBuilder UseColorCode(
        this MarkdownPipelineBuilder pipeline,
        StyleDictionary? styleDictionary = null)
    {
        pipeline.Extensions.Add(new ColorCodeExtension(styleDictionary ?? StyleDictionary.DefaultLight));

        return pipeline;
    }
}

using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using AlJabai.Core.Models;

namespace AlJabai.Infrastructure.Services;

public interface IWordTemplateParser
{
    Task<string> ExtractAllTextAsync(Stream docxStream);
    Task<WordParseResult> ParseVariablesAsync(Stream docxStream);
    Task<byte[]> FillTemplateAsync(Stream docxStream, Dictionary<string, string> values);
}

public class WordParseResult
{
    public List<string> FoundVariables { get; set; } = new();
    public List<string> KnownVariables { get; set; } = new();
    public List<string> MissingVariables { get; set; } = new();
    public List<string> CustomVariables { get; set; } = new();
    public int TotalVariableOccurrences { get; set; }
    public bool IsValid => FoundVariables.Count > 0;
    public string ValidationMessage { get; set; } = string.Empty;
}

public class WordTemplateParser : IWordTemplateParser
{
    private static readonly Regex VariableRegex = new(@"\{\{([a-z_]+)\}\}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public async Task<string> ExtractAllTextAsync(Stream docxStream)
    {
        if (docxStream == null)
        {
            throw new ArgumentNullException(nameof(docxStream));
        }

        if (docxStream.CanSeek)
        {
            docxStream.Seek(0, SeekOrigin.Begin);
        }

        using var copyStream = new MemoryStream();
        await docxStream.CopyToAsync(copyStream);
        copyStream.Seek(0, SeekOrigin.Begin);

        using var document = WordprocessingDocument.Open(copyStream, false);
        var chunks = new List<string>();

        chunks.AddRange(ExtractParagraphText(document.MainDocumentPart?.Document?.Body));

        if (document.MainDocumentPart != null)
        {
            foreach (var header in document.MainDocumentPart.HeaderParts)
            {
                chunks.AddRange(ExtractParagraphText(header.Header));
            }

            foreach (var footer in document.MainDocumentPart.FooterParts)
            {
                chunks.AddRange(ExtractParagraphText(footer.Footer));
            }
        }

        return string.Join(Environment.NewLine, chunks.Where(s => !string.IsNullOrWhiteSpace(s)));
    }

    public async Task<WordParseResult> ParseVariablesAsync(Stream docxStream)
    {
        var result = new WordParseResult();
        var text = await ExtractAllTextAsync(docxStream);
        var matches = VariableRegex.Matches(text);

        result.TotalVariableOccurrences = matches.Count;

        var unique = matches
            .Select(m => m.Groups[1].Value.Trim().ToLowerInvariant())
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(v => v)
            .ToList();

        result.FoundVariables = unique;
        result.KnownVariables = unique.Where(TemplateVariables.IsKnown).OrderBy(v => v).ToList();
        result.CustomVariables = unique.Where(v => !TemplateVariables.IsKnown(v)).OrderBy(v => v).ToList();
        result.MissingVariables = TemplateVariables.All
            .Select(v => v.Key)
            .Where(v => !result.KnownVariables.Contains(v, StringComparer.OrdinalIgnoreCase))
            .OrderBy(v => v)
            .ToList();

        result.ValidationMessage = result.IsValid
            ? $"تم العثور على {result.TotalVariableOccurrences} استخدام متغير"
            : "لم يتم العثور على أي متغيرات من الصيغة {{variable_name}}";

        return result;
    }

    public async Task<byte[]> FillTemplateAsync(Stream docxStream, Dictionary<string, string> values)
    {
        if (docxStream == null)
        {
            throw new ArgumentNullException(nameof(docxStream));
        }

        var normalizedValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in values)
        {
            normalizedValues[pair.Key] = pair.Value ?? string.Empty;
        }

        using var output = new MemoryStream();
        if (docxStream.CanSeek)
        {
            docxStream.Seek(0, SeekOrigin.Begin);
        }

        await docxStream.CopyToAsync(output);
        output.Seek(0, SeekOrigin.Begin);

        using (var document = WordprocessingDocument.Open(output, true))
        {
            ReplaceInParagraphContainer(document.MainDocumentPart?.Document?.Body, normalizedValues);

            if (document.MainDocumentPart != null)
            {
                foreach (var header in document.MainDocumentPart.HeaderParts)
                {
                    ReplaceInParagraphContainer(header.Header, normalizedValues);
                    header.Header?.Save();
                }

                foreach (var footer in document.MainDocumentPart.FooterParts)
                {
                    ReplaceInParagraphContainer(footer.Footer, normalizedValues);
                    footer.Footer?.Save();
                }
            }

            document.MainDocumentPart?.Document?.Save();
        }

        return output.ToArray();
    }

    private static IEnumerable<string> ExtractParagraphText(OpenXmlElement? root)
    {
        if (root == null)
        {
            return [];
        }

        var result = new List<string>();
        foreach (var paragraph in root.Descendants<Paragraph>())
        {
            var text = string.Concat(paragraph.Descendants<Text>().Select(t => t.Text));
            if (!string.IsNullOrWhiteSpace(text))
            {
                result.Add(text);
            }
        }

        foreach (var textBox in root.Descendants<TextBoxContent>())
        {
            foreach (var paragraph in textBox.Descendants<Paragraph>())
            {
                var text = string.Concat(paragraph.Descendants<Text>().Select(t => t.Text));
                if (!string.IsNullOrWhiteSpace(text))
                {
                    result.Add(text);
                }
            }
        }

        return result;
    }

    private static void ReplaceInParagraphContainer(OpenXmlElement? root, IReadOnlyDictionary<string, string> values)
    {
        if (root == null)
        {
            return;
        }

        foreach (var paragraph in root.Descendants<Paragraph>())
        {
            ReplaceInParagraph(paragraph, values);
        }
    }

    private static void ReplaceInParagraph(Paragraph paragraph, IReadOnlyDictionary<string, string> values)
    {
        var runs = paragraph.Elements<Run>().ToList();
        if (runs.Count == 0)
        {
            return;
        }

        var fullText = string.Concat(runs.SelectMany(r => r.Descendants<Text>()).Select(t => t.Text));
        if (string.IsNullOrEmpty(fullText))
        {
            return;
        }

        var matches = VariableRegex.Matches(fullText);
        if (matches.Count == 0)
        {
            return;
        }

        var firstRunProperties = runs.FirstOrDefault(r => r.RunProperties != null)?.RunProperties?.CloneNode(true) as RunProperties;

        foreach (var run in runs)
        {
            run.Remove();
        }

        var cursor = 0;
        foreach (Match match in matches)
        {
            if (match.Index > cursor)
            {
                var literal = fullText[cursor..match.Index];
                AppendText(paragraph, literal, firstRunProperties);
            }

            var key = match.Groups[1].Value;
            var originalPlaceholder = match.Value;
            var replacement = values.TryGetValue(key, out var value) ? value : originalPlaceholder;
            AppendText(paragraph, replacement, firstRunProperties);

            cursor = match.Index + match.Length;
        }

        if (cursor < fullText.Length)
        {
            AppendText(paragraph, fullText[cursor..], firstRunProperties);
        }
    }

    private static void AppendText(Paragraph paragraph, string value, RunProperties? runProperties)
    {
        var normalized = value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal);
        var lines = normalized.Split('\n');

        for (var i = 0; i < lines.Length; i++)
        {
            var run = new Run();
            if (runProperties != null)
            {
                run.RunProperties = (RunProperties)runProperties.CloneNode(true);
            }

            if (!string.IsNullOrEmpty(lines[i]))
            {
                run.AppendChild(new Text(lines[i]) { Space = SpaceProcessingModeValues.Preserve });
            }

            paragraph.AppendChild(run);

            if (i < lines.Length - 1)
            {
                var breakRun = new Run();
                if (runProperties != null)
                {
                    breakRun.RunProperties = (RunProperties)runProperties.CloneNode(true);
                }

                breakRun.AppendChild(new Break());
                paragraph.AppendChild(breakRun);
            }
        }
    }
}
